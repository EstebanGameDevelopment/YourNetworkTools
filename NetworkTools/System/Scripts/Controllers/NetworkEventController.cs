#if ENABLE_PHOTON
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using YourCommonTools;
using UnityEngine.SceneManagement;

namespace YourNetworkingTools
{
	public delegate void NetworkEventHandler(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list);

	/******************************************
	 * 
	 * BasicEventController
	 * 
	 * Class used to dispatch events through all the system
	 * 
	 * @author Esteban Gallardo
	 */
	public class NetworkEventController : MonoBehaviour
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        // SYSTEM
        public const string EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED = "EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED";
		public const string EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED = "EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED";
		public const string EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS = "EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS";
		public const string EVENT_SYSTEM_DESTROY_NETWORK_SCREENS = "EVENT_SYSTEM_DESTROY_NETWORK_SCREENS";
		public const string EVENT_SYSTEM_PLAYER_HAS_BEEN_DESTROYED = "EVENT_SYSTEM_PLAYER_HAS_BEEN_DESTROYED";
		public const string EVENT_STREAMSERVER_REPORT_CLOSED_STREAM = "EVENT_STREAMSERVER_REPORT_CLOSED_STREAM";

		public const string EVENT_SYSTEM_VARIABLE_CREATE_REMOTE = "EVENT_SYSTEM_VARIABLE_CREATE_REMOTE";
		public const string EVENT_SYSTEM_VARIABLE_CREATE_LOCAL = "EVENT_SYSTEM_VARIABLE_CREATE_LOCAL";
		public const string EVENT_SYSTEM_VARIABLE_SET = "EVENT_SYSTEM_VARIABLE_SET";
		public const string EVENT_SYSTEM_VARIABLE_DESTROY = "EVENT_SYSTEM_VARIABLE_DESTROY";

		// CommunicationsController
		public const string EVENT_COMMUNICATIONSCONTROLLER_REGISTER_INITIAL_DATA_ON_SERVER = "EVENT_COMMUNICATIONSCONTROLLER_REGISTER_INITIAL_DATA_ON_SERVER";
		public const string EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_FROM_SERVER_TO_CLIENTS = "EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_FROM_SERVER_TO_CLIENTS";
		public const string EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_CLIENT_TO_SERVER = "EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_CLIENT_TO_SERVER";
		public const string EVENT_COMMUNICATIONSCONTROLLER_REQUEST_TO_CREATE_NETWORK_OBJECT = "EVENT_COMMUNICATIONSCONTROLLER_REQUEST_TO_CREATE_NETWORK_OBJECT";
		public const string EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT = "EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT";
		public const string EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_REGISTRATION = "EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_REGISTRATION";
		public const string EVENT_COMMUNICATIONSCONTROLLER_REGISTER_ALL_NETWORK_PREFABS = "EVENT_COMMUNICATIONSCONTROLLER_REGISTER_ALL_NETWORK_PREFABS";
		public const string EVENT_COMMUNICATIONSCONTROLLER_REGISTER_PREFAB = "EVENT_COMMUNICATIONSCONTROLLER_REGISTER_PREFAB";

		// PlayerConnectionController
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_CREATE_NETWORK_OBJECT = "EVENT_PLAYERCONNECTIONCONTROLLER_CREATE_NETWORK_OBJECT";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_NETWORK_OBJECT = "EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_NETWORK_OBJECT";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_CONFIRMATION_NETWORK_OBJECT = "EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_CONFIRMATION_NETWORK_OBJECT";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_UPDATE_PROPERTY = "EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_UPDATE_PROPERTY";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_MESSAGE = "EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_MESSAGE";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_RPC_MESSAGE = "EVENT_PLAYERCONNECTIONCONTROLLER_RPC_MESSAGE";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_REGISTER_VARIABLE_COMPLETED = "EVENT_PLAYERCONNECTIONCONTROLLER_REGISTER_VARIABLE_COMPLETED";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_KICK_OUT_PLAYER = "EVENT_PLAYERCONNECTIONCONTROLLER_KICK_OUT_PLAYER";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_CONFIRMATION_KICKED_OUT_PLAYER = "EVENT_PLAYERCONNECTIONCONTROLLER_CONFIRMATION_KICKED_OUT_PLAYER";

