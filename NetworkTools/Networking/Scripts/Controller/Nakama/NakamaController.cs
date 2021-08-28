#if ENABLE_NAKAMA
using Nakama;
using Nakama.TinyJson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using YourCommonTools;

namespace YourNetworkingTools
{
	public class OpCodes
	{
		public const long AssignUID = 1;
		public const long Message = 2;
		public const long Transform = 3;
	}

	/******************************************
	 * 
	 * NakamaController
	 * 
	 * Singleton that handles the sending and receiving of Nakama comms
	 * 
	 * @author Esteban Gallardo
	 */
	public class NakamaController : MonoBehaviour
	{
		public const bool DEBUG = false;

		// ----------------------------------------------
		// PUBLIC EVENTS
		// ----------------------------------------------	
		public const string EVENT_NAKAMACONTROLLER_GAME_STARTED = "EVENT_NAKAMACONTROLLER_GAME_STARTED";
		public const string EVENT_NAKAMACONTROLLER_TIMEOUT_SEND_UIDS = "EVENT_NAKAMACONTROLLER_TIMEOUT_SEND_UIDS";
		public const string EVENT_NAKAMACONTROLLER_SEND_INITIAL_ROOMS = "EVENT_NAKAMACONTROLLER_SEND_INITIAL_ROOMS";

		// ----------------------------------------------
		// PUBLIC CONSTANTS
		// ----------------------------------------------
		public const string ROOMS_CHAT_MESSAGE = "rooms";
		public const string REMOVE_ROOMS_MESSAGE = "remroom";
		public const string LEAVE_CHAT_MESSAGE = "leavechat";
		public const char ROOMS_SEPARATOR = ';';
		public const char PARAM_SEPARATOR = ',';

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static NakamaController _instance;

		public static NakamaController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(NakamaController)) as NakamaController;
					if (_instance == null)
                    {
						GameObject container = Instantiate(Resources.Load("Nakama/NakamaController")) as GameObject;
						DontDestroyOnLoad(container);
						container.name = "NakamaController";
						_instance = container.GetComponent(typeof(NakamaController)) as NakamaController;
					}					
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PUBLIC VARIABLES
		// ----------------------------------------------	
		public NakamaConnection NakamaConnection;

		// ----------------------------------------------
		// PRIVATE VARIABLES
		// ----------------------------------------------	
		private List<NakamaPlayer> m_players = new List<NakamaPlayer>();
		private IUserPresence m_localUser;
		private IMatch m_currentMatch;
		private bool m_isInLobby = false;
		private bool m_hasBeenInitialized = false;
		private bool m_hasBeenDestroyed = false;

		private string m_roomName = "";
		private int m_uid = -1;
		private bool m_isGameCreator = false;
		private int m_totalPlayers = -1;

		private string m_roomsBuffer = "";
		private List<ItemMultiTextEntry> m_roomsLobby = new List<ItemMultiTextEntry>();
		private List<PlayerConnectionData> m_playersConnections = new List<PlayerConnectionData>();

		private List<ItemMultiObjectEntry> m_events = new List<ItemMultiObjectEntry>();

		private UnityMainThreadDispatcher m_mainThread;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool IsConnected
        {
			get { return m_isInLobby; }
        }
		public int UniqueNetworkID
        {
			get { return m_uid; }
        }

		public List<ItemMultiTextEntry> RoomsLobby
		{
			get { return m_roomsLobby; }
		}

		public string ServerIPAddress
        {
			get { return ""; }
			set { }
        }

