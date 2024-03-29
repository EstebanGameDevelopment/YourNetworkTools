#if ENABLE_NAKAMA
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;
using YourCommonTools;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * NakamaConnection
	 * 
	 * A singleton class that handles all connectivity with the Nakama server.
	 * 
	 * @author Esteban Gallardo
	 */
	[Serializable]
	[CreateAssetMenu]
	public class NakamaConnection : ScriptableObject
	{
		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public string Scheme = "http";
		public string Host = "localhost";
		public int Port = 7350;
		public string ServerKey = "defaultkey";

#if EXTRA_NAKAMA_SESSION
		private const string SessionPrefName = "nakama.extra.session";
		private const string DeviceIdentifierPrefName = "nakama.extra.deviceUniqueIdentifier";
#else
		private const string SessionPrefName = "nakama.session";
		private const string DeviceIdentifierPrefName = "nakama.deviceUniqueIdentifier";
#endif

		public IClient Client;
		public ISession Session;
		public ISocket Socket;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private IChannel m_channel;

		private string m_currentMatchmakingTicket;
		private string m_currentMatchId;

		private bool m_connectedToMainChat = false;

		public IChannel Channel
		{
			get { return m_channel; }
		}
		public bool ConnectedToMainChat
        {
			get { return m_connectedToMainChat; }
        }

		// -------------------------------------------
		/* 
		 * Connect
		 */
		public async Task Connect()
		{
			Client = new Nakama.Client(Scheme, Host, Port, ServerKey, UnityWebRequestAdapter.Instance);

			// Attempt to restore an existing user session.
			if (NakamaController.DEBUG) Debug.LogError("NakamaController::Connect::SessionPrefName[" + SessionPrefName + "]");

			var authToken = PlayerPrefs.GetString(SessionPrefName);
			if (!string.IsNullOrEmpty(authToken))
			{
				var session = Nakama.Session.Restore(authToken);
				if (!session.IsExpired)
				{
					Session = session;
				}
			}

			// If we weren't able to restore an existing session, authenticate to create a new user session.
			if (Session == null)
			{
				string deviceId;

#if UNITY_EDITOR
				deviceId = Utilities.RandomCodeGeneration("user_" + Utilities.GetTimestamp());
#else
				if (PlayerPrefs.HasKey(DeviceIdentifierPrefName))
				{
					deviceId = PlayerPrefs.GetString(DeviceIdentifierPrefName);
				}
				else
				{
					deviceId = SystemInfo.deviceUniqueIdentifier;
					if (deviceId == SystemInfo.unsupportedIdentifier)
					{
						deviceId = System.Guid.NewGuid().ToString();
					}

					PlayerPrefs.SetString(DeviceIdentifierPrefName, deviceId);
				}
#endif

				Session = await Client.AuthenticateDeviceAsync(deviceId);

				if (NakamaController.DEBUG) Debug.LogError("NakamaController::Connect::deviceId[" + deviceId + "]");
				PlayerPrefs.SetString(SessionPrefName, Session.AuthToken);
			}

			// Open a new Socket for realtime communication.
			Socket = Client.NewSocket();
			await Socket.ConnectAsync(Session, true);
		}

		// -------------------------------------------
		/* 
		 * Disconnect
		 */
		public async Task Disconnect(IMatch match)
		{
			await Socket.LeaveMatchAsync(match);

			await Socket.CloseAsync();

			Socket = null;
		}

		// -------------------------------------------
		/* 
		 * Starts looking for a match with a given number of minimum players.
		 */
		public async Task FindMatch(string roomName, int minPlayers, int maxPlayers)
		{
			var matchmakingProperties = new Dictionary<string, string>
			{
				{ "roomname", roomName }
			};

			// Add this client to the matchmaking pool and get a ticket.
			var matchmakerTicket = await Socket.AddMatchmakerAsync("+properties.roomname:"+ roomName, minPlayers, maxPlayers, matchmakingProperties);
			m_currentMatchmakingTicket = matchmakerTicket.Ticket;
		}

		// -------------------------------------------
		/* 
		 * JoinMainChat
		 */
		public async Task JoinMainChat()
		{
			var roomname = "YourNetworkTools";
			var persistence = true;
			var hidden = false;
			m_channel = await Socket.JoinChatAsync(roomname, ChannelType.Room, persistence, hidden);
			Debug.LogFormat("Now connected to channel id: '{0}'", m_channel.Id);
			m_connectedToMainChat = true;
		}

		// -------------------------------------------
		/* 
		 * LeaveMainChat
		 */
		public async Task LeaveMainChat()
		{
			m_connectedToMainChat = false;
			await Socket.LeaveChatAsync(m_channel);
			m_channel = null;
		}

		// -------------------------------------------
		/* 
		 * SendMainChatMessage
		 */
		public async Task SendMainChatMessage(string _title, string _description)
		{
			if (m_connectedToMainChat)
            {
				Dictionary<string, string> content = new Dictionary<string, string> { { _title, _description } };
				string serializedContent = Json.Serialize(content);
				if (m_channel != null)
				{
					var sendAck = await Socket.WriteChatMessageAsync(m_channel.Id, serializedContent);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * CancelMatchmaking
		 */
		public async Task CancelMatchmaking()
		{
			await Socket.RemoveMatchmakerAsync(m_currentMatchmakingTicket);
		}
	}
}
#endif