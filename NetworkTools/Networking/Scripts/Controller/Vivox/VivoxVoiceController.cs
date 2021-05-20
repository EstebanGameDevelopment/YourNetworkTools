using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using UnityEngine;
#if ENABLE_VIVOX
using VivoxUnity;
using YourCommonTools;
#endif

namespace YourNetworkingTools
{
	public class VivoxVoiceController : MonoBehaviour
	{
		public const string EVENT_VIVOXVOICECONTROLLER_STARTED = "EVENT_VIVOXVOICECONTROLLER_STARTED";
		public const string EVENT_VIVOXVOICECONTROLLER_INITIALIZED = "EVENT_VIVOXVOICECONTROLLER_INITIALIZED";
		public const string EVENT_VIVOXVOICECONTROLLER_LOGGED_IN = "EVENT_VIVOXVOICECONTROLLER_LOGGED_IN";
		public const string EVENT_VIVOXVOICECONTROLLER_LOGGED_OUT = "EVENT_VIVOXVOICECONTROLLER_LOGGED_OUT";
		public const string EVENT_VIVOXVOICECONTROLLER_NEW_PARTICIPANT = "EVENT_VIVOXVOICECONTROLLER_NEW_PARTICIPANT";

#if ENABLE_VIVOX
		/// <summary>
		/// Defines properties that can change.  Used by the functions that subscribe to the OnAfterTYPEValueUpdated functions.
		/// </summary>
		public enum ChangedProperty
		{
			None,
			Speaking,
			Typing,
			Muted
		}

		public enum ChatCapability
		{
			TextOnly,
			AudioOnly,
			TextAndAudio
		};

		public delegate void ParticipantValueChangedHandler(string username, ChannelId channel, bool value);
		public event ParticipantValueChangedHandler OnSpeechDetectedEvent;
		public delegate void ParticipantValueUpdatedHandler(string username, ChannelId channel, double value);
		public event ParticipantValueUpdatedHandler OnAudioEnergyChangedEvent;


		public delegate void ParticipantStatusChangedHandler(string username, ChannelId channel, IParticipant participant);
		public event ParticipantStatusChangedHandler OnParticipantAddedEvent;
		public event ParticipantStatusChangedHandler OnParticipantRemovedEvent;

		public delegate void ChannelTextMessageChangedHandler(string sender, IChannelTextMessage channelTextMessage);
		public event ChannelTextMessageChangedHandler OnTextMessageLogReceivedEvent;

		public delegate void LoginStatusChangedHandler();
		public event LoginStatusChangedHandler OnUserLoggedInEvent;
		public event LoginStatusChangedHandler OnUserLoggedOutEvent;

		private Uri _serverUri
		{
			get => new Uri(_server);

			set
			{
				_server = value.ToString();
			}
		}
		[SerializeField]
		private string _server = "https://GETFROMPORTAL.www.vivox.com/api2";
		[SerializeField]
		private string _domain = "GET VALUE FROM VIVOX DEVELOPER PORTAL";
		[SerializeField]
		private string _tokenIssuer = "GET VALUE FROM VIVOX DEVELOPER PORTAL";
		[SerializeField]
		private string _tokenKey = "GET VALUE FROM VIVOX DEVELOPER PORTAL";
		private TimeSpan _tokenExpiration = TimeSpan.FromSeconds(90);

		private Client _client = new Client();
		private AccountId _accountId;

		// Check to see if we're about to be destroyed.
		private static object m_Lock = new object();
		private static VivoxVoiceController m_Instance;

		/// <summary>
		/// Access singleton instance through this propriety.
		/// </summary>
		public static VivoxVoiceController Instance
		{
			get
			{
				lock (m_Lock)
				{
					if (m_Instance == null)
					{
						// Search for existing instance.
						m_Instance = (VivoxVoiceController)FindObjectOfType(typeof(VivoxVoiceController));
					}
					// Make instance persistent even if its already in the scene
					DontDestroyOnLoad(m_Instance.gameObject);
					return m_Instance;
				}
			}
		}