		public string RoomsBuffer
        {
			get { return m_roomsBuffer; }
			set
            {
				m_roomsBuffer = value;
				m_roomsLobby = new List<ItemMultiTextEntry>();
				if (RoomsBuffer.IndexOf(ROOMS_SEPARATOR) != -1)
                {
					string[] currentRooms = RoomsBuffer.Split(ROOMS_SEPARATOR);
					if (currentRooms.Length > 0)
                    {
						for (int i = 0; i < currentRooms.Length; i++)
						{
							string[] entryRoom = currentRooms[i].Split(PARAM_SEPARATOR);
							if (entryRoom.Length == 3)
                            {
								string nameRoom = entryRoom[0];
								string totalPlayers = entryRoom[1];
								string extraData = entryRoom[2];
								extraData = ((extraData.Length == 0)?"extraData": extraData);
								ItemMultiTextEntry item = new ItemMultiTextEntry(false.ToString(), i.ToString(), nameRoom, extraData, totalPlayers.ToString());
								m_roomsLobby.Add(item);
							}
						}
						if (DEBUG)
                        {
							Debug.LogError("+++++++++++++TOTAL ROOMS[" + m_roomsLobby.Count + "]");
							for (int j = 0; j < m_roomsLobby.Count; j++)
							{
								Debug.LogError("m_roomsLobby[" + j + "]=" + m_roomsLobby[j].Package());
							}
						}
					}
				}
			}
        }

		// -------------------------------------------
		/* 
		 * IsServer
		 */
		public bool IsServer()
		{
			return m_isGameCreator;
		}

		// -------------------------------------------
		/* 
		 * Initialitzation
		 */
		public async void Initialitzation()
		{
			if (m_hasBeenInitialized) return;
			m_hasBeenInitialized = true;

			if (DEBUG) Debug.LogError("NakamaController::Initialitzation");

			m_mainThread = UnityMainThreadDispatcher.Instance();

			// Connect to the Nakama server.
			await NakamaConnection.Connect();

			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
			BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);

			// Setup network event handlers.
			NakamaConnection.Socket.ReceivedMatchmakerMatched += ActionReceivedMatchmakerMatched;
			NakamaConnection.Socket.ReceivedMatchPresence += ActionReceivedMatchPresence;
			NakamaConnection.Socket.ReceivedMatchState += ActionReceivedMatchState;

			NakamaConnection.Socket.ReceivedChannelPresence += ActionReceivedChannelPresence;
			NakamaConnection.Socket.ReceivedChannelMessage += ActionReceivedChannelMessage;