		// PlayerConnectionData
		public const string EVENT_PLAYERCONNECTIONDATA_USER_CONNECTED = "EVENT_PLAYERCONNECTIONDATA_USER_CONNECTED";
		public const string EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED = "EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED";
		public const string EVENT_PLAYERCONNECTIONDATA_NETWORK_ADDRESS = "EVENT_PLAYERCONNECTIONDATA_NETWORK_ADDRESS";

		// WORLD CONTROLLER
		public const string EVENT_WORLDOBJECTCONTROLLER_REMOTE_CREATION_CONFIRMATION = "EVENT_WORLDOBJECTCONTROLLER_REMOTE_CREATION_CONFIRMATION";
		public const string EVENT_WORLDOBJECTCONTROLLER_LOCAL_CREATION_CONFIRMATION = "EVENT_WORLDOBJECTCONTROLLER_LOCAL_CREATION_CONFIRMATION";
		public const string EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA = "EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA";
		public const string EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST = "EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST";
		public const string EVENT_WORLDOBJECTCONTROLLER_DESTROY_CONFIRMATION = "EVENT_WORLDOBJECTCONTROLLER_DESTROY_CONFIRMATION";

		// NETWORK VARIABLE
		public const string EVENT_NETWORKVARIABLE_STATE_REPORT = "EVENT_NETWORKVARIABLE_STATE_REPORT";
		public const string EVENT_NETWORKVARIABLE_REGISTER_NEW = "EVENT_NETWORKVARIABLE_REGISTER_NEW";

		// GENERIC EVENTS
		public const string EVENT_SIMPLE_TEXT_MESSAGE = "EVENT_SIMPLE_TEXT_MESSAGE";
		public const string EVENT_BINARY_SEND_DATA_MESSAGE = "EVENT_BINARY_SEND_DATA_MESSAGE";

		// REGISTER PREFAB TYPES
		public const string REGISTER_PREFABS_OBJECTS = "WorldObjects";

		// CLASS NAME WHICH CONTAINS THE SPECIFIC PROGRAM NETWORK PREFAB OBJECTS
		public const string CLASS_WORLDOBJECTCONTROLLER_NAME = "WorldObjectController";

		// BASIC TYPES
		public const string BASIC_TYPE_NETWORK_INTEGER_NAME = "NetworkIntegerData";
		public const string BASIC_TYPE_NETWORK_FLOAT_NAME = "NetworkFloatData";
		public const string BASIC_TYPE_NETWORK_STRING_NAME = "NetworkStringData";
		public const string BASIC_TYPE_NETWORK_VECTOR3_NAME = "NetworkVector3Data";

		public event NetworkEventHandler NetworkEvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static NetworkEventController instance;

