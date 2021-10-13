#if ENABLE_PHOTON
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text;
using YourCommonTools;

namespace YourNetworkingTools
{

    /******************************************
	 * 
	 * PhotonController
	 * 
	 * Singleton that handles the sending and receiving of the communications
	 * 
	 * @author Esteban Gallardo
	 */
    public class PhotonController : MonoBehaviourPunCallbacks
    {
        public const bool DEBUG = false;

		public const int MESSAGE_EVENT = 0;
		public const int MESSAGE_TRANSFORM = 1;
		public const int MESSAGE_DATA = 2;

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_PHOTONCONTROLLER_GAME_STARTED = "EVENT_PHOTONCONTROLLER_GAME_STARTED";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------
        public const char TOKEN_SEPARATOR_EVENTS = '%';
		public const char TOKEN_SEPARATOR_PARTY = '@';
		public const char TOKEN_SEPARATOR_PLAYERS_IDS = '~';

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static PhotonController _instance;

		public static PhotonController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(PhotonController)) as PhotonController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						DontDestroyOnLoad(container);
						container.name = "PhotonController";
						_instance = container.AddComponent(typeof(PhotonController)) as PhotonController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBER VARIABLES
		// ----------------------------------------------	
		internal bool m_socketConnected = false;

		private int m_uniqueNetworkID = -1;
		private int m_idNetworkServer = -1;
        private bool m_isConnected = false;
        private bool m_requestInitialitzationReport = false;

        private string m_uidPlayer = "null";
        private string m_serverIPAddress = "";

        private int m_room = -1;
		private int m_hostRoomID = -1;
        private int m_totalNumberOfPlayers = -1;
        private List<ItemMultiObjectEntry> m_events = new List<ItemMultiObjectEntry>();
        private List<ItemMultiTextEntry> m_roomsLobby = new List<ItemMultiTextEntry>();

		private List<PlayerConnectionData> m_playersConnections = new List<PlayerConnectionData>();

		public bool SocketConnected
		{
			get { return m_socketConnected; }
		}
		public int UniqueNetworkID
		{
			get { return m_uniqueNetworkID; }
        }
		public int ServerNetworkID
		{
			get { return m_idNetworkServer; }
		}
		public List<ItemMultiTextEntry> RoomsLobby
		{
			get { return m_roomsLobby; }
		}
        public bool IsConnected
        {
            get { return m_isConnected; }
        }
        public string ServerIPAdress
        {
            get { return m_serverIPAddress; }
            set { m_serverIPAddress = value; }
        }

        // -------------------------------------------
        /* 
		 * Set up the connection with the server
		 */
        public void Login()
		{
            PhotonNetwork.LocalPlayer.NickName = Utilities.RandomCodeGeneration(UnityEngine.Random.Range(100, 999).ToString());
            if (m_serverIPAddress.Length > 0)
            {
                AppSettings appSet = new AppSettings();
                appSet.AppIdRealtime = m_serverIPAddress;
                PhotonNetwork.ConnectUsingSettings(appSet);
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }

            NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
        }

        private bool m_hasBeenDestroyed = false;

		// -------------------------------------------
		/* 
		 * Release resources
		 */
		public void Destroy()
		{
			if (m_hasBeenDestroyed) return;
			m_hasBeenDestroyed = true;

            PhotonNetwork.Disconnect();

            if (m_uniqueNetworkID != -1)
            {
                try { PhotonMessageHUB.Instance.Destroy(); } catch (Exception err) { };
            }

            NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS);
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
			UIEventController.Instance.UIEvent -= OnUIEvent;
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
            Destroy(_instance.gameObject);
			_instance = null;