			await NakamaConnection.JoinMainChat();
		}

		private void ActionReceivedMatchmakerMatched(IMatchmakerMatched m) { m_mainThread.Enqueue(() => OnReceivedMatchmakerMatched(m)); }
		private void ActionReceivedMatchPresence(IMatchPresenceEvent m) { m_mainThread.Enqueue(() => OnReceivedMatchPresence(m)); }
		private void ActionReceivedMatchState(IMatchState m) { m_mainThread.Enqueue(() => OnReceivedMatchState(m)); }
		private void ActionReceivedChannelPresence(IChannelPresenceEvent m) { m_mainThread.Enqueue(() => OnReceivedChannelPresence(m)); }
		private void ActionReceivedChannelMessage(IApiChannelMessage m) { m_mainThread.Enqueue(() => OnReceivedChannelMessage(m)); }

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
        {
			if (m_hasBeenDestroyed) return;
			m_hasBeenDestroyed = true;

			NakamaConnection.Socket.ReceivedMatchmakerMatched -= ActionReceivedMatchmakerMatched;
			NakamaConnection.Socket.ReceivedMatchPresence -= ActionReceivedMatchPresence;
			NakamaConnection.Socket.ReceivedMatchState -= ActionReceivedMatchState;

			NakamaConnection.Socket.ReceivedChannelPresence -= ActionReceivedChannelPresence;
			NakamaConnection.Socket.ReceivedChannelMessage -= ActionReceivedChannelMessage;

			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
			BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;

			m_mainThread.Clear();

			QuitMatch();

			GameObject.Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * OnReceivedChannelPresence
		 */
		private async void OnReceivedChannelPresence(IChannelPresenceEvent m)
		{
			if (m.Leaves.ToArray().Length == 0)
			{
				m_isInLobby = true;
				if (DEBUG) Debug.LogError("RECEIVED CHANNEL PRESENCE::ChannelId=" + m.ChannelId + "::SENDING ROOMS=" + RoomsBuffer);
				if (NakamaConnection.ConnectedToMainChat)
                {
					await NakamaConnection.SendMainChatMessage(ROOMS_CHAT_MESSAGE, RoomsBuffer);
				}
				else
                {
					BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_NAKAMACONTROLLER_SEND_INITIAL_ROOMS, 1);
                }
			}
		}

		// -------------------------------------------
		/* 
		 * OnReceivedChannelMessage
		 */
		private async void OnReceivedChannelMessage(IApiChannelMessage m)
		{
			Dictionary<string, object> message = (Dictionary<string, object>)Json.Deserialize(m.Content);
			foreach (KeyValuePair<string, object> item in message)
			{
				if (item.Key == ROOMS_CHAT_MESSAGE)
				{
					string buf = (string)item.Value;
					if (buf.Length > 0)
                    {
						RoomsBuffer = (string)item.Value;
					}					
					if (GameObject.FindObjectOfType<YourNetworkTools>() == null)
					{
						UIEventController.Instance.DispatchUIEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID);
					}
					else
					{
						NetworkEventController.Instance.DelayLocalEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID, 0.1f);
					}
					UIEventController.Instance.DelayUIEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS, 0.3f);
					if (DEBUG) Debug.LogError("ROOMS_CHAT_MESSAGE::Message KEY=" + item.Key + "::ROOMS=" + RoomsBuffer);
				}
				else if (item.Key == REMOVE_ROOMS_MESSAGE)
				{
					string roomToDelete = (string)item.Value;
					string[] currentRooms = RoomsBuffer.Split(ROOMS_SEPARATOR);
					string finalRooms = "";
					bool hasBeenDeleted = false;
					for (int i = 0; i < currentRooms.Length; i++)
					{
						if (currentRooms[i].IndexOf(roomToDelete) != -1)
						{
							if (DEBUG) Debug.LogError("------------------DELETED ROOM=" + roomToDelete);
							hasBeenDeleted = true;
						}
						else
						{
							finalRooms = currentRooms[i] + ((finalRooms.Length > 0) ? ROOMS_SEPARATOR +"" : "") + finalRooms;
						}                    
					}
					if (hasBeenDeleted)
					{
						RoomsBuffer = finalRooms;
						if (DEBUG) Debug.LogError("******************ROOMS AFTER DELETE=" + finalRooms);
						if (m_localUser != null)
						{
							if (NakamaConnection.Channel != null)
							{
								await NakamaConnection.LeaveMainChat();
							}
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnReceivedMatchmakerMatched
		 */
		private async void OnReceivedMatchmakerMatched(IMatchmakerMatched matched)
		{
			// Cache a reference to the local user.
			m_localUser = matched.Self.Presence;

			// Debug.LogError("MatchId=" + matched.MatchId);
			// Debug.LogError("Token=" + matched.Token);
			// Debug.LogError("Ticket=" + matched.Ticket);

			// Join the match.
			var match = await NakamaConnection.Socket.JoinMatchAsync(matched);

			// Spawn a player instance for each connected user.
			foreach (var user in match.Presences)
			{
				RegisterPlayer(match.Id, user);
			}

			m_currentMatch = match;

			await NakamaConnection.SendMainChatMessage(REMOVE_ROOMS_MESSAGE, m_roomName);
		}

		// -------------------------------------------
		/* 
		 * OnReceivedMatchPresence
		 */
		private void OnReceivedMatchPresence(IMatchPresenceEvent _matchPresenceEvent)
		{
			if (DEBUG) Debug.LogError("NakamaController::IMatchPresenceEvent::JOINS["+ _matchPresenceEvent.Joins.ToList().Count + "]");

			// For each new user that joins, spawn a player for them.
			foreach (var user in _matchPresenceEvent.Joins)
			{
				RegisterPlayer(_matchPresenceEvent.MatchId, user);
			}

			// For each player that leaves, despawn their player.
			foreach (var user in _matchPresenceEvent.Leaves)
			{
				for (int i = 0; i < m_players.Count; i++)
				{
					NakamaPlayer player = m_players[i];
					if (player.UserPresence.SessionId == user.SessionId)
                    {
						m_players.RemoveAt(i);
						i--;
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * GetNakamaPlayer
		 */
		private NakamaPlayer GetNakamaPlayer(string _userID)
		{
			for (int i = 0; i < m_players.Count; i++)
			{
				NakamaPlayer player = m_players[i];
				if (player.UserPresence.UserId == _userID)
				{
					return player;
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		 * GetRoomByName
		 */
		private ItemMultiTextEntry GetRoomByName(string _roomName)
        {
			for (int i = 0; i < m_roomsLobby.Count; i++)
            {
				if (m_roomsLobby[i].Items[2].IndexOf(_roomName) != -1)
                {
					return m_roomsLobby[i];
				}
            }
			return null;
		}

		// -------------------------------------------
		/* 
		 * GetRoomIDByName
		 */
		public int GetRoomIDByName(string _roomName)
		{
			for (int i = 0; i < m_roomsLobby.Count; i++)
			{
				if (m_roomsLobby[i].Items[2].IndexOf(_roomName) != -1)
				{
					return i;
				}
			}
			return -1;
		}

		// -------------------------------------------
		/* 
		 * GetExtraDataForRoom
		 */
		public string GetExtraDataForRoom(int _idRoom)
        {
			return m_roomsLobby[_idRoom].Items[3];
		}

		// -------------------------------------------
		/* 
		* OnReceivedMatchState
		*/
		private void OnReceivedMatchState(IMatchState _matchState)
		{
			OnProcessReceivedMatchState(_matchState.UserPresence.SessionId, _matchState.OpCode, System.Text.Encoding.UTF8.GetString(_matchState.State));
		}

		// -------------------------------------------
		/* 
		* OnReceivedMatchState
		*/
		private void OnProcessReceivedMatchState(string _userSessionId, long _opCode, string _matchStateData)
		{
			Dictionary<string, string> state = _matchStateData.Length > 0 ? _matchStateData.FromJson<Dictionary<string, string>>() : null;

			// Decide what to do based on the Operation Code as defined in OpCodes.
			switch (_opCode)
			{
				case OpCodes.AssignUID:
					BasicSystemEventController.Instance.ClearBasicSystemEvents(EVENT_NAKAMACONTROLLER_TIMEOUT_SEND_UIDS);
					foreach (KeyValuePair<string, string> playerUID in state)
                    {
						NakamaPlayer nakamaPlayer = GetNakamaPlayer(playerUID.Key);
						if (nakamaPlayer != null)
                        {
							nakamaPlayer.UID = int.Parse(playerUID.Value);
							if (nakamaPlayer.Equals(m_localUser))
                            {
								m_uid = nakamaPlayer.UID;
								if (DEBUG) Debug.LogError("+++++++++++++++++++ASSIGNED UID["+ m_uid + "]");
							}
						}
					}
					BasicSystemEventController.Instance.DelayBasicSystemEvent(NakamaController.EVENT_NAKAMACONTROLLER_GAME_STARTED, 0.1f + (m_uid * 0.2F));
					break;

				case OpCodes.Message:
					string eventName = "";
					state.TryGetValue(MatchDataJson.EVENTNAME_KEY, out eventName);
					string origin = "";
					state.TryGetValue(MatchDataJson.ORIGIN_KEY, out origin);
					string target = "";
					state.TryGetValue(MatchDataJson.TARGET_KEY, out target);
					string data = "";
					state.TryGetValue(MatchDataJson.DATA_KEY, out data);

					// Debug.LogError("+++++++++++++++++++MESSAGE RECEIVED::eventName[" + eventName + "]::data["+ data + "]");

					if (eventName.Length > 0)
                    {
						string[] paramData = data.Split(ClientTCPEventsController.TOKEN_SEPARATOR_EVENTS);
						NetworkEventController.Instance.DispatchCustomNetworkEvent(eventName, true, int.Parse(origin), int.Parse(target), paramData);
					}
					break;

				case OpCodes.Transform:
					string netid = "";
					state.TryGetValue(MatchDataJson.NETID_KEY, out netid);
					string uid = "";
					state.TryGetValue(MatchDataJson.UID_KEY, out uid);
					string indexPrefab = "";
					state.TryGetValue(MatchDataJson.INDEX_KEY, out indexPrefab);
					string position = "";
					state.TryGetValue(MatchDataJson.POSITION_KEY, out position);
					string rotation = "";
					state.TryGetValue(MatchDataJson.ROTATION_KEY, out rotation);
					string scale = "";
					state.TryGetValue(MatchDataJson.SCALE_KEY, out scale);

					NetworkEventController.Instance.DispatchLocalEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_TRANSFORM_DATA, int.Parse(netid), int.Parse(uid), int.Parse(indexPrefab), Utilities.StringToVector3(position), Utilities.StringToVector3(rotation), Utilities.StringToVector3(scale));
					break;

				default:
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * SpawnPlayer
		 *		<param name="matchId">The match the player is connected to.</param>
		 *		<param name="user">The player's network presence data.</param>
		 */
		private async void RegisterPlayer(string _matchId, IUserPresence _user)
		{
			if (DEBUG) Debug.LogError("+++++++++++++++++++++++++++++++++++++++++++++REGISTERPLAYER::_user.UserId=" + _user.UserId);

			NakamaPlayer newPlayer = new NakamaPlayer(_user.UserId, _matchId, _user);

			bool found = false;
			foreach (NakamaPlayer player in m_players)
            {
				if (player.Equals(newPlayer))
				{
					found = true;
				}
			}

			if (!found)
            {
				m_players.Add(newPlayer);
			}

			if (m_isGameCreator)
            {
				if (m_players.Count == m_totalPlayers)
                {
					if (DEBUG) Debug.LogError("RegisterPlayer::SENDING UIDS");
					await SendUIDsPlayers();
				}
			}
		}

		// -------------------------------------------
		/* 
		 * SendUIDsPlayers
		 */
		private async Task SendUIDsPlayers()
        {
			List<string> uids = new List<string>();
			foreach (NakamaPlayer player in m_players)
			{
				uids.Add(player.ID);
			}
			BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_NAKAMACONTROLLER_TIMEOUT_SEND_UIDS, 2);
			await SendMatchStateAsync(OpCodes.AssignUID, MatchDataJson.AssignUIDS(uids.ToArray()), true);
		}

		// -------------------------------------------
		/* 
		 * QuitMatch
		 */
		public async Task QuitMatch()
		{
			if (m_currentMatch != null)
            {
				await NakamaConnection.Disconnect(m_currentMatch);

				m_currentMatch = null;
				m_localUser = null;

				m_players.Clear();
			}
		}

		// -------------------------------------------
		/* 
		 * SendMatchStateAsync
		 */
		public async Task SendMatchStateAsync(long opCode, string state, bool sendLocal)
		{
			if (m_currentMatch != null)
            {
				await NakamaConnection.Socket.SendMatchStateAsync(m_currentMatch.Id, opCode, state);
				if (sendLocal)
                {
					OnProcessReceivedMatchState(m_currentMatch.Self.SessionId, opCode, state);
				}				
			}			
		}

		// -------------------------------------------
		/* 
		 * SendMatchState
		 */
		public void SendMatchState(long opCode, string state, bool sendLocal)
		{
			if (m_currentMatch != null)
            {
				NakamaConnection.Socket.SendMatchStateAsync(m_currentMatch.Id, opCode, state);
				if (sendLocal)
                {
					OnProcessReceivedMatchState(m_currentMatch.Self.SessionId, opCode, state);
				}					
			}				
		}

		// -------------------------------------------
		/* 
		 * FindMatch
		 */
		public async Task FindMatch(string _roomName, int _totalPlayers, string _extraData = "")
		{
			m_roomName = _roomName;
			string newRooms = RoomsBuffer;
			if (RoomsBuffer.IndexOf(m_roomName) == -1)
			{
				m_isGameCreator = true;
				m_totalPlayers = _totalPlayers;
				newRooms = m_roomName + PARAM_SEPARATOR + _totalPlayers + PARAM_SEPARATOR + _extraData;
				newRooms += ROOMS_SEPARATOR + RoomsBuffer;
			}
            else
            {
				ItemMultiTextEntry roomFound = GetRoomByName(_roomName);
				m_totalPlayers = int.Parse(roomFound.Items[4]);
			}
			if (DEBUG) Debug.LogError("+++++++++++++++++++++++++FindMatch::_roomName[" + _roomName + "]::m_totalPlayers["+ m_totalPlayers + "]");
			await NakamaConnection.FindMatch(m_roomName, m_totalPlayers, m_totalPlayers);

			UIEventController.Instance.DelayUIEvent(ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM, 0.2f, m_totalPlayers);

			// Update the lobby list of rooms
			await NakamaConnection.SendMainChatMessage(ROOMS_CHAT_MESSAGE, newRooms);
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
		 * SendTransform
		 */
		public void SendTransform(int _netID, int _uID, int _indexPrefab, Vector3 _position, Vector3 _forward, Vector3 _scale)
        {
			if (m_uid != -1)
            {
				m_events.Add(new ItemMultiObjectEntry(OpCodes.Transform, MatchDataJson.Transform(_netID, _uID, _indexPrefab, _position, _forward, _scale)));
			}			
		}

		// -------------------------------------------
		/* 
		 * OnNetworkEvent
		 */
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS)
			{
				Destroy();
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED)
			{
				int networkIDPlayer = int.Parse((string)_list[0]);
				if (networkIDPlayer != m_uid)
				{
					if (ClientNewConnection(networkIDPlayer))
					{
						NetworkEventController.Instance.DelayNetworkEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED, 0.2f, m_uid.ToString());
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
				m_events.Add(new ItemMultiObjectEntry(OpCodes.Message, MatchDataJson.Message(_nameEvent, _networkOriginID, _networkTargetID, _list)));
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of basic system events
		 */
		private void OnBasicSystemEvent(string _nameEvent, object[] _list)
		{
			if (_nameEvent == EVENT_NAKAMACONTROLLER_SEND_INITIAL_ROOMS)
            {
				if (NakamaConnection.ConnectedToMainChat)
				{
					NakamaConnection.SendMainChatMessage(ROOMS_CHAT_MESSAGE, RoomsBuffer);
				}
				else
				{
					BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_NAKAMACONTROLLER_SEND_INITIAL_ROOMS, 1);
				}
			}
			if (_nameEvent == EVENT_NAKAMACONTROLLER_GAME_STARTED)
			{
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED, m_uid);
				foreach (NakamaPlayer nakamaPlayer in m_players)
                {
					if (!nakamaPlayer.Equals(m_localUser))
                    {
						NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED, 0.1f, nakamaPlayer.UID.ToString());
					}
				}
				bool isServer = m_isGameCreator;
				if (_list.Length > 0)
				{
					isServer = (bool)_list[0];
				}
				if (isServer)
				{
					BasicSystemEventController.Instance.DispatchBasicSystemEvent(CommunicationsController.EVENT_COMMSCONTROLLER_SET_UP_IS_SERVER);
				}
			}
			if (_nameEvent == EVENT_NAKAMACONTROLLER_TIMEOUT_SEND_UIDS)
            {
				if (DEBUG) Debug.LogError("++++++++++++++++++++++++++++++++TIMEOUT FOR UIDS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
				SendUIDsPlayers();
			}
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
				if (m_uid == -1)
				{
					GUILayout.Box(new GUIContent("--[NAKAMA]--SERVER IS SETTING UP. WAIT..."));
				}
				else
				{
					GUILayout.Box(new GUIContent("++[NAKAMA]["+ m_roomName + "]++CONN ID[" + m_uid + "][" + (IsServer() ? "SERVER" : "CLIENT") + "]"));
				}
				GUILayout.EndVertical();
			}
#endif
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		private async void Update()
        {
			if (m_uid != -1)
            {
				while (m_events.Count > 0)
				{
					ItemMultiObjectEntry newMessage = m_events[0];
					m_events.RemoveAt(0);
					await SendMatchStateAsync((long)newMessage.Objects[0], (string)newMessage.Objects[1], false);
				}
			}
		}
	}
}
#endif