		public static NetworkEventController Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(NetworkEventController)) as NetworkEventController;
					if (!instance)
					{
						GameObject container = new GameObject();
						DontDestroyOnLoad(container);
						container.name = "NetworkEventController";
						instance = container.AddComponent(typeof(NetworkEventController)) as NetworkEventController;
					}
				}
				return instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<AppEventData> m_listEvents = new List<AppEventData>();
        private List<AppEventData> m_listPriorityEvents = new List<AppEventData>();        

        private string m_nameRoomLobby = "";
		private bool m_isLobbyMode = false;
		private string m_targetScene = "";
        private bool m_checkIgnoreEvent = false;
        private string m_nameIgnoreEvent = "";

        private int m_totalNumberOfPlayers = -1;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------
        public string NameRoomLobby
		{
			get { return m_nameRoomLobby; }
		}
		public bool IsLobbyMode
		{
			get { return m_isLobbyMode; }
		}
        public List<ItemMultiTextEntry> RoomsLobby
        {
            get
            {
#if ENABLE_PHOTON
                return PhotonController.Instance.RoomsLobby;
#elif ENABLE_NAKAMA
				return NakamaController.Instance.RoomsLobby;
#else
                return ClientTCPEventsController.Instance.RoomsLobby;
#endif
			}
        }
        public bool IsConnected
        {
            get
            {
#if ENABLE_PHOTON
                return PhotonController.Instance.IsConnected;
#elif ENABLE_NAKAMA
				return NakamaController.Instance.IsConnected;
#else
                return ClientTCPEventsController.Instance.SocketConnected;
#endif
			}
		}
        public string ServerIPAdress
        {
            get
            {
#if ENABLE_PHOTON
                return PhotonController.Instance.ServerIPAdress;
#elif ENABLE_NAKAMA
				return NakamaController.Instance.ServerIPAddress;
#else
                return ClientTCPEventsController.Instance.ServerIPAddress;
#endif
			}
            set
            {
#if ENABLE_PHOTON
                PhotonController.Instance.ServerIPAdress = value;
#elif ENABLE_NAKAMA
				NakamaController.Instance.ServerIPAddress = value;
#else
                ClientTCPEventsController.Instance.ServerIPAddress = value; 
#endif
			}
		}

        // -------------------------------------------
        /* 
		 * Awake
		 */
        void Awake()
        {
#if ENABLE_PHOTON
            PhotonNetwork.AutomaticallySyncScene = true;
#endif
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        private NetworkEventController()
		{
		}


        // -------------------------------------------
        /* 
		 * Destroy
		 */
        void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			if (instance != null)
			{
                try
                {
                    DispatchLocalEvent(EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS);
                    Destroy(instance.gameObject);
                }
                catch (Exception err) { };
                instance = null;

                try
                {
                    if (m_isLobbyMode)
                    {
#if ENABLE_PHOTON
                        PhotonController.Instance.Destroy();
#elif ENABLE_NAKAMA
						NakamaController.Instance.Destroy();
#else
                        ClientTCPEventsController.Instance.Destroy();
#endif
                    }
                }
                catch (Exception err) { };
            }
        }

        // -------------------------------------------
        /* 
		 * GetRoomIDByName
		 */
        public int GetRoomIDByName(string _roomName)
        {
#if ENABLE_PHOTON
            return PhotonController.Instance.GetRoomIDByName(_roomName);
#elif ENABLE_NAKAMA
			return NakamaController.Instance.GetRoomIDByName(_roomName);
#else
            return ClientTCPEventsController.Instance.GetRoomIDByName(_roomName);
#endif
		}

        // -------------------------------------------
        /* 
		 * GetExtraDataForRoom
		 */
        public string GetExtraDataForRoom(int _roomID)
        {
#if ENABLE_PHOTON
            return PhotonController.Instance.GetExtraDataForRoom(_roomID);
#elif ENABLE_NAKAMA
			return NakamaController.Instance.GetExtraDataForRoom(_roomID);
#else
            return ClientTCPEventsController.Instance.GetExtraDataForRoom(_roomID);
#endif
		}


		// -------------------------------------------
		/* 
		 * CheckToIgnoreEvent
		 */
		private bool CheckToApplyEvent(string _nameEvent)
        {
            if (m_checkIgnoreEvent)
            {
                if (m_nameIgnoreEvent == _nameEvent)
                {
                    m_checkIgnoreEvent = false;
                    m_nameIgnoreEvent = "";
                    return false;
                }
            }
            return true;
        }

		// -------------------------------------------
		/* 
		 * Will dispatch a network event
		 */
		public void DispatchNetworkEvent(string _nameEvent, params string[] _list)
		{
			// Debug.Log("[NETWORK]_nameEvent=" + _nameEvent);
			if ((NetworkEvent != null) && CheckToApplyEvent(_nameEvent))
			{
				if (YourNetworkTools.Instance != null) NetworkEvent(_nameEvent, false, YourNetworkTools.Instance.GetUniversalNetworkID(), -1, _list);
			}
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a custom event
		 */
		public void DispatchCustomNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params string[] _list)
		{
            // Debug.Log("[NETWORK]_nameEvent=" + _nameEvent);
            if ((NetworkEvent != null) && CheckToApplyEvent(_nameEvent)) NetworkEvent(_nameEvent, _isLocalEvent, _networkOriginID, _networkTargetID, _list);
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a local event
		 */
		public void DispatchLocalEvent(string _nameEvent, params object[] _list)
		{
            // Debug.Log("_nameEvent=" + _nameEvent);
            if ((NetworkEvent != null) && CheckToApplyEvent(_nameEvent)) NetworkEvent(_nameEvent, true, -99, -99, _list);
		}

		// -------------------------------------------
		/* 
		 * Will add a new delayed local event to the queue
		 */
		public void DelayLocalEvent(string _nameEvent, float _time, params object[] _list)
		{
            if ((NetworkEvent != null) && CheckToApplyEvent(_nameEvent))
            {
                m_listEvents.Add(new AppEventData(_nameEvent, AppEventData.CONFIGURATION_INTERNAL_EVENT, true, -99, _time, _list));
            }                
		}

		// -------------------------------------------
		/* 
		 * Clone a delayed event
		 */
		public void DelayBasicEvent(AppEventData _timeEvent)
		{
            if ((NetworkEvent != null) && CheckToApplyEvent(_timeEvent.NameEvent))
            {
                m_listEvents.Add(new AppEventData(_timeEvent.NameEvent, AppEventData.CONFIGURATION_INTERNAL_EVENT, _timeEvent.IsLocalEvent, _timeEvent.NetworkID, _timeEvent.Time, _timeEvent.ListParameters));
            }
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a delayed network event
		 */
		public void DelayNetworkEvent(string _nameEvent, float _time, params string[] _list)
		{
            if ((NetworkEvent != null) && CheckToApplyEvent(_nameEvent))
            {
                if (YourNetworkTools.Instance != null) m_listEvents.Add(new AppEventData(_nameEvent, AppEventData.CONFIGURATION_INTERNAL_EVENT, false, YourNetworkTools.Instance.GetUniversalNetworkID(), _time, _list));
            }
		}

        // -------------------------------------------
        /* 
		 * Will dispatch a delayed network event
		 */
        public void PriorityDelayNetworkEvent(string _nameEvent, float _time, params string[] _list)
        {
            if ((NetworkEvent != null) && CheckToApplyEvent(_nameEvent))
            {
				if (YourNetworkTools.Instance != null) m_listPriorityEvents.Add(new AppEventData(_nameEvent, AppEventData.CONFIGURATION_INTERNAL_EVENT, false, YourNetworkTools.Instance.GetUniversalNetworkID(), _time, _list));
            }
        }

        // -------------------------------------------
        /* 
		 * Will dispatch a binary event
		 */
        public void DispatchNetworkBinaryDataEvent(string _nameEvent, params object[] _list)
		{
			int totalSizePacket = 4 + _nameEvent.Length + 4;
			int subTotalSize = 0;
			for (int i = 0; i < _list.Length; i++)
			{
				totalSizePacket += ((byte[])_list[i]).Length;
				subTotalSize += ((byte[])_list[i]).Length;
			}

			int counter = 0;
			byte[] message = new byte[totalSizePacket];
			Array.Copy(BitConverter.GetBytes(_nameEvent.Length), 0, message, counter, 4);
			counter += 4;
			Array.Copy(Encoding.ASCII.GetBytes(_nameEvent), 0, message, counter, _nameEvent.Length);
			counter += _nameEvent.Length;
			Array.Copy(BitConverter.GetBytes(subTotalSize), 0, message, counter, 4);
			counter += 4;
			for (int i = 0; i < _list.Length; i++)
			{
				byte[] item = (byte[])_list[i];
				totalSizePacket += item.Length;
				Array.Copy(item, 0, message, counter, item.Length);
				counter += item.Length;
			}
			if (NetworkEvent != null) NetworkEvent(EVENT_BINARY_SEND_DATA_MESSAGE, true, -99, -99, message);
		}

		// -------------------------------------------
		/* 
		 * Set if we are going to go to the lobby
		 */
		public void MenuController_SetLobbyMode(bool _value)
		{
			m_isLobbyMode = _value;
			MultiplayerConfiguration.SaveIsRoomLobby(m_isLobbyMode);
		}

		// -------------------------------------------
		/* 
		 * Set the name of the room lobby
		 */
		public void MenuController_SetNameRoomLobby(string _value)
		{
			m_nameRoomLobby = _value;
		}

		// -------------------------------------------
		/* 
		 * Set if the connection is going to be local(UNET) or global(Sockets)
		 */
		public void MenuController_SetLocalGame(bool _value)
		{
			YourNetworkTools.SetLocalGame(_value);
		}

		// -------------------------------------------
		/* 
		 * Will save the number of players
		 */
		public void MenuController_SaveNumberOfPlayers(int _value)
		{
			MultiplayerConfiguration.SaveNumberOfPlayers(_value);
		}

		// -------------------------------------------
		/* 
		 * Will load the number of players
		 */
		public int MenuController_LoadNumberOfPlayers()
		{
			return MultiplayerConfiguration.LoadNumberOfPlayers();
		}

		// -------------------------------------------
		/* 
		 * Will create a new room for lobby
		 */
		public void MenuController_CreateNewLobbyRoom(string _nameLobby, int _finalNumberOfPlayers, string _extraData)
		{
            MultiplayerConfiguration.SaveNameRoomLobby(_nameLobby);
#if ENABLE_BALANCE_LOADER
			UIEventController.Instance.DispatchUIEvent(MenuScreenController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE);
			CommsHTTPConfiguration.CreateNewRoom(true, _nameLobby, ClientTCPEventsController.GetPlayersString(_finalNumberOfPlayers), _extraData);
#else
			MenuController_CreateRoomForLobby(_nameLobby, _finalNumberOfPlayers, _extraData);
#endif
        }

        // -------------------------------------------
        /* 
		 * Will create a new room for friends
		 */
        public void MenuController_CreateNewFacebookRoom(string _friends, List<string> _friendsIDs, string _extraData)
		{
#if ENABLE_BALANCE_LOADER
			UIEventController.Instance.DispatchUIEvent(MenuScreenController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE);
			MultiplayerConfiguration.SaveFriendsGame(_friends);
			MultiplayerConfiguration.SaveNumberOfPlayers(_friends.Split(',').Length);
			CommsHTTPConfiguration.CreateNewRoom(false, FacebookController.Instance.NameHuman, _friends, _extraData);
#else
			ClientTCPEventsController.Instance.CreateRoomForFriends(_friendsIDs.ToArray(), _extraData);
#endif
		}

		// -------------------------------------------
		/* 
		 * Will create the socket connection
		 */
		public void MenuController_InitialitzationSocket(int _numberRoom, int _idMachineHost)
		{
#if ENABLE_PHOTON
            PhotonController.Instance.Login();
#elif ENABLE_NAKAMA
			NakamaController.Instance.Initialitzation();
#else
            ClientTCPEventsController.Instance.Initialitzation(MultiplayerConfiguration.LoadIPAddressServer(), MultiplayerConfiguration.LoadPortServer(), MultiplayerConfiguration.LoadRoomNumberInServer(_numberRoom), MultiplayerConfiguration.LoadMachineIDServer(_idMachineHost), MultiplayerConfiguration.LoadBufferSizeReceive(), MultiplayerConfiguration.LoadTimeoutReceive(), MultiplayerConfiguration.LoadBufferSizeSend(), MultiplayerConfiguration.LoadTimeoutSend());
#endif
        }

        // -------------------------------------------
        /* 
		 * Will connect the socket to create the lobby
		 */
        public void MenuController_CreateRoomForLobby(string _nameLobby, int _finalNumberOfPlayers, string _extraData)
		{
#if ENABLE_PHOTON
            PhotonController.Instance.CreateRoom(_nameLobby, _finalNumberOfPlayers, _extraData);
#elif ENABLE_NAKAMA
			NakamaController.Instance.FindMatch(_nameLobby, _finalNumberOfPlayers, _extraData);
#else
			ClientTCPEventsController.Instance.CreateRoomForLobby(_nameLobby, _finalNumberOfPlayers, _extraData);
#endif
        }

		// -------------------------------------------
		/* 
		 * Will join to an existing room of friends
		 */
		public void MenuController_JoinRoomForFriends(int _room, string _players, string _extraData)
		{
			ClientTCPEventsController.Instance.JoinRoomForFriends(_room, _players, _extraData);
		}

		// -------------------------------------------
		/* 
		 * Will join to an existing room of the lobby
		 */
		public void MenuController_JoinRoomOfLobby(int _room, string _players, string _extraData)
		{
#if !ENABLE_PHOTON && !ENABLE_NAKAMA
			ClientTCPEventsController.Instance.JoinRoomOfLobby(_room, _players, _extraData);
#endif
        }

        // -------------------------------------------
        /* 
		 * Will join to an existing room of the lobby
		 */
        public void MenuController_JoinRoomOfLobby(string _room, string _players, string _extraData)
        {
#if ENABLE_PHOTON
           PhotonController.Instance.JoinRoom(_room, _players, _extraData);
#elif ENABLE_NAKAMA
			NakamaController.Instance.FindMatch(_room, -1, _extraData);
#endif
		}

        // -------------------------------------------
        /* 
		 * Will save the room number we should connect
		 */
        public void MenuController_SaveRoomNumberInServer(int _value)
		{
			MultiplayerConfiguration.SaveRoomNumberInServer(_value);
		}

        // -------------------------------------------
        /* 
		 * Will save the room number we should connect
		 */
        public void MenuController_SaveRoomNameInServer(string _value)
        {
            MultiplayerConfiguration.SaveRoomNameInServer(_value);
        }

        // -------------------------------------------
        /* 
		 * We save the IP address we should connect
		 */
        public void MenuController_SaveIPAddressServer(string _value)
		{
			MultiplayerConfiguration.SaveIPAddressServer(_value);
		}

		// -------------------------------------------
		/* 
		 * Will save the port number of the host
		 */
		public void MenuController_SavePortServer(int _value)
		{
            MultiplayerConfiguration.SavePortServer(_value);
		}

		// -------------------------------------------
		/* 
		 * Will save the ID of the machine which is hosting the room
		 */
		public void MenuController_SaveMachineIDServer(int _value)
		{
			MultiplayerConfiguration.SaveMachineIDServer(_value);
		}		

		// -------------------------------------------
		/* 
		 * Will load the game scene after 1 second delay
		 */
		public void MenuController_LoadGameScene(string _targetScene)
		{
            if (m_targetScene.Length == 0)
            {
                m_targetScene = _targetScene;
#if ENABLE_OCULUS || ENABLE_WORLDSENSE || ENABLE_HTCVIVE || ENABLE_PICONEO
            MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_DISABLED);
#endif
				StartCoroutine(LoadScene());
            }
        }

		// -------------------------------------------
		/* 
		 * LoadGameScene
		 */
		IEnumerator LoadScene()
		{
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_LOAD_NEW_SCENE);
			yield return new WaitForSeconds(0.3f);
			SceneManager.LoadScene(m_targetScene);
		}

        // -------------------------------------------
        /* 
		 * ClearNetworkEvents
		 */
        public void ClearNetworkEvents(string _nameEvent = "", bool _applyFutureEvent = false)
        {
            if (_nameEvent.Length == 0)
            {
                for (int i = 0; i < m_listPriorityEvents.Count; i++)
                {
                    m_listPriorityEvents[i].Time = -1000;
                }
                for (int i = 0; i < m_listEvents.Count; i++)
                {
                    m_listEvents[i].Time = -1000;
                }
            }
            else
            {
                if (_applyFutureEvent)
                {
                    m_nameIgnoreEvent = _nameEvent;
                    m_checkIgnoreEvent = true;
                }

                for (int i = 0; i < m_listPriorityEvents.Count; i++)
                {
                    AppEventData eventData = m_listPriorityEvents[i];
                    if (eventData.NameEvent == _nameEvent)
                    {
                        eventData.Time = -1000;
                        m_nameIgnoreEvent = "";
                        m_checkIgnoreEvent = false;
                    }
                }
                for (int i = 0; i < m_listEvents.Count; i++)
                {
                    AppEventData eventData = m_listEvents[i];
                    if (eventData.NameEvent == _nameEvent)
                    {
                        eventData.Time = -1000;
                        m_nameIgnoreEvent = "";
                        m_checkIgnoreEvent = false;
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Will process the queue of delayed events 
		 */
        void Update()
		{
            // PRIORITY DELAYED EVENTS
            for (int i = 0; i < m_listPriorityEvents.Count; i++)
            {
                AppEventData eventData = m_listPriorityEvents[i];
                float previousTime = eventData.Time;
                eventData.Time -= Time.deltaTime;
                if (eventData.Time <= 0)
                {
                    m_listPriorityEvents.RemoveAt(i);
                    if (previousTime >= 0)
                    {
                        NetworkEvent(eventData.NameEvent, eventData.IsLocalEvent, eventData.NetworkID, -1, eventData.ListParameters);
                    }
                    eventData.Destroy();
                    return;
                }
            }

            // DELAYED EVENTS
            for (int i = 0; i < m_listEvents.Count; i++)
			{
				AppEventData eventData = m_listEvents[i];
                float previousTime = eventData.Time;
                eventData.Time -= Time.deltaTime;
				if (eventData.Time <= 0)
				{
                    m_listEvents.RemoveAt(i);
                    if (previousTime >= 0)
                    {
                        NetworkEvent(eventData.NameEvent, eventData.IsLocalEvent, eventData.NetworkID, -1, eventData.ListParameters);
                    }
					eventData.Destroy();
					return;
				}
			}
		}
	}
}