            if (DEBUG) Debug.LogError("PhotonController::Destroy::PHOTON CONNECTION HAS BEEN SUCCESSFULLY DESTROYED!!!!!!!!!!!!!!!!!!!!!!!!");
		}

		// -------------------------------------------
		/* 
		 * Returns if the current machine acts as a server machine
		 */
		public bool IsServer()
		{
			return PhotonNetwork.IsMasterClient;
		}

        // -------------------------------------------
        /* 
		 * CreateRoom
		 */
        public void CreateRoom(string _nameLobby, int _finalNumberOfPlayers, string _extraData = "")
		{
            if (m_totalNumberOfPlayers == -1)
            {
                m_totalNumberOfPlayers = _finalNumberOfPlayers;
                ExitGames.Client.Photon.Hashtable properties = null;
                if (_extraData != null)
                {
                    if (_extraData.Length > 0)
                    {
                        properties = new ExitGames.Client.Photon.Hashtable { { "extraData", "" } };
                    }
                }
                RoomOptions options = null;
                if (properties != null)
                {
                    options = new RoomOptions { MaxPlayers = (byte)_finalNumberOfPlayers, PlayerTtl = 10000, CustomRoomProperties = properties };
                }
                else
                {
                    options = new RoomOptions { MaxPlayers = (byte)_finalNumberOfPlayers, PlayerTtl = 10000 };
                }
                PhotonNetwork.CreateRoom(_nameLobby, options, null);
                if (DEBUG) Debug.LogError("PhotonController::CreateRoom::CREATING THE ROOM["+ _nameLobby + "] WITH PLAYERS["+ m_totalNumberOfPlayers + "]...");
            }
        }

        // -------------------------------------------
        /* 
		 * GetListRooms
		 */
        public void GetListRooms()
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
                if (DEBUG) Debug.LogError("PhotonController::GetListRooms:REQUEST TO JOIN THE LOBBY");
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS);
            }
        }

        // -------------------------------------------
        /* 
		 * JoinRoom
		 */
        public void JoinRoom(string _room, string _players, string _extraData)
        {
            if (m_totalNumberOfPlayers == -1)
            {
                m_totalNumberOfPlayers = -999999;
                if (PhotonNetwork.InLobby)
                {
                    PhotonNetwork.LeaveLobby();
                }

                PhotonNetwork.JoinRoom(_room);
                if (DEBUG) Debug.LogError("PhotonController::JoinRoom::JOINING THE ROOM....");
            }
        }

        // -------------------------------------------
        /* 
		* New client has been connected
		*/
        public bool ClientNewConnection(int _idConnection)
        {
            PlayerConnectionData newPlayerConnection = new PlayerConnectionData(_idConnection, null);
            if (!m_playersConnections.Contains(newPlayerConnection))
            {
                m_playersConnections.Add(newPlayerConnection);
                string eventConnected = CommunicationsController.CreateJSONMessage(_idConnection, CommunicationsController.MESSAGE_TYPE_NEW_CONNECTION);
                Debug.Log(eventConnected);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG, eventConnected);
                Debug.LogError("ClientNewConnection::m_playersConnections[" + m_playersConnections.Count + "] NEW CONNECTION ID["+ _idConnection + "]");
                return true;
            }
            else
            {
                return false;
            }
        }

        // -------------------------------------------
        /* 
		 * A client has been disconnected
		 */
        public void ClientDisconnected(int _idConnection)
        {
            if (RemoveConnection(_idConnection))
            {
                string eventDisconnected = CommunicationsController.CreateJSONMessage(_idConnection, CommunicationsController.MESSAGE_TYPE_DISCONNECTION);
                Debug.Log(eventDisconnected);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG, eventDisconnected);
                NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED, _idConnection);
            }
        }

        // -------------------------------------------
        /* 
		 * Remove a player connection by id
		 */
        private bool RemoveConnection(int _idConnection)
        {
            for (int i = 0; i < m_playersConnections.Count; i++)
            {
                if (m_playersConnections[i].Id == _idConnection)
                {
                    m_playersConnections[i].Destroy();
                    m_playersConnections.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }


        // -------------------------------------------
        /* 
		* Manager of global events
		*/
        private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (m_uniqueNetworkID == -1) return;

			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS)
			{
				Destroy();
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED)
			{
				int networkIDPlayer = int.Parse((string)_list[0]);
				if (networkIDPlayer != m_uniqueNetworkID)
				{
					if (ClientNewConnection(networkIDPlayer))
					{
						NetworkEventController.Instance.DelayNetworkEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED, 0.2f, m_uniqueNetworkID.ToString());
					}
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_PLAYER_HAS_BEEN_DESTROYED)
			{
				int networkIDPlayer = int.Parse((string)_list[0]);
				ClientDisconnected(networkIDPlayer);
			}
			if (_nameEvent == NetworkEventController.EVENT_STREAMSERVER_REPORT_CLOSED_STREAM)
			{
				int networkIDPlayer = int.Parse((string)_list[0]);
				ClientDisconnected(networkIDPlayer);
			}
			if (!_isLocalEvent)
			{
				m_events.Add(new ItemMultiObjectEntry(_nameEvent, _networkOriginID, _networkTargetID, _list));
			}
			else
			{
            }
		}


		// -------------------------------------------
		/* 
		 * Manager of ui events
		 */
		private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REQUEST_LIST_USERS)
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
			}
		}

        // -------------------------------------------
        /* 
		 * Manager of basic system events
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_PHOTONCONTROLLER_GAME_STARTED)
            {
                NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED, m_uniqueNetworkID);
                bool isServer = true;
                if (_list.Length > 0)
                {
                    isServer = (bool)_list[0];
                }
                if (isServer)
                {
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(CommunicationsController.EVENT_COMMSCONTROLLER_SET_UP_IS_SERVER);
                }                
            }
        }

        // -------------------------------------------
        /* 
		 * OnConnectedToMaster
		 */
        public override void OnConnectedToMaster()
        {
            if (DEBUG) Debug.LogError("PhotonController::OnConnectedToMaster");
            m_isConnected = true;
            m_requestInitialitzationReport = true;
            Invoke("GetListRooms", 0.2f);
        }

        // -------------------------------------------
        /* 
		 * OnRoomListUpdate
		 */
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            if (DEBUG) Debug.LogError("PhotonController::OnRoomListUpdate:roomList.Count["+ roomList.Count + "]");
            m_roomsLobby.Clear();
            for (int i = 0; i < roomList.Count; i++)
            {
                RoomInfo info = roomList[i];
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    continue;
                }

                string extraData = "extraData";
                if (info.CustomProperties != null)
                {
                    ExitGames.Client.Photon.Hashtable customData = info.CustomProperties;
                    foreach (object k in customData)
                    {
                        extraData = (string)customData[(string)k];
                    }
                    if (extraData.Length == 0)
                    {
                        extraData = "extraData";
                    }
                }

                ItemMultiTextEntry item = new ItemMultiTextEntry(false.ToString(), i.ToString(), info.Name, extraData);
                m_roomsLobby.Add(item);
            }
            if (DEBUG) Debug.LogError("PhotonController::OnRoomListUpdate::REPORTING LIST OF ROOMS["+ m_roomsLobby.Count + "]");
            UIEventController.Instance.DispatchUIEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS);
            if (m_requestInitialitzationReport)
            {
                m_requestInitialitzationReport = false;
                UIEventController.Instance.DispatchUIEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID, -1);
            }
        }

        // -------------------------------------------
        /* 
		 * OnLeftLobby
		 */
        public override void OnLeftLobby()
        {
            if (DEBUG) Debug.LogError("PhotonController::OnLeftLobby");
        }

        // -------------------------------------------
        /* 
		 * OnLeftLobby
		 */
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            if (DEBUG) Debug.LogError("PhotonController::OnCreateRoomFailed");
        }

        // -------------------------------------------
        /* 
		 * OnLeftLobby
		 */
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            if (DEBUG) Debug.LogError("PhotonController::OnJoinRoomFailed");
        }

        // -------------------------------------------
        /* 
		 * OnJoinedRoom
		 */
        public override void OnJoinedRoom()
        {
            if (m_uniqueNetworkID == -1)
            {
                m_uniqueNetworkID = PhotonNetwork.LocalPlayer.ActorNumber;
                if (DEBUG) Debug.LogError("PhotonController::OnJoinedRoom::UniqueNetworkID[" + UniqueNetworkID + "]::MasterClient[" + PhotonNetwork.IsMasterClient + "]");
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Instantiate("PhotonMessageHUB", Vector3.zero, Quaternion.identity);
                }
                UIEventController.Instance.DispatchUIEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM, m_totalNumberOfPlayers);
            }
        }

        // -------------------------------------------
        /* 
		 * SetRoomExtraData
		 */
        public void SetRoomExtraData(string _extraData)
        {
            if (_extraData != null)
            {
                if (_extraData.Length > 0)
                {
                    ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable { { "extraData", _extraData } };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
                    // Debug.LogError("PhotonController::SetRoomExtraData::properties.Count[" + PhotonNetwork.CurrentRoom.CustomProperties.Count + "]!!!!!!!!!!!");
                }
            }
        }

        // -------------------------------------------
        /* 
		 * GetRoomExtraData
		 */
        public string GetRoomExtraData()
        {
            string extraData = "";
            if (PhotonNetwork.CurrentRoom.CustomProperties != null)
            {
                ExitGames.Client.Photon.Hashtable customData = PhotonNetwork.CurrentRoom.CustomProperties;
                foreach (object k in customData)
                {
                    extraData = (string)customData[(string)k];
                }
            }
            return extraData;
        }

        // -------------------------------------------
        /* 
		 * OnLeftRoom
		 */
        public override void OnLeftRoom()
        {
            if (DEBUG) Debug.LogError("PhotonController::OnLeftRoom");
        }

        // -------------------------------------------
        /* 
		 * OnPlayerEnteredRoom
		 */
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            int otherNetworkID = newPlayer.ActorNumber;
            if (DEBUG) Debug.LogError("PhotonController::OnPlayerEnteredRoom::otherNetworkID["+ otherNetworkID + "]");
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("EVENT_CLIENT_TCP_CONNECTED_ROOM::ASSIGNED OTHER CLIENT NUMBER[" + otherNetworkID + "] IN THE ROOM[" + m_room + "] WHERE THE SERVER IS[" + m_idNetworkServer + "]------------");
#endif
            NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED, otherNetworkID.ToString());
        }

        // -------------------------------------------
        /* 
		 * OnPlayerLeftRoom
		 */
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (DEBUG) Debug.LogError("PhotonController::OnPlayerLeftRoom");
        }

        // -------------------------------------------
        /* 
		 * OnMasterClientSwitched
		 */
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (DEBUG) Debug.LogError("PhotonController::OnMasterClientSwitched");
        }

        // -------------------------------------------
        /* 
		 * OnPlayerPropertiesUpdate
		 */
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (DEBUG) Debug.LogError("PhotonController::OnPlayerPropertiesUpdate");
        }

        // -------------------------------------------
        /* 
		* GetRoomIDByName
		*/
        public int GetRoomIDByName(string _roomName)
        {
            foreach(ItemMultiTextEntry room in m_roomsLobby)
            {
                if (room.Items[2] == _roomName)
                {
                    return int.Parse(room.Items[1]);
                }
            }
            return -1;
        }

        // -------------------------------------------
        /* 
		* GetExtraDataForRoom
		*/
        public string GetExtraDataForRoom(int _roomID)
        {
            foreach (ItemMultiTextEntry room in m_roomsLobby)
            {
                if (int.Parse(room.Items[1]) == _roomID)
                {
                    return room.Items[3];
                }
            }
            return "";
        }
        

        // -------------------------------------------
        /* 
		* Display information about the operation mode
		*/
        void OnGUI()
        {
#if UNITY_EDITOR
            if (MultiplayerConfiguration.DEBUG_MODE)
            {
                GUILayout.BeginVertical();
                if (m_uniqueNetworkID == -1)
                {
                    GUILayout.Box(new GUIContent("--[PHOTON]--SERVER IS SETTING UP. WAIT..."));
                }
                else
                {
                    GUILayout.Box(new GUIContent("++[PHOTON]++MACHINE CONNECTION[" + m_uniqueNetworkID + "][" + (IsServer() ? "SERVER" : "CLIENT") + "]"));
                }
                GUILayout.EndVertical();
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        public void Update()
		{
            if (m_uniqueNetworkID != -1)
            {
                while (m_events.Count > 0)
                {
                    ItemMultiObjectEntry item = m_events[0];
                    m_events.RemoveAt(0);
                    int uniqueNetworkID = (int)item.Objects[1];
                    string[] paramsEvent = new string[0];
                    if (item.Objects.Count > 3)
                    {
                        paramsEvent = (string[])item.Objects[3];
                    }
                    PhotonMessageHUB.Instance.PrepareMessage((string)item.Objects[0], uniqueNetworkID, (int)item.Objects[2], paramsEvent);
                }
            }
        }
	}
}
#endif