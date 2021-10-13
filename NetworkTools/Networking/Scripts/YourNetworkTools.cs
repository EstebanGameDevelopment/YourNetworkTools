using System;
using System.Collections.Generic;
using UnityEngine;
using YourCommonTools;
#if ENABLE_PHOTON
using Photon.Pun;
#endif

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * YourNetworkTools
	 * 
	 * Interface to include the normal GameObject that the programmer
	 * wants to be Network managed and other configurations
	 *
	 * @author Esteban Gallardo
	 */
	public class YourNetworkTools : MonoBehaviour
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_YOURNETWORKTOOLS_NETID_NEW = "EVENT_YOURNETWORKTOOLS_NETID_NEW";
        public const string EVENT_YOURNETWORKTOOLS_CREATED_GAMEOBJECT = "EVENT_YOURNETWORKTOOLS_CREATED_GAMEOBJECT";
        public const string EVENT_YOURNETWORKTOOLS_DESTROYED_GAMEOBJECT = "EVENT_YOURNETWORKTOOLS_DESTROYED_GAMEOBJECT";
		public const string EVENT_YOURNETWORKTOOLS_INITIALITZATION_DATA = "EVENT_YOURNETWORKTOOLS_INITIALITZATION_DATA";

		public const string COOCKIE_IS_LOCAL_GAME = "COOCKIE_IS_LOCAL_GAME";
		public const char TOKEN_SEPARATOR_NAME = '_';

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static YourNetworkTools instance;

		public static YourNetworkTools Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(YourNetworkTools)) as YourNetworkTools;
				}
				return instance;
			}
		}
		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public bool IsLocalGame = true;
        public float TimeToUpdateTransforms = 0.2f;
		public GameObject[] LocalNetworkPrefabManagers;
		public GameObject NetworkVariablesManager;
		public GameObject[] GameObjects;
        public float TimeToUpdateNetworkedObjects = 0.2f;

#if !DISABLE_UNET_COMMS
        private List<NetworkWorldObject> m_unetNetworkObjects = new List<NetworkWorldObject>();