		public LoginState LoginState { get; private set; }
		public ILoginSession LoginSession;
		public VivoxUnity.IReadOnlyDictionary<ChannelId, IChannelSession> ActiveChannels => LoginSession?.ChannelSessions;
		public IAudioDevices AudioInputDevices => _client.AudioInputDevices;
		public IAudioDevices AudioOutputDevices => _client.AudioOutputDevices;

		/// <summary>
		/// Retrieves the first instance of a session that is transmitting. 
		/// </summary>
		public IChannelSession TransmittingSession
		{
			get
			{
				if (_client == null)
					throw new NullReferenceException("client");
				return _client.GetLoginSession(_accountId).ChannelSessions.FirstOrDefault(x => x.IsTransmitting);
			}
			set
			{
				if (value != null)
				{
					_client.GetLoginSession(_accountId).SetTransmissionMode(TransmissionMode.Single, value.Channel);
				}
			}
		}

		void Start()
        {
			BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_VIVOXVOICECONTROLLER_STARTED);
        }

		public void Initialize(string _serverPar, string _domainPar, string _tokenIssuerPar, string _tokenKeyPar)
		{
			_server = _serverPar;
			_domain = _domainPar;
			_tokenIssuer = _tokenIssuerPar;
			_tokenKey = _tokenKeyPar;
			_client.Uninitialize();

			_client.Initialize();

			BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_VIVOXVOICECONTROLLER_INITIALIZED, 0.1F);
		}

		public void Destroy()
        {
			if (m_Instance != null)
            {
				GameObject.Destroy(m_Instance);
				m_Instance = null;

				OnApplicationQuit();
			}
        }

		private void OnApplicationQuit()
		{
			// Needed to add this to prevent some unsuccessful uninit, we can revisit to do better -carlo
			Client.Cleanup();
			if (_client != null)
			{
				VivoxLog("Uninitializing client.");
				_client.Uninitialize();
				_client = null;
			}
		}

		public void Login(string displayName = null)
		{
			string uniqueId = Guid.NewGuid().ToString();
			//for proto purposes only, need to get a real token from server eventually
			_accountId = new AccountId(_tokenIssuer, uniqueId, _domain, displayName);
			LoginSession = _client.GetLoginSession(_accountId);
			LoginSession.PropertyChanged += OnLoginSessionPropertyChanged;
			LoginSession.BeginLogin(_serverUri, LoginSession.GetLoginToken(_tokenKey, _tokenExpiration), SubscriptionMode.Accept, null, null, null, ar =>
			{
				try
				{
					LoginSession.EndLogin(ar);
				}
				catch (Exception e)
				{
					// Handle error 
					VivoxLogError(nameof(e));
					// Unbind if we failed to login.
					LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
					return;
				}
			});
		}

		public void Logout()
		{
			if (LoginSession != null && LoginState != LoginState.LoggedOut && LoginState != LoginState.LoggingOut)
			{
				OnUserLoggedOutEvent?.Invoke();
				LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
				LoginSession.Logout();
			}
		}

		public void JoinChannel(string channelName, ChannelType channelType, ChatCapability chatCapability,
			bool switchTransmission = true, Channel3DProperties properties = null)
		{
			if (LoginState == LoginState.LoggedIn)
			{

				ChannelId channelId = new ChannelId(_tokenIssuer, channelName, _domain, channelType, properties);
				IChannelSession channelSession = LoginSession.GetChannelSession(channelId);
				channelSession.PropertyChanged += OnChannelPropertyChanged;
				channelSession.Participants.AfterKeyAdded += OnParticipantAdded;
				channelSession.Participants.BeforeKeyRemoved += OnParticipantRemoved;
				channelSession.Participants.AfterValueUpdated += OnParticipantValueUpdated;
				channelSession.MessageLog.AfterItemAdded += OnMessageLogRecieved;
				channelSession.BeginConnect(chatCapability != ChatCapability.TextOnly, chatCapability != ChatCapability.AudioOnly, switchTransmission, channelSession.GetConnectToken(_tokenKey, _tokenExpiration), ar =>
				{
					try
					{
						channelSession.EndConnect(ar);
					}
					catch (Exception e)
					{
						// Handle error 
						VivoxLogError($"Could not connect to voice channel: {e.Message}");
						return;
					}
				});
			}
			else
			{
				VivoxLogError("Cannot join a channel when not logged in.");
			}
		}

		public void SendTextMessage(string messageToSend, ChannelId channel, string applicationStanzaNamespace = null, string applicationStanzaBody = null)
		{
			if (ChannelId.IsNullOrEmpty(channel))
			{
				throw new ArgumentException("Must provide a valid ChannelId");
			}
			if (string.IsNullOrEmpty(messageToSend))
			{
				throw new ArgumentException("Must provide a message to send");
			}
			var channelSession = LoginSession.GetChannelSession(channel);
			channelSession.BeginSendText(null, messageToSend, applicationStanzaNamespace, applicationStanzaBody, ar =>
			{
				try
				{
					channelSession.EndSendText(ar);
				}
				catch (Exception e)
				{
					VivoxLog($"SendTextMessage failed with exception {e.Message}");
				}
			});
		}

		public void DisconnectAllChannels()
		{
			if (ActiveChannels?.Count > 0)
			{
				foreach (var channelSession in ActiveChannels)
				{
					channelSession?.Disconnect();
				}
			}
		}

		private void OnMessageLogRecieved(object sender, QueueItemAddedEventArgs<IChannelTextMessage> textMessage)
		{
			ValidateArgs(new object[] { sender, textMessage });

			IChannelTextMessage channelTextMessage = textMessage.Value;
			VivoxLog(channelTextMessage.Message);
			OnTextMessageLogReceivedEvent?.Invoke(channelTextMessage.Sender.DisplayName, channelTextMessage);
		}

		private void OnLoginSessionPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName != "State")
			{
				return;
			}
			var loginSession = (ILoginSession)sender;
			LoginState = loginSession.State;
			VivoxLog("Detecting login session change");
			switch (LoginState)
			{
				case LoginState.LoggingIn:
					{
						VivoxLog("Logging in");
						break;
					}
				case LoginState.LoggedIn:
					{
						VivoxLog("Connected to voice server and logged in.");
						OnUserLoggedInEvent?.Invoke();
						BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_VIVOXVOICECONTROLLER_LOGGED_IN);
						break;
					}
				case LoginState.LoggingOut:
					{
						VivoxLog("Logging out");
						break;
					}
				case LoginState.LoggedOut:
					{
						VivoxLog("Logged out");
						LoginSession.PropertyChanged -= OnLoginSessionPropertyChanged;
						BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_VIVOXVOICECONTROLLER_LOGGED_OUT);
						break;
					}
				default:
					break;
			}
		}

		private void OnParticipantAdded(object sender, KeyEventArg<string> keyEventArg)
		{
			ValidateArgs(new object[] { sender, keyEventArg });

			// INFO: sender is the dictionary that changed and trigger the event.  Need to cast it back to access it.
			var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
			// Look up the participant via the key.
			var participant = source[keyEventArg.Key];
			var username = participant.Account.Name;
			var channel = participant.ParentChannelSession.Key;
			var channelSession = participant.ParentChannelSession;

			// Trigger callback
			OnParticipantAddedEvent?.Invoke(username, channel, participant);
			
			BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_VIVOXVOICECONTROLLER_NEW_PARTICIPANT, username, channel.Name, channelSession.IsTransmitting);
		}

		private void OnParticipantRemoved(object sender, KeyEventArg<string> keyEventArg)
		{
			ValidateArgs(new object[] { sender, keyEventArg });

			// INFO: sender is the dictionary that changed and trigger the event.  Need to cast it back to access it.
			var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
			// Look up the participant via the key.
			var participant = source[keyEventArg.Key];
			var username = participant.Account.Name;
			var channel = participant.ParentChannelSession.Key;
			var channelSession = participant.ParentChannelSession;

			if (participant.IsSelf)
			{
				VivoxLog($"Unsubscribing from: {channelSession.Key.Name}");
				// Now that we are disconnected, unsubscribe.
				channelSession.PropertyChanged -= OnChannelPropertyChanged;
				channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
				channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
				channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
				channelSession.MessageLog.AfterItemAdded -= OnMessageLogRecieved;

				// Remove session.
				var user = _client.GetLoginSession(_accountId);
				user.DeleteChannelSession(channelSession.Channel);
			}

			// Trigger callback
			OnParticipantRemovedEvent?.Invoke(username, channel, participant);
		}

		private static void ValidateArgs(object[] objs)
		{
			foreach (var obj in objs)
			{
				if (obj == null)
					throw new ArgumentNullException(obj.GetType().ToString(), "Specify a non-null/non-empty argument.");
			}
		}

		private void OnParticipantValueUpdated(object sender, ValueEventArg<string, IParticipant> valueEventArg)
		{
			ValidateArgs(new object[] { sender, valueEventArg });

			var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
			// Look up the participant via the key.
			var participant = source[valueEventArg.Key];

			string username = valueEventArg.Value.Account.Name;
			ChannelId channel = valueEventArg.Value.ParentChannelSession.Key;
			string property = valueEventArg.PropertyName;

			switch (property)
			{
				case "SpeechDetected":
					{
						VivoxLog($"OnSpeechDetectedEvent: {username} in {channel}.");
						OnSpeechDetectedEvent?.Invoke(username, channel, valueEventArg.Value.SpeechDetected);
						break;
					}
				case "AudioEnergy":
					{
						OnAudioEnergyChangedEvent?.Invoke(username, channel, valueEventArg.Value.AudioEnergy);
						break;
					}
				default:
					break;
			}
		}

		private void OnChannelPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			ValidateArgs(new object[] { sender, propertyChangedEventArgs });

			//if (_client == null)
			//    throw new InvalidClient("Invalid client.");
			var channelSession = (IChannelSession)sender;

			// IF the channel has removed audio, make sure all the VAD indicators aren't showing speaking.
			if (propertyChangedEventArgs.PropertyName == "AudioState" && channelSession.AudioState == ConnectionState.Disconnected)
			{
				VivoxLog($"Audio disconnected from: {channelSession.Key.Name}");

				foreach (var participant in channelSession.Participants)
				{
					OnSpeechDetectedEvent?.Invoke(participant.Account.Name, channelSession.Channel, false);
				}
			}

			// IF the channel has fully disconnected, unsubscribe and remove.
			if ((propertyChangedEventArgs.PropertyName == "AudioState" || propertyChangedEventArgs.PropertyName == "TextState") &&
				channelSession.AudioState == ConnectionState.Disconnected &&
				channelSession.TextState == ConnectionState.Disconnected)
			{
				VivoxLog($"Unsubscribing from: {channelSession.Key.Name}");
				// Now that we are disconnected, unsubscribe.
				channelSession.PropertyChanged -= OnChannelPropertyChanged;
				channelSession.Participants.AfterKeyAdded -= OnParticipantAdded;
				channelSession.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
				channelSession.Participants.AfterValueUpdated -= OnParticipantValueUpdated;
				channelSession.MessageLog.AfterItemAdded -= OnMessageLogRecieved;

				// Remove session.
				var user = _client.GetLoginSession(_accountId);
				user.DeleteChannelSession(channelSession.Channel);

			}
		}

		private void VivoxLog(string msg)
		{
			Debug.Log("<color=green>VivoxVoice: </color>: " + msg);
		}

		private void VivoxLogError(string msg)
		{
			Debug.LogError("<color=green>VivoxVoice: </color>: " + msg);
		}
#endif		
	}
}