#endif
		private List<GameObject> m_tcpNetworkObjects = new List<GameObject>();
		private List<GameObject> m_tcpNetworkTypes = new List<GameObject>();
		private Dictionary<string, string> m_initialData = new Dictionary<string, string>();
		private float m_timeoutUpdateRemoteNetworkObject = 0;

		private bool m_activateTransformUpdate = false;
		private int m_networkIDReceived = -1;

		private int m_uidCounter = 0;
		private bool m_hasBeenInitialized = false;

        private bool m_enabledPhotonEngine = false;

        public bool IsServer
		{
			get
			{
				if (IsLocalGame)
				{
                    if (MultiplayerConfiguration.LoadNumberOfPlayers() != 1)
                    {
                        return CommunicationsController.Instance.IsServer;
                    }
                    else
                    {
                        return true;
                    }
                }
				else
				{
#if ENABLE_PHOTON
                    return PhotonController.Instance.IsServer();
#elif ENABLE_NAKAMA
					return NakamaController.Instance.IsServer();
#else
                    return ClientTCPEventsController.Instance.IsServer();
#endif
				}
			}
		}
		public bool ActivateTransformUpdate
		{
			get { return m_activateTransformUpdate; }
			set { m_activateTransformUpdate = value; }
		}
		public bool HasBeenInitialized
		{
			get { return m_hasBeenInitialized; }
		}

        // -------------------------------------------
        /* 
		 * Stores in the coockie if it's a local game
		 */
        public static void SetLocalGame(bool _isLocalGame)
		{
			PlayerPrefs.SetInt(COOCKIE_IS_LOCAL_GAME, (_isLocalGame ? -1 : 1));
		}

        // -------------------------------------------
        /* 
		 * Stores in the coockie if it's a local game
		 */
        public static bool GetIsLocalGame()
		{
			return (PlayerPrefs.GetInt(COOCKIE_IS_LOCAL_GAME, -1000) == -1);
		}

        // -------------------------------------------
        /* 
        * Converts the normal GameObjects in Network GameObjects
        */
        void Start()
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);

			int isLocalGame = PlayerPrefs.GetInt(COOCKIE_IS_LOCAL_GAME, -1);
			if (isLocalGame == -1)
			{
				IsLocalGame = true;
			}
			else
			{
				IsLocalGame = false;
			}

			if (IsLocalGame)
			{
                if (MultiplayerConfiguration.LoadNumberOfPlayers() != 1)
                {
                    // INSTANTIATE LOCAL NETWORK PREFAB MANAGERS
                    for (int j = 0; j < LocalNetworkPrefabManagers.Length; j++)
                    {
                        Utilities.AddChild(transform, LocalNetworkPrefabManagers[j]);
                    }

                    // NETWORK VARIABLES MANAGER
                    Utilities.AddChild(transform, NetworkVariablesManager);

                    // ASSIGN THE GAME OBJECTS TO THE CONTROLLER
                    WorldObjectController worldObjectController = GameObject.FindObjectOfType<WorldObjectController>();
                    if (worldObjectController != null)
                    {
						int totalNumberLocalObjects = 0;
						for (int i = 0; i < GameObjects.Length; i++)
						{
							if (GameObjects[i].GetComponent<NetworkWorldObjectData>() != null)
                            {
								totalNumberLocalObjects++;
							}
						}

						int counterAppWorldObjects = 0;
						worldObjectController.AppWorldObjects = new GameObject[totalNumberLocalObjects];
                        for (int i = 0; i < GameObjects.Length; i++)
                        {
                            GameObject prefabToNetwork = GameObjects[i];
							if (prefabToNetwork.GetComponent<NetworkWorldObjectData>() != null)
                            {
								prefabToNetwork.GetComponent<NetworkWorldObjectData>().enabled = true;
#if ENABLE_MIRROR
								GameObject.FindObjectOfType<MirrorDiscoveryController>().spawnPrefabs.Add(prefabToNetwork);
#endif
								if (prefabToNetwork.GetComponent<NetworkID>() != null)
								{
									prefabToNetwork.GetComponent<NetworkID>().enabled = false;
								}
								if (prefabToNetwork.GetComponent<ActorNetwork>() == null)
								{
									prefabToNetwork.AddComponent<ActorNetwork>();
								}
								worldObjectController.AppWorldObjects[counterAppWorldObjects] = prefabToNetwork;
								counterAppWorldObjects++;
							}
						}
                    }
                }
                else
                {
                    NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED, 0.2f, 1);
                }
            }
			else
			{
#if !ENABLE_PHOTON && !ENABLE_NAKAMA
                // CONNECT TO THE SERVER
                ClientTCPEventsController.Instance.Initialitzation(MultiplayerConfiguration.LoadIPAddressServer(), MultiplayerConfiguration.LoadPortServer(), MultiplayerConfiguration.LoadRoomNumberInServer(0), MultiplayerConfiguration.LoadMachineIDServer(0), MultiplayerConfiguration.LoadBufferSizeReceive(), MultiplayerConfiguration.LoadTimeoutReceive(), MultiplayerConfiguration.LoadBufferSizeSend(), MultiplayerConfiguration.LoadTimeoutSend());

                // NETWORK VARIABLES MANAGER
                Utilities.AddChild(transform, NetworkVariablesManager);
#elif !ENABLE_PHOTON && ENABLE_NAKAMA
				NakamaController.Instance.Initialitzation();
#endif

				// ADD NETWORK IDENTIFICATION TO THE GAME OBJECTS
				for (int i = 0; i < GameObjects.Length; i++)
                    {
                        GameObject prefabToNetwork = GameObjects[i];
                        if (prefabToNetwork.GetComponent<NetworkID>() == null)
                        {
                            prefabToNetwork.AddComponent<NetworkID>();
                        }
                        else
                        {
                            prefabToNetwork.GetComponent<NetworkID>().enabled = true;
                        }
#if !DISABLE_UNET_COMMS
                        if (prefabToNetwork.GetComponent<NetworkWorldObjectData>() != null)
                        {
                            prefabToNetwork.GetComponent<NetworkWorldObjectData>().enabled = false;
                        }
#endif
                        if (prefabToNetwork.GetComponent<ActorNetwork>() == null)
                        {
                            prefabToNetwork.AddComponent<ActorNetwork>();
                        }
                    }
            }

            m_hasBeenInitialized = true;
		}

		// -------------------------------------------
		/* 
		 * Release resources
		 */
		public void Destroy()
		{
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;

			if (instance != null)
            {
				GameObject.Destroy(instance.gameObject);
				instance = null;

			}

		}

		// -------------------------------------------
		/* 
		 * Returns the network identificator independently if it's a local or a remote game
		 */
		public int GetUniversalNetworkID()
		{
			if (IsLocalGame)
			{
                if (MultiplayerConfiguration.LoadNumberOfPlayers() != 1)
                {
                    return CommunicationsController.Instance.NetworkID;
                }
                else
                {
                    return 1;
                }
            }
			else
			{
#if ENABLE_PHOTON
                return PhotonController.Instance.UniqueNetworkID;
#elif ENABLE_NAKAMA
				return NakamaController.Instance.UniqueNetworkID;
#else
				return ClientTCPEventsController.Instance.UniqueNetworkID;
#endif
			}
		}

        // -------------------------------------------
        /* 
		 * Returns the network identificator of the server
		 */
        public int GetServerNetworkID()
        {
            if (IsLocalGame)
            {
                return 0;
            }
            else
            {
                return ClientTCPEventsController.Instance.ServerNetworkID;
            }
        }

        // -------------------------------------------
        /* 
		 * Get the prefab by name
		 */
        private GameObject GetPrefabByName(string _prefabName)
		{
			for (int i = 0; i < GameObjects.Length; i++)
			{
				if (_prefabName.IndexOf(GameObjects[i].name) != -1)
				{
					return GameObjects[i];
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		 * Get the prefab index by name
		 */
		private int GetPrefabIndexOfName(string _prefabName)
		{
			for (int i = 0; i < GameObjects.Length; i++)
			{
				if (_prefabName.IndexOf(GameObjects[i].name) != -1)
				{
					return i;
				}
			}
			return -1;
		}

		// -------------------------------------------
		/* 
		 * Get the network object by id
		 */
		private object GetNetworkObjectByID(int _netID, int _uid)
		{
			if (IsLocalGame)
			{
#if !DISABLE_UNET_COMMS
				for (int i = 0; i < m_unetNetworkObjects.Count; i++)
				{
					if (m_unetNetworkObjects[i] != null)
					{
						if (m_unetNetworkObjects[i].GetNetworkObjectData().CheckID(_netID, _uid))
						{
							return m_unetNetworkObjects[i];
						}
					}
					else
					{
						m_unetNetworkObjects.RemoveAt(i);
						i--;
					}
				}
#endif
			}
			else
			{
				for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
				{
					if (m_tcpNetworkObjects[i] != null)
					{
						if (m_tcpNetworkObjects[i].GetComponent<NetworkID>().CheckID(_netID, _uid))
						{
							return m_tcpNetworkObjects[i];
						}
					}
					else
					{
						m_tcpNetworkObjects.RemoveAt(i);
						i--;
					}
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		* IncreaseInstanceCounter
		*/
		public int IncreaseInstanceCounter()
		{
			m_uidCounter++;
			return m_uidCounter;
		}

		// -------------------------------------------
		/* 
		* CreatePathToPrefabInResources
		*/
		public string CreatePathToPrefabInResources(string _nameNetworkAsset, bool _addExtension = false, bool _forceSinglePlayer = false)
        {
			string finalNameNetworkAssets = "Network/";
			if (_forceSinglePlayer)
            {
				finalNameNetworkAssets += "Socket/" + _nameNetworkAsset + (_addExtension ? "Socket" : "");
			}
			else
            {
				if (YourNetworkTools.Instance.IsLocalGame)
				{
					finalNameNetworkAssets += "Mirror/" + _nameNetworkAsset + (_addExtension ? "Mirror" : "");
				}
				else
				{
#if ENABLE_PHOTON
                finalNameNetworkAssets += "Photon/" + _nameNetworkAsset + (_addExtension?"Photon":"");
#elif ENABLE_NAKAMA
					finalNameNetworkAssets += "Nakama/" + _nameNetworkAsset + (_addExtension ? "Nakama" : "");
#else
					finalNameNetworkAssets += "Socket/" + _nameNetworkAsset + (_addExtension ? "Socket" : "");
#endif
				}
			}
			var networkAsset = Resources.Load(finalNameNetworkAssets) as GameObject;
			if (networkAsset != null)
            {
				return finalNameNetworkAssets;
			}
			else
            {
				return null;
            }			
		}

		// -------------------------------------------
		/* 
		* Create a NetworkObject
		*/
		public void CreateLocalNetworkObject(string _basePrefabName, string _prefabName, object _initialData, bool _createInServer, float _x = 0, float _y = 0, float _z = 0)
		{
			if (IsLocalGame)
			{
				string assignedNetworkName = _basePrefabName + TOKEN_SEPARATOR_NAME + GetUniversalNetworkID() + TOKEN_SEPARATOR_NAME + m_uidCounter;
				m_uidCounter++;
#if !DISABLE_UNET_COMMS
				NetworkWorldObject networkWorldObject = new NetworkWorldObject(assignedNetworkName, _prefabName, new Vector3(_x, _y, _z), Vector3.zero, Vector3.one, _initialData, true, true, _createInServer);
				m_unetNetworkObjects.Add(networkWorldObject);
#endif
			}
			else
			{
#if ENABLE_PHOTON
                object[] instantiationData = { GetUniversalNetworkID(), m_uidCounter };
                GameObject networkGameObject = PhotonNetwork.Instantiate(_prefabName, Vector3.zero, Quaternion.identity, 0, instantiationData);
                networkGameObject.transform.position = new Vector3(_x, _y, _z);
                networkGameObject.GetComponent<NetworkID>().NetID = GetUniversalNetworkID();
                networkGameObject.GetComponent<NetworkID>().UID = m_uidCounter;
                m_uidCounter++;
                networkGameObject.GetComponent<NetworkID>().IndexPrefab = GetPrefabIndexOfName(_prefabName);
                m_tcpNetworkObjects.Add(networkGameObject);
                if (networkGameObject.GetComponent<IGameNetworkActor>() != null)
                {
                    networkGameObject.GetComponent<IGameNetworkActor>().Initialize(_initialData);
                }
#else
                GameObject networkGameObject = Utilities.AddChild(this.gameObject.transform, GetPrefabByName(_prefabName));
                networkGameObject.transform.position = new Vector3(_x, _y, _z);
                networkGameObject.GetComponent<NetworkID>().NetID = GetUniversalNetworkID();
				networkGameObject.GetComponent<NetworkID>().UID = m_uidCounter;
				m_uidCounter++;
				networkGameObject.GetComponent<NetworkID>().IndexPrefab = GetPrefabIndexOfName(_prefabName);
				m_tcpNetworkObjects.Add(networkGameObject);
				if (networkGameObject.GetComponent<IGameNetworkActor>() != null)
				{
					networkGameObject.GetComponent<IGameNetworkActor>().Initialize(_initialData);
				}
#if ENABLE_NAKAMA
				NakamaController.Instance.SendTransform(networkGameObject.GetComponent<NetworkID>().NetID, networkGameObject.GetComponent<NetworkID>().UID, networkGameObject.GetComponent<NetworkID>().IndexPrefab, networkGameObject.transform.position, networkGameObject.transform.forward, networkGameObject.transform.localScale);
#else
				ClientTCPEventsController.Instance.SendTranform(networkGameObject.GetComponent<NetworkID>().NetID, networkGameObject.GetComponent<NetworkID>().UID, networkGameObject.GetComponent<NetworkID>().IndexPrefab, networkGameObject.transform.position, networkGameObject.transform.forward, networkGameObject.transform.localScale);
#endif
#endif
			}
		}

		// -------------------------------------------
		/* 
		 * Will get the prefab from the referenced class name
		 */
		public GameObject GetPrefabFromClassName(GameObject[] _prefabs, string _prefabName)
		{
			GameObject prefab = null;
			if (_prefabs == null) return null;
			if (_prefabs.Length == 0) return null;

			for (int i = 0; i < _prefabs.Length; i++)
			{
				if (_prefabs[i].name == _prefabName)
				{
					prefab = _prefabs[i];
				}
			}

			return prefab;
		}

		// -------------------------------------------
		/* 
		* Will check if there are objects to be initialized with the data received
		*/
		private void CheckInitializationObjects(string _netID = null)
		{
			if (IsLocalGame)
			{
#if !DISABLE_UNET_COMMS
				for (int i = 0; i < m_unetNetworkObjects.Count; i++)
				{
					if (m_unetNetworkObjects[i] != null)
					{
						if (m_unetNetworkObjects[i].GetNetworkObjectData() != null)
						{
							if (_netID == null)
							{
								CheckExistingInitialDataForObject(m_unetNetworkObjects[i].GetNetworkObjectData().GetID(), m_unetNetworkObjects[i].GetNetworkObjectData().gameObject);
							}
							else
							{
								if (m_unetNetworkObjects[i].GetNetworkObjectData().GetID() == _netID)
								{
									CheckExistingInitialDataForObject(m_unetNetworkObjects[i].GetNetworkObjectData().GetID(), m_unetNetworkObjects[i].GetNetworkObjectData().gameObject);
								}
							}
						}
					}
				}
#endif
			}
			else
			{
				for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
				{
					if (m_tcpNetworkObjects[i] != null)
					{
						if (_netID == null)
						{
							CheckExistingInitialDataForObject(m_tcpNetworkObjects[i].GetComponent<NetworkID>().GetID(), m_tcpNetworkObjects[i]);
						}
						else
						{
							if (m_tcpNetworkObjects[i].GetComponent<NetworkID>().GetID() == _netID)
							{
								CheckExistingInitialDataForObject(m_tcpNetworkObjects[i].GetComponent<NetworkID>().GetID(), m_tcpNetworkObjects[i]);
							}
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		* CheckExistingInitialDataForObject
		*/
		private bool CheckExistingInitialDataForObject(string _keyID, GameObject _objectToInit)
		{
            if (m_initialData.ContainsKey(_keyID))
			{
				string initialData = "";
				if (m_initialData.TryGetValue(_keyID, out initialData))
				{
					// _objectToInit.GetComponent<IGameNetworkActor>().Initialize(initialData);
					// Debug.LogError("+++++++++++++++++SENDING INITIAL DATA TO ["+ _keyID + "]::DATA("+ initialData + ")");
					// NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_YOURNETWORKTOOLS_INITIALITZATION_DATA, 0.01f, _keyID, initialData);
					NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_YOURNETWORKTOOLS_INITIALITZATION_DATA, 0.01f, _keyID, initialData);
					return true;
				}
			}
			return false;
		}

		// -------------------------------------------
		/* 
		* Will destroy a network object by NetID and UID
		*/
		private void DestroyNetworkObject(int _netID, int _uid)
		{
			if (IsLocalGame)
			{
#if !DISABLE_UNET_COMMS
				for (int i = 0; i < m_unetNetworkObjects.Count; i++)
				{
					if (m_unetNetworkObjects[i] != null)
					{
						if (m_unetNetworkObjects[i].GetNetworkObjectData() != null)
						{
							if ((m_unetNetworkObjects[i].GetNetworkObjectData().NetID == _netID)
								&& (m_unetNetworkObjects[i].GetNetworkObjectData().UID == _uid))
							{
#if DEBUG_MODE_DISPLAY_LOG
								Debug.LogError("[UNET] REMOVED FROM LIST");
#endif
								m_unetNetworkObjects.RemoveAt(i);
								return;
							}
						}
						else
						{
							m_unetNetworkObjects.RemoveAt(i);
							i--;
						}
					}
					else
					{
						m_unetNetworkObjects.RemoveAt(i);
						i--;
					}
				}
#endif
			}
			else
			{
				for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
				{
					if (m_tcpNetworkObjects[i] != null)
					{
						if ((m_tcpNetworkObjects[i].GetComponent<NetworkID>().NetID == _netID)
							&& (m_tcpNetworkObjects[i].GetComponent<NetworkID>().UID == _uid))
						{
#if DEBUG_MODE_DISPLAY_LOG
							Debug.LogError("[SOCKET] REMOVED FROM LIST");
#endif
							m_tcpNetworkObjects.RemoveAt(i);
							return;
						}
					}
					else
					{
						m_tcpNetworkObjects.RemoveAt(i);
						i--;
					}
				}
			}
		}



		// -------------------------------------------
		/* 
		* Manager of global events
		*/
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == EVENT_YOURNETWORKTOOLS_INITIALITZATION_DATA)
            {
				string targetNetworkID = (string)_list[0];
				string initialDataNetwork = (string)_list[1];
				ActorNetwork[] networkActors = GameObject.FindObjectsOfType<ActorNetwork>();
				// Debug.LogError("+++++++++++++EVENT_YOURNETWORKTOOLS_INITIALITZATION_DATA::TARGET[" + targetNetworkID + "]::data[" + initialDataNetwork + "]::TOTAL NETWORK ACTORS["+ networkActors.Length + "]");
				for (int i = 0; i < networkActors.Length; i++)
                {
					IGameNetworkActor networkActor = networkActors[i].GetComponentInParent<IGameNetworkActor>();
					if (networkActor != null)
                    {
						if (networkActor.NetworkID.CheckID(targetNetworkID))
                        {
							networkActor.Initialize(initialDataNetwork);
						}
                    }
				}
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID)
			{
#if ENABLE_BALANCE_LOADER
				int totalPlayersConfigurated = MultiplayerConfiguration.LoadNumberOfPlayers();
				if (totalPlayersConfigurated != MultiplayerConfiguration.VALUE_FOR_JOINING)
				{
					string friends = MultiplayerConfiguration.LoadFriendsGame();
					if (friends.Length > 0)
					{
						string[] friendIDs = friends.Split(',');
						int idRoomLobby = MultiplayerConfiguration.LoadRoomNumberInServer(-1);
						ClientTCPEventsController.Instance.CreateRoomForFriends(idRoomLobby, friendIDs, "");
					}
					else
					{
						string nameRoomLobby = MultiplayerConfiguration.LoadNameRoomLobby();
						if (nameRoomLobby.Length > 0)
						{
							int idRoomLobby = MultiplayerConfiguration.LoadRoomNumberInServer(-1);
							ClientTCPEventsController.Instance.CreateRoomForLobby(idRoomLobby, nameRoomLobby, totalPlayersConfigurated, "");
						}
						else
						{
							throw new Exception("THERE IS NO NAME OF LOBBY TO CREATE A TCP CONNECTION");
						}
					}
				}
				else
				{
					int idRoomLobby = MultiplayerConfiguration.LoadRoomNumberInServer(-1);
					if (idRoomLobby != -1)
					{
						if (MultiplayerConfiguration.LoadIsRoomLobby())
						{
							ClientTCPEventsController.Instance.JoinRoomOfLobby(idRoomLobby, "null", "");
						}
						else
						{
							ClientTCPEventsController.Instance.JoinRoomForFriends(idRoomLobby, "null", "");
						}
					}
					else
					{
						throw new Exception("NO GOOD");
					}
				}
#endif
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM)
			{
				// Debug.LogError("EVENT_CLIENT_TCP_CONNECTED_ROOM::UniversalUniqueID[" + GetUniversalNetworkID() + "]");
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED)
            {
				if (IsServer)
				{
					// Debug.LogError("++++++++++++++++++++SENDING INFORMATION ABOUT ALL EXISTING NETWORK OBJECTS+++++++++++++++++++++++++++++");
					CheckInitializationObjects();
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_LOCAL_CREATION_CONFIRMATION)
			{
				if (IsServer)
				{
					string keyNetworkGO = (string)_list[0];
					CheckInitializationObjects(keyNetworkGO);
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA)
			{
				if (IsServer)
                {
					if (!m_initialData.ContainsKey((string)_list[0]))
					{
						string keyNetworkGO = (string)_list[0];
						string dataNetworkGO = (string)_list[1];
						m_initialData.Add(keyNetworkGO, dataNetworkGO);
						// Debug.LogError("*************************************DATA ADDED TO LIST(" + keyNetworkGO + ")("+ dataNetworkGO + ")::TOTAL INITIAL DATA["+ m_initialData.Count + "]::TOTAL TCP PLAYERS["+m_tcpNetworkObjects.Count+"]");
						CheckInitializationObjects(keyNetworkGO);
					}
				}
			}
			if (_nameEvent == EVENT_YOURNETWORKTOOLS_NETID_NEW)
            {
				if (IsServer)
				{
					// Debug.LogError("*************************************NEW TCP NETWORK OBJECT REGISTERED(" + m_tcpNetworkObjects.Count + "]");
					CheckInitializationObjects();
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST)
			{
				DestroyNetworkObject(int.Parse((string)_list[0]), int.Parse((string)_list[1]));
			}
			if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT)
			{
#if !DISABLE_UNET_COMMS
				m_unetNetworkObjects.Add(new NetworkWorldObject((GameObject)_list[0]));
#endif
			}
			if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED)
			{
				Debug.Log("----------------------DISCONNECTED PLAYER[" + (int)_list[0] + "]");
			}
#if ENABLE_PHOTON
            if (_nameEvent == EVENT_YOURNETWORKTOOLS_CREATED_GAMEOBJECT)
            {
                GameObject newGO = (GameObject)_list[0];
                if (!m_tcpNetworkObjects.Contains(newGO))
                {
                    m_tcpNetworkObjects.Add(newGO);
					NetworkEventController.Instance.DispatchLocalEvent(EVENT_YOURNETWORKTOOLS_NETID_NEW);
				}
            }
#endif
            if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_TRANSFORM_DATA)
			{
				int NetID = (int)_list[0];
				int UID = (int)_list[1];
				int prefabIndex = (int)_list[2];
				Vector3 position = (Vector3)_list[3];
				Vector3 forward = (Vector3)_list[4];
				Vector3 scale = (Vector3)_list[5];
				object networkObject = GetNetworkObjectByID(NetID, UID);
				GameObject networkGameObject = null;
				if (networkObject == null)
				{
					m_networkIDReceived = NetID;
					networkGameObject = Utilities.AddChild(this.gameObject.transform, GetPrefabByName(GameObjects[prefabIndex].name));
					networkGameObject.GetComponent<NetworkID>().IndexPrefab = GetPrefabIndexOfName(GameObjects[prefabIndex].name);
					networkGameObject.GetComponent<NetworkID>().NetID = NetID;
					networkGameObject.GetComponent<NetworkID>().UID = UID;
					m_tcpNetworkObjects.Add(networkGameObject);
					networkGameObject.transform.position = position;
					networkGameObject.transform.forward = forward;
					networkGameObject.transform.localScale = scale;
					NetworkEventController.Instance.DispatchLocalEvent(EVENT_YOURNETWORKTOOLS_NETID_NEW);
				}
				else
				{
					networkGameObject = (GameObject)networkObject;
					InterpolatorController.Instance.Interpolate(networkGameObject, position, TimeToUpdateTransforms * 1.01f);
					InterpolatorController.Instance.InterpolateForward(networkGameObject, forward, TimeToUpdateTransforms * 1.01f);
					networkGameObject.transform.localScale = scale;
				}
			}
			if (_nameEvent == EVENT_YOURNETWORKTOOLS_DESTROYED_GAMEOBJECT)
			{
				int NetID = (int)_list[1];
				int UID = (int)_list[2];

				if (IsLocalGame)
				{
#if !DISABLE_UNET_COMMS
					for (int i = 0; i < m_unetNetworkObjects.Count; i++)
					{
						bool removeObject = false;
						if (m_unetNetworkObjects[i] == null)
						{
							removeObject = true;
						}
						else
						{
							if (m_unetNetworkObjects[i].GetNetworkObjectData() == null)
							{
								removeObject = true;
							}
						}
						if (removeObject)
						{
							m_unetNetworkObjects.RemoveAt(i);
						}
						else
						{
							if ((m_unetNetworkObjects[i].GetNetworkObjectData().NetID == NetID)
							&& (m_unetNetworkObjects[i].GetNetworkObjectData().UID == UID))
							{
								m_unetNetworkObjects[i].Destroy();
								m_unetNetworkObjects.RemoveAt(i);
								return;
							}
						}
					}
#endif
				}
				else
				{
					for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
					{
						bool removeObject = false;
						if (m_tcpNetworkObjects[i] == null)
						{
							removeObject = true;
						}
						if (removeObject)
						{
							m_tcpNetworkObjects.RemoveAt(i);
						}
						else
						{
							if ((m_tcpNetworkObjects[i].GetComponent<NetworkID>().NetID == NetID)
								&& (m_tcpNetworkObjects[i].GetComponent<NetworkID>().UID == UID))
							{
								m_tcpNetworkObjects.RemoveAt(i);
								return;
							}
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		* Will update the transforms in the remotes players
		*/
		public void UpdateRemoteTransforms(bool _force)
		{
			if (m_activateTransformUpdate || _force)
			{
				if (!IsLocalGame)
				{
#if !ENABLE_PHOTON
                    m_timeoutUpdateRemoteNetworkObject += Time.deltaTime;
					if ((m_timeoutUpdateRemoteNetworkObject >= TimeToUpdateTransforms) || _force)
					{
						m_timeoutUpdateRemoteNetworkObject = 0;
						for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
						{
							GameObject networkGameObject = m_tcpNetworkObjects[i];
							if (networkGameObject != null)
							{
								if (networkGameObject.GetComponent<NetworkID>() != null)
								{
									if ((networkGameObject.GetComponent<NetworkID>().NetID == GetUniversalNetworkID()) || _force)
									{
#if ENABLE_NAKAMA
										NakamaController.Instance.SendTransform(networkGameObject.GetComponent<NetworkID>().NetID, networkGameObject.GetComponent<NetworkID>().UID, networkGameObject.GetComponent<NetworkID>().IndexPrefab, networkGameObject.transform.position, networkGameObject.transform.forward, networkGameObject.transform.localScale);
#else
										ClientTCPEventsController.Instance.SendTranform(networkGameObject.GetComponent<NetworkID>().NetID, networkGameObject.GetComponent<NetworkID>().UID, networkGameObject.GetComponent<NetworkID>().IndexPrefab, networkGameObject.transform.position, networkGameObject.transform.forward, networkGameObject.transform.localScale);
#endif
									}
								}
							}
						}
					}
#endif
									}
				else
				{
#if !DISABLE_UNET_COMMS
					for (int i = 0; i < m_unetNetworkObjects.Count; i++)
					{
						NetworkWorldObject unetNetworkObject = m_unetNetworkObjects[i];
						bool destroyNetworkObject = false;
						if (unetNetworkObject != null)
						{
							try
							{
								if ((unetNetworkObject.GetNetworkObjectData().NetID == CommunicationsController.Instance.NetworkID) || _force)
								{
									unetNetworkObject.SetPosition(unetNetworkObject.GetNetworkObjectData().gameObject.transform.position);
									unetNetworkObject.SetScale(unetNetworkObject.GetNetworkObjectData().gameObject.transform.localScale);
									unetNetworkObject.SetForward(unetNetworkObject.GetNetworkObjectData().gameObject.transform.forward);
								}
							}
							catch (Exception err)
							{
								destroyNetworkObject = true;
							}
						}
						if (destroyNetworkObject)
						{
							m_unetNetworkObjects.RemoveAt(i);
							i--;
						}
					}
#endif
				}
			}
		}

		// -------------------------------------------
		/* 
		* Send the information about the local transforms
		*/
		void Update()
		{
			UpdateRemoteTransforms(false);
		}
    }
}
