using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourCommonTools;
#if ENABLE_YOURVRUI
using YourVRUI;
#endif
#if ENABLE_USER_SERVER
using UserManagement;
#endif

namespace YourNetworkingTools
{

    /******************************************
	 * 
	 * FunctionsScreenController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal
	 * 
	 * @author Esteban Gallardo
	 */
    public class FunctionsScreenController : ScreenController
	{
        public enum PROFILE_PLAYER { PLAYER = 0, DIRECTOR = 1, SPECTATOR = 2 }

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE  = "EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE";
        public const string EVENT_MENUEVENTCONTROLLER_CREATED_NEW_GAME      = "EVENT_MENUEVENTCONTROLLER_CREATED_NEW_GAME";
        public const string EVENT_MENUEVENTCONTROLLER_JOIN_EXISTING_GAME    = "EVENT_MENUEVENTCONTROLLER_JOIN_EXISTING_GAME";
        public const string EVENT_MENUEVENTCONTROLLERLOAD_GAME_WITH_SETTINGS = "EVENT_MENUEVENTCONTROLLERLOAD_GAME_WITH_SETTINGS";
        
        // ----------------------------------------------
        // PUBLIC CONSTANTS
        // ----------------------------------------------	
        public const string BLOCKCHAIN_TAG_BEGIN = "<blockchain>";
        public const string BLOCKCHAIN_TAG_END = "</blockchain>";

        private const string PREFS_NAME_ROOM = "APP_NAME_ROOM";
        private const string PREFS_NUMBER_PLAYERS = "APP_NUMBER_PLAYERS";
        private const string PREFS_PROFILE = "APP_PROFILE";
        private const string PREFS_AVATAR = "APP_AVATAR";
        private const string PREFS_LEVEL = "APP_LEVEL";
        private const string PREFS_LOCAL_OR_NETWORK = "APP_LOCAL_OR_NETWORK";
        private const string PREFS_ARCORE_ENABLED = "PREFS_ARCORE_ENABLED";

        public const char TOKEN_SEPARATOR_CONFIG = ',';

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        [Tooltip("Target scene where the real application is")]
		public string TargetGameScene;

		[Tooltip("Application instruction images")]
		public Sprite[] Instructions;

		[Tooltip("Custom selector menu buttons graphic")]
		public Sprite SelectorGraphic;

		[Tooltip("Maximum number of players allowed")]
		public int MaxPlayers = 5;

		[Tooltip("Fix the number of players of a game")]
		public int ForceFixedPlayers = -1;

		[Tooltip("Name of the game options screen")]
		public string ScreenGameOptions = "";

        [Tooltip("The IP address of the server")]
        public string ServerIPAdress = "";

        [Tooltip("The IP address of the server")]
        public int ServerPortNumber = -1;

        [Tooltip("Image used by ARCore to set the anchor")]
        public Sprite ScanImageARCore;

        [Tooltip("Main camera used to display the menus in 2D")]
        public GameObject MainCamera2D;

        [Tooltip("Components to display the menus in VR")]
        public GameObject VRComponents;

        [Tooltip("Allow the option to enable AR or VR gaming")]
        public bool AskToEnableBackgroundARCore = false;

        [Tooltip("Maximum number of rooms allowed in server")]
        public int MaxAllowedRooms = 15;

        [Tooltip("Ask for user permission to download the asset bundle")]
        public bool RequestPermissionAssetBundleDownload = false;

        [Tooltip("It's going to use the settings organization")]
        public bool EnableAppOrganization = false;

        [HideInInspector]
        public object ParamsScreenGameOptions = null;

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public Sprite IconApp;
		public Sprite LogoApp;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        protected bool m_isFriendsRoom = false;
        protected int m_numberOfPlayers = -1;
        protected string m_friends;
        protected List<string> m_friendsIDs;
        protected string m_extraData = "";

        protected string m_extraDataBlockchain = "";
        protected decimal m_priceBlockchainService = 0;
        protected string m_currencySelected = "";
        protected string m_publicKeyAddressProvider = "";

        protected bool m_checkDefaultMirror = true;

        protected PROFILE_PLAYER m_profileSelected;
        protected int m_appTotalNumberOfPlayers = 1;
        protected int m_appIndexCharacterSelected = 0;
        protected int m_appIndexLevelSelected = 0;
        protected string m_appRoomName = "";
        protected bool m_appIsLocal = true;
        protected bool m_appEnableARCore = false;

        private IBasicView m_screenRequesterToLoadGame = null;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public string ExtraData
		{
			get { return m_extraData; }
			set {
                m_extraData = value;
                MultiplayerConfiguration.SaveExtraData(m_extraData);
                GetBlockchainFromExtraData();
            }
		}
        public int NumberOfPlayers
        {
            get { return m_numberOfPlayers; }
            set { m_numberOfPlayers = value; }
        }
        public decimal PriceBlockchain
        {
            get { return m_priceBlockchainService; }
            set { m_priceBlockchainService = value; }
        }
        public string CurrencySelected
        {
            get { return m_currencySelected; }
        }
        public string PublicKeyAddressProvider
        {
            get { return m_publicKeyAddressProvider; }
        }
        public string ExtraDataBlockchain
        {
            get { return m_extraDataBlockchain; }
        }
        public bool CheckDefaultMirror
        {
            get { return m_checkDefaultMirror; }
            set { m_checkDefaultMirror = value; }
        }
        public PROFILE_PLAYER ProfileSelected
        {
            get { return m_profileSelected; }
        }
        public int AppTotalNumberOfPlayers
        {
            get { return m_appTotalNumberOfPlayers; }
        }
        public int AppIndexCharacterSelected
        {
            get { return m_appIndexCharacterSelected; }
        }
        public int AppIndexLevelSelected
        {
            get { return m_appIndexLevelSelected; }
        }
        public string AppRoomName
        {
            get { return m_appRoomName; }
        }
        public bool AppIsLocal
        {
            get { return m_appIsLocal; }
        }
        public bool AppEnableARCore
        {
            get { return m_appEnableARCore; }
        }

#if ENABLE_YOURVRUI
        public int ScreensVREnabled
        {
            get
            {
                BaseVRScreenView[] totalScreenVR = YourVRUIScreenController.Instance.gameObject.GetComponentsInChildren<BaseVRScreenView>(true);
                return totalScreenVR.Length;
            }
        }
#endif

        // -------------------------------------------
        /* 
		 * Awake
		 */
        public override void Awake()
        {
#if ENABLE_WORLDSENSE || ENABLE_OCULUS || ENABLE_HTCVIVE
            if (MainCamera2D != null) MainCamera2D.SetActive(false);
            if (VRComponents != null) VRComponents.SetActive(true);
#else
            if (MainCamera2D != null) MainCamera2D.SetActive(true);
            if (VRComponents != null) VRComponents.SetActive(false);
#endif
        }

        // -------------------------------------------
        /* 
		 * Initialitzation listener
		 */
        public override void Start()
		{
			base.Start();

            InitConfigurationSession();

            if (Application.isEditor)
            {
                Application.runInBackground = true;
            }

            if (DebugMode)
			{
				Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
			}

#if !ENABLE_OCULUS && !ENABLE_WORLDSENSE && !ENABLE_HTCVIVE
            Screen.orientation = ScreenOrientation.Portrait;
#endif

			LanguageController.Instance.Initialize();
			SoundsController.Instance.Initialize();
#if ENABLE_USER_SERVER
            UsersController.Instance.Initialize();
#endif

            if (ServerIPAdress.Length > 0) MultiplayerConfiguration.SaveIPAddressServer(ServerIPAdress);
            if (ServerPortNumber != -1) MultiplayerConfiguration.SavePortServer(ServerPortNumber);

            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);

#if ENABLE_WORLDSENSE || ENABLE_OCULUS || ENABLE_HTCVIVE
            KeysEventInputController.Instance.EnableActionOnMouseDown = false;
#endif
            StartSplashScreen();
        }

        // -------------------------------------------
        /* 
		 * StartSplashScreen
		 */
        public virtual void StartSplashScreen()
        {
#if UNITY_EDITOR
            // UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenSplashView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
#else
		UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenSplashView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);        
#endif
        }

        // -------------------------------------------
        /* 
		 * InitConfigurationSession
		 */
        protected void InitConfigurationSession()
        {
            m_profileSelected = (PROFILE_PLAYER)PlayerPrefs.GetInt(PREFS_PROFILE, 0);
            m_appTotalNumberOfPlayers = PlayerPrefs.GetInt(PREFS_NUMBER_PLAYERS, 5);
            m_appIndexCharacterSelected = PlayerPrefs.GetInt(PREFS_AVATAR, 0);
            m_appIndexLevelSelected = PlayerPrefs.GetInt(PREFS_LEVEL, 0);
            m_appRoomName = PlayerPrefs.GetString(PREFS_NAME_ROOM, "MyVRRoom");
            m_appIsLocal = PlayerPrefs.GetInt(PREFS_LOCAL_OR_NETWORK, 1) == 0;
            m_appEnableARCore = PlayerPrefs.GetInt(PREFS_ARCORE_ENABLED, 1) == 0;
        }

        // -------------------------------------------
        /* 
		 * Destroy all references
		 */
        void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		* Destroy all references
		*/
		public override void Destroy()
		{
			base.Destroy();

			UIEventController.Instance.UIEvent -= OnUIEvent;
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;

            LanguageController.Instance?.Destroy();
            SoundsController.Instance?.Destroy();

#if ENABLE_USER_SERVER
            UsersController.Instance?.Destroy();
            CommsHTTPConstants.Instance?.Destroy();
#endif
        }

        // -------------------------------------------
        /* 
		 * GetBlockchainFromExtraData
		 */
        public bool GetBlockchainFromExtraData()
        {
            if (m_extraDataBlockchain.Length == 0)
            {
                m_extraDataBlockchain = MultiplayerConfiguration.LoadExtraData();
            }

            if (m_extraDataBlockchain.IndexOf(BLOCKCHAIN_TAG_BEGIN) == -1)
            {
                m_extraDataBlockchain = "";
            }
            else
            {                
                int startingBlockchainAddress = m_extraDataBlockchain.IndexOf(BLOCKCHAIN_TAG_BEGIN) + BLOCKCHAIN_TAG_BEGIN.Length;
                int sizeBlockchainAddress = m_extraDataBlockchain.IndexOf(BLOCKCHAIN_TAG_END) - startingBlockchainAddress;
                string dataBlockchain = m_extraDataBlockchain.Substring(startingBlockchainAddress, sizeBlockchainAddress);
                string[] paramsToPay = dataBlockchain.Split(':');

                if (paramsToPay.Length == 3)
                {
                    m_priceBlockchainService = decimal.Parse(paramsToPay[0]);
                    m_currencySelected = (string)paramsToPay[1];
                    m_publicKeyAddressProvider = (string)paramsToPay[2];

                    return true;
                }
            }
            return false;
        }

        // -------------------------------------------
        /* 
		 * RemoveBlockchainFromExtraData
		 */
        public string RemoveBlockchainFromExtraData(string _extraData)
        {
            if (_extraData.IndexOf(BLOCKCHAIN_TAG_BEGIN) == -1)
            {
                return _extraData;
            }
            else
            {
                int startingBlockchainAddress = _extraData.IndexOf(BLOCKCHAIN_TAG_BEGIN);
                int endBlockchainAddress = _extraData.IndexOf(BLOCKCHAIN_TAG_END) + BLOCKCHAIN_TAG_END.Length;

                string part1 = _extraData.Substring(0, startingBlockchainAddress);
                string part2 = _extraData.Substring(endBlockchainAddress, _extraData.Length - endBlockchainAddress);

                return part1 + part2;
            }
        }

        // -------------------------------------------
        /* 
		 * AddBlockchainToExtraData
		 */
        public void AddBlockchainToExtraData(decimal _price, string _currency, string _providerAddress)
        {
            ExtraData = RemoveBlockchainFromExtraData(ExtraData);
            ExtraData += GetBlockchainExtraData(_price, _currency, _providerAddress);
        }

        // -------------------------------------------
        /* 
		 * GetBlockchainExtraData
		 */
        public string GetBlockchainExtraData(decimal _price, string _currency, string _providerAddress)
        {
            return BLOCKCHAIN_TAG_BEGIN + _price + ":" + _currency + ":" + _providerAddress + BLOCKCHAIN_TAG_END;
        }

        // -------------------------------------------
        /* 
		* OnBasicSystemEvent
		*/
        protected virtual void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
#if ENABLE_USER_SERVER
            if (_nameEvent == UsersController.EVENT_USER_LOGIN_FORMATTED)
            {
                Debug.LogError("EVENT_USER_LOGIN_FORMATTED::(bool)_list[0]=" + (bool)_list[0]);
                if ((bool)_list[0])
                {
                    ParseCloudData();
                }
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * RefreshProperties
		 */
        private void ParseCloudData()
        {
#if ENABLE_USER_SERVER
            if (UsersController.Instance.CurrentUser.Email.Length > 0)
            {
                string[] dataConfig = UsersController.Instance.CurrentUser.Profile.Data.Split(TOKEN_SEPARATOR_CONFIG);

                if (dataConfig.Length >= 6)
                {
                    bool isLocal = bool.Parse(dataConfig[0]);
                    UIEventController.Instance.DispatchUIEvent(ScreenGameOrganizationView.EVENT_SCREENMAIN_LOCAL_OR_REMOTE_PARTY, isLocal, false);

                    PROFILE_PLAYER profileSelected = (PROFILE_PLAYER)int.Parse(dataConfig[1]);
                    UIEventController.Instance.DispatchUIEvent(ScreenDirectorModeView.EVENT_SCREENDIRECTORMODE_SELECTED_PROFILE, profileSelected, false);

                    int amicIndexCharacterSelected = int.Parse(dataConfig[2]);
                    UIEventController.Instance.DispatchUIEvent(ScreenCharacterSelectionView.EVENT_SCREENCHARACTERSELECTION_SELECTED_CHARACTER, amicIndexCharacterSelected, false);

                    int amicIndexLevelSelected = int.Parse(dataConfig[3]);
                    UIEventController.Instance.DispatchUIEvent(ScreenLevelSelectionView.EVENT_SCREENLEVELSELECTION_SELECTED_LEVEL, amicIndexLevelSelected, false);

                    int amicTotalNumberOfPlayers = int.Parse(dataConfig[4]);
                    UIEventController.Instance.DispatchUIEvent(ScreenMenuNumberPlayersView.EVENT_SCREENNUMBERPLAYERS_SET_NUMBER_PLAYERS, amicTotalNumberOfPlayers, false);

                    string amicRoomName = dataConfig[5];
                    UIEventController.Instance.DispatchUIEvent(ScreenCreateRoomView.EVENT_SCREENCREATEROOM_SETUP_NAME, amicRoomName, false);

                    if (dataConfig.Length > 6)
                    {
                        bool isARCoreEnabled = bool.Parse(dataConfig[6]);
                        UIEventController.Instance.DispatchUIEvent(ScreenEnableARCore.EVENT_SCREENARCORE_ENABLED_ARCORE, isARCoreEnabled, false);
                    }
                }
                else
                {
                    SaveDataInCloud();
                }
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * SaveDataInCloud
		 */
        public void SaveDataInCloud()
        {
#if ENABLE_USER_SERVER
            if (UsersController.Instance.CurrentUser.Email.Length > 0)
            {
                string dataConfig = "";
                dataConfig += AppIsLocal.ToString() + TOKEN_SEPARATOR_CONFIG;
                dataConfig += ((int)ProfileSelected).ToString() + TOKEN_SEPARATOR_CONFIG;
                dataConfig += AppIndexCharacterSelected.ToString() + TOKEN_SEPARATOR_CONFIG;
                dataConfig += AppIndexLevelSelected.ToString() + TOKEN_SEPARATOR_CONFIG;
                dataConfig += AppTotalNumberOfPlayers.ToString() + TOKEN_SEPARATOR_CONFIG;
                dataConfig += AppRoomName.ToString() + TOKEN_SEPARATOR_CONFIG;
                dataConfig += AppEnableARCore.ToString();

                UIEventController.Instance.DispatchUIEvent(UsersController.EVENT_USER_UPDATE_PROFILE_DATA_REQUEST, dataConfig);
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * LoadGameScene
		 */
        public void LoadGameScene(IBasicView _screen = null)
        {
            try
            {
                if (_screen != null)
                {
                    if (_screen != null) _screen.Destroy();
                    _screen.Destroy();
                }
                else
                {
                    if (m_screenRequesterToLoadGame != null) m_screenRequesterToLoadGame.Destroy();
                    m_screenRequesterToLoadGame = null;
                }
            }
            catch (Exception err) { }
#if UNITY_STANDALONE
            CardboardLoaderVR.Instance.SaveEnableCardboard(false);
            MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
#else
#if ENABLE_GOOGLE_ARCORE
            CardboardLoaderVR.Instance.SaveEnableCardboard(false);
#else
            if (YourVRUIScreenController.Instance == null)
            {
                CardboardLoaderVR.Instance.SaveEnableCardboard(false);
            }
            else
            {
                CardboardLoaderVR.Instance.SaveEnableCardboard(true);
            }
#endif
            MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
#endif
        }

        // -------------------------------------------
        /* 
		 * SetUpFinalLevel
		 */
        protected virtual void SetUpFinalLevel()
        {
            MultiplayerConfiguration.SaveLevel6DOF(m_appIndexLevelSelected);
        }

        // -------------------------------------------
        /* 
		 * SetUpFinalCharacter
		 */
        protected virtual void SetUpFinalCharacter()
        {
            MultiplayerConfiguration.SaveCharacter6DOF(m_appIndexCharacterSelected);
        }

        // -------------------------------------------
        /* 
		 * OnUISettings
		 */
        protected virtual void OnUISettings(string _nameEvent, params object[] _list)
        {
            if (_nameEvent == ScreenDirectorModeView.EVENT_SCREENDIRECTORMODE_SELECTED_PROFILE)
            {
                m_profileSelected = (PROFILE_PLAYER)_list[0];
                PlayerPrefs.SetInt(PREFS_PROFILE, (int)m_profileSelected);
                bool shouldRefresh = true;
                if (_list.Length > 1) shouldRefresh = (bool)_list[1];
                if (shouldRefresh)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenAppSettingsView.EVENT_SCREENAPPSETTINGS_REFRESH);
                    SaveDataInCloud();
                }
            }
            if (_nameEvent == ScreenEnableARCore.EVENT_SCREENARCORE_ENABLED_ARCORE)
            {
                m_appEnableARCore = (bool)_list[0];
                PlayerPrefs.SetInt(PREFS_ARCORE_ENABLED, (m_appEnableARCore?1:0));
                bool shouldRefresh = true;
                if (_list.Length > 1) shouldRefresh = (bool)_list[1];
                if (shouldRefresh)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenAppSettingsView.EVENT_SCREENAPPSETTINGS_REFRESH);
                    SaveDataInCloud();
                }
            }
            if (_nameEvent == ScreenMenuNumberPlayersView.EVENT_SCREENNUMBERPLAYERS_SET_NUMBER_PLAYERS)
            {
                m_appTotalNumberOfPlayers = (int)_list[0];
                PlayerPrefs.SetInt(PREFS_NUMBER_PLAYERS, m_appTotalNumberOfPlayers);
                bool shouldRefresh = true;
                if (_list.Length > 1) shouldRefresh = (bool)_list[1];
                if (shouldRefresh)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenAppSettingsView.EVENT_SCREENAPPSETTINGS_REFRESH);
                    SaveDataInCloud();
                }
            }
            if (_nameEvent == ScreenCharacterSelectionView.EVENT_SCREENCHARACTERSELECTION_SELECTED_CHARACTER)
            {
                m_appIndexCharacterSelected = (int)_list[0];
                PlayerPrefs.SetInt(PREFS_AVATAR, m_appIndexCharacterSelected);
                bool shouldRefresh = true;
                if (_list.Length > 1) shouldRefresh = (bool)_list[1];
                if (shouldRefresh)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenAppSettingsView.EVENT_SCREENAPPSETTINGS_REFRESH);
                    SaveDataInCloud();
                }
            }
            if (_nameEvent == ScreenLevelSelectionView.EVENT_SCREENLEVELSELECTION_SELECTED_LEVEL)
            {
                m_appIndexLevelSelected = (int)_list[0];
                PlayerPrefs.SetInt(PREFS_LEVEL, m_appIndexLevelSelected);
                bool shouldRefresh = true;
                if (_list.Length > 1) shouldRefresh = (bool)_list[1];
                if (shouldRefresh)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenAppSettingsView.EVENT_SCREENAPPSETTINGS_REFRESH);
                    SaveDataInCloud();
                }
            }
            if (_nameEvent == ScreenCreateRoomView.EVENT_SCREENCREATEROOM_SETUP_NAME)
            {
                m_appRoomName = (string)_list[0];
                PlayerPrefs.SetString(PREFS_NAME_ROOM, m_appRoomName);
                bool shouldRefresh = true;
                if (_list.Length > 1) shouldRefresh = (bool)_list[1];
                if (shouldRefresh)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenAppSettingsView.EVENT_SCREENAPPSETTINGS_REFRESH);
                    SaveDataInCloud();
                }
            }
            if (_nameEvent == ScreenGameOrganizationView.EVENT_SCREENMAIN_LOCAL_OR_REMOTE_PARTY)
            {
                m_appIsLocal = (bool)_list[0];
                PlayerPrefs.SetInt(PREFS_LOCAL_OR_NETWORK, (m_appIsLocal ? 0 : 1));
                bool shouldRefresh = true;
                if (_list.Length > 1) shouldRefresh = (bool)_list[1];
                if (shouldRefresh)
                {
                    UIEventController.Instance.DispatchUIEvent(ScreenAppSettingsView.EVENT_SCREENAPPSETTINGS_REFRESH);
                    SaveDataInCloud();
                }
            }
            if (_nameEvent == EVENT_MENUEVENTCONTROLLERLOAD_GAME_WITH_SETTINGS)
            {
                m_screenRequesterToLoadGame = (IBasicView)_list[0];

                switch (m_profileSelected)
                {
                    case FunctionsScreenController.PROFILE_PLAYER.PLAYER:
                        MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_DISABLED);
                        CardboardLoaderVR.Instance.SaveEnableCardboard(true);
                        break;

                    case FunctionsScreenController.PROFILE_PLAYER.DIRECTOR:
                        MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_ENABLED);
                        CardboardLoaderVR.Instance.SaveEnableCardboard(false);
                        break;

                    case FunctionsScreenController.PROFILE_PLAYER.SPECTATOR:
                        MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_ENABLED);
                        MultiplayerConfiguration.SaveSpectatorMode(MultiplayerConfiguration.SPECTATOR_MODE_ENABLED);
                        CardboardLoaderVR.Instance.SaveEnableCardboard(false);
                        break;
                }

                SetUpFinalLevel();
                SetUpFinalCharacter();

                NumberOfPlayers = m_appTotalNumberOfPlayers;

                if (m_appIsLocal)
                {
                    NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(m_appTotalNumberOfPlayers);
                    NetworkEventController.Instance.MenuController_SetLocalGame(true);
                    NetworkEventController.Instance.MenuController_SetLobbyMode(false);
                    m_screenRequesterToLoadGame.Destroy();
                    LoadGameScene();
                }
                else
                {
                    NetworkEventController.Instance.MenuController_SetLocalGame(false);
                    NetworkEventController.Instance.MenuController_SetLobbyMode(true);

                    if (m_appRoomName.Length > 0)
                    {
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.lobby.connecting.wait"), null, "");
                        NetworkEventController.Instance.MenuController_InitialitzationSocket(-1, 0);
                    }
                }
            }
            if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_WAIT);
                if (m_appRoomName.Length > 0)
                {
                    bool roomFound = false;
                    int indexRoomFound = -1;
                    for (int i = 0; i < NetworkEventController.Instance.RoomsLobby.Count; i++)
                    {
                        ItemMultiTextEntry item = NetworkEventController.Instance.RoomsLobby[i];
                        int roomNumber = int.Parse(item.Items[1]);
                        string nameRoom = item.Items[2];
                        if (nameRoom.Equals(m_appRoomName))
                        {
                            roomFound = true;
                            indexRoomFound = roomNumber;
                        }
                    }

                    NetworkEventController.Instance.MenuController_SetNameRoomLobby(m_appRoomName);

                    if (roomFound)
                    {
#if UNITY_EDITOR
                        Debug.LogError("ROOM NAME[" + m_appRoomName + "] ++YES++ FOUND IN LIST ROOMS LOBBY[" + NetworkEventController.Instance.RoomsLobby.Count + "]::NOW JOINNING...");
#endif
                        // JOIN
                        NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(MultiplayerConfiguration.VALUE_FOR_JOINING);
                        PlayerPrefs.SetString(ScreenCreateRoomView.PLAYERPREFS_YNT_ROOMNAME, m_appRoomName);
                        NetworkEventController.Instance.MenuController_SaveRoomNumberInServer(indexRoomFound);
                        NetworkEventController.Instance.MenuController_SaveRoomNameInServer(m_appRoomName);
                        NetworkEventController.Instance.MenuController_SetNameRoomLobby(m_appRoomName);
                        MenuScreenController.Instance.ExtraData = "";
                        LoadGameScene();
                    }
                    else
                    {
                        // CREATE
#if UNITY_EDITOR
                        Debug.LogError("ROOM NAME[" + m_appRoomName + "] --NOT-- FOUND IN LIST ROOMS LOBBY[" + NetworkEventController.Instance.RoomsLobby.Count + "] CREATING ROOM[" + m_appRoomName + "]");
#endif
                        NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(m_appTotalNumberOfPlayers);
                        LoadGameScene();
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Manager of global events
		 */
        protected override void OnUIEvent(string _nameEvent, params object[] _list)
		{
            if (!PreProcessScreenEvents(_nameEvent, _list)) return;

            OnUISettings(_nameEvent, _list);

#if ENABLE_YOURVRUI
            ProcessConnectionEvents(_nameEvent, _list);

            ProcessVRUIScreens(_nameEvent, _list);
#else
            base.OnUIEvent(_nameEvent, _list);

            ProcessConnectionEvents(_nameEvent, _list);
#endif

            if (_nameEvent == EVENT_APP_LOST_FOCUS)
            {
#if ENABLE_WORLDSENSE
                if ((bool)_list[0])
                {
                    Application.Quit();
                }
#endif
            }

            if (_nameEvent == ScreenBaseView.EVENT_SCREENBASE_OPENED)
            {
                DisplayLogoForScreen();
            }
        }

        // -------------------------------------------
        /* 
		 * DisplayLogoForScreen
		 */
        protected virtual void DisplayLogoForScreen()
        {
            if (LogoApp != null)
            {
                UIEventController.Instance.DelayUIEvent(ScreenController.EVENT_SCREENCONTROLLER_REPLACE_LOGO, 0.001F, LogoApp);
            }
        }

        // -------------------------------------------
        /* 
		 * Process connection events
		 */
        protected void ProcessConnectionEvents(string _nameEvent, params object[] _list)
        {
            if (_nameEvent == ScreenLoadingView.EVENT_SCREENLOADING_LOAD_OR_JOIN_GAME)
            {
                CreateOrJoinRoomInServer((bool)_list[0], false);
            }
            if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM)
            {
                if (NetworkEventController.Instance.MenuController_LoadNumberOfPlayers() != MultiplayerConfiguration.VALUE_FOR_JOINING)
                {
                    NetworkEventController.Instance.MenuController_SaveNumberOfPlayers((int)_list[0]);
                }
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
                NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
            }
            if (_nameEvent == EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE)
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
            }
            if (_nameEvent == CreateNewRoomHTTP.EVENT_CLIENT_HTTP_NEW_ROOM_CREATED)
            {
                // CREATE ROOM IN LOBBY
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
                if (_list.Length == 4)
                {
                    NetworkEventController.Instance.MenuController_SaveRoomNumberInServer((int)_list[0]);
                    NetworkEventController.Instance.MenuController_SaveIPAddressServer((string)_list[1]);
                    NetworkEventController.Instance.MenuController_SavePortServer((int)_list[2]);
                    NetworkEventController.Instance.MenuController_SaveMachineIDServer((int)_list[3]);
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
                    NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
                    CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.room.not.created.right"), null, "");
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Process optional VR menus
		 */
        protected void ProcessVRUIScreens(string _nameEvent, params object[] _list)
        {
#if ENABLE_YOURVRUI
            if (YourVRUIScreenController.Instance == null)
            {
                ProcessScreenEvents(_nameEvent, _list);
            }
            else
            {
                // GENERIC SCREEN
                int layerScreen = 0;
                int indexToCheck = -1;
                float depth = 0;
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN)
                {
                    depth = 0;
                    indexToCheck = 0;
                }
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN)
                {
                    layerScreen = (int)_list[0];
                    depth = layerScreen * 0.1f;
                    indexToCheck = 2;
                }
                if (indexToCheck != -1)
                { 
                    if (_list.Length > indexToCheck + 2)
                    {
                        if ((bool)_list[indexToCheck + 2])
                        {
                            YourVRUIScreenController.Instance.DestroyScreens();
                        }
                        else
                        {
                            YourVRUIScreenController.Instance.EnableScreens = true;
                        }
                    }
                    object pages = null;
                    if (_list.Length > indexToCheck + 3)
                    {
                        pages = _list[indexToCheck + 3];
                    }
                    float scaleScreen = -1;
                    if (_list.Length > indexToCheck + 4)
                    {
                        if (_list[indexToCheck + 4] is float)
                        {
                            scaleScreen = (float)_list[indexToCheck + 4];
                        }
                    }
                    bool isTemporalScreen = true;
                    if (_list.Length > indexToCheck + 5)
                    {
                        if (_list[indexToCheck + 5] is bool)
                        {
                            isTemporalScreen = (bool)_list[indexToCheck + 5];
                        }
                    }
                    if (ScreensVREnabled > TotalStackedScreensAllowed)
                    {
                        List<PageInformation> pagesInformation = new List<PageInformation>();
                        pagesInformation.Add(new PageInformation(LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("total.maximum.screen.reached"), null, "", "", ""));

                        YourVRUIScreenController.Instance.CreateScreenLinkedToCamera(GetScreenPrefabByName(ScreenInformationView.SCREEN_INFORMATION), pagesInformation, 1.5f - depth, -1, false, scaleScreen, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, true, layerScreen);
                    }
                    else
                    {
                        YourVRUIScreenController.Instance.CreateScreenLinkedToCamera(GetScreenPrefabByName((string)_list[indexToCheck]), pages, 1.5f, -1, false, scaleScreen, (UIScreenTypePreviousAction)_list[indexToCheck + 1], isTemporalScreen, layerScreen);
                    }
                    if (depth == 0)
                    {
                        if ((string)_list[0] == ScreenCreateRoomView.SCREEN_NAME)
                        {
                            UIEventController.Instance.DispatchUIEvent(ScreenCreateRoomView.EVENT_SCREENCREATEROOM_CREATE_RANDOM_NAME);
                        }
                    }
                }

                // INFORMATION SCREEN
                layerScreen = 0;
                indexToCheck = -1;
                depth = 0;
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN)
                {
                    depth = 0;
                    indexToCheck = 0;
                }
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_INFORMATION_SCREEN)
                {
                    layerScreen = (int)_list[0];
                    depth = layerScreen * 0.1f;
                    indexToCheck = 2;
                }
                if (indexToCheck != -1)
                {
                    string nameScreen = (string)_list[indexToCheck];
                    UIScreenTypePreviousAction previousAction = (UIScreenTypePreviousAction)_list[indexToCheck + 1];
                    string title = (string)_list[indexToCheck + 2];
                    string description = (string)_list[indexToCheck + 3];
                    Sprite image = (Sprite)_list[indexToCheck + 4];
                    string eventData = (string)_list[indexToCheck + 5];
                    float scaleScreen = -1;
                    if (_list.Length > indexToCheck + 6)
                    {
                        if (_list[indexToCheck + 6] is float)
                        {
                            scaleScreen = (float)_list[indexToCheck + 6];
                        }
                    }
                    List<PageInformation> pages = new List<PageInformation>();
                    pages.Add(new PageInformation(title, description, image, eventData, "", ""));
                    YourVRUIScreenController.Instance.CreateScreenLinkedToCamera(GetScreenPrefabByName((string)_list[indexToCheck]), pages, 1.4f, -1, false, scaleScreen, previousAction, ScreenController.TOTAL_LAYERS_SCREENS - 1);
                }
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_LOAD_NEW_SCENE)
                {
                    if (YourVRUIScreenController.Instance != null)
                    {
                        YourVRUIScreenController.Instance.Destroy();
                    }
                }
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * Create the room in server
		 */
        public void CreateRoomInServer(int _finalNumberOfPlayers, string _extraData, bool _checkScreenGameOptions = false)
		{
			// NUMBER OF PLAYERS
			int finalNumberOfPlayers = _finalNumberOfPlayers;
			if ((finalNumberOfPlayers > 0) && (finalNumberOfPlayers <= MaxPlayers))
			{
				NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(finalNumberOfPlayers);
				if (NetworkEventController.Instance.IsLobbyMode)
				{
					if (NetworkEventController.Instance.NameRoomLobby.Length > 0)
					{
						NetworkEventController.Instance.MenuController_CreateNewLobbyRoom(NetworkEventController.Instance.NameRoomLobby, finalNumberOfPlayers, _extraData);
					}
					else
					{
						UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
						CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.there.is.no.name.for.room"), null, "");
					}
				}
				else
				{
                    if ((GetScreenGameOptions().Length > 0) && !_checkScreenGameOptions)
					{
                        if (AlphaAnimationNameStack != -1)
                        {
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, AlphaAnimationNameStack }, GetScreenGameOptions(), UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true, ParamsScreenGameOptions);
                        }
                        else
                        {
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, GetScreenGameOptions(), UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, ParamsScreenGameOptions);
                        }						
					}
					else
					{
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
						NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
					}
				}
			}
			else
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
				CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.player.number.not.right.number"), null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * Will create the game or it will load a custom game screen
		 */
		public void LoadCustomGameScreenOrCreateGame(bool _isFriendsRoom, int _numberOfPlayers, string _friends, List<string> _friendsIDs, bool _loadNextScreen = true)
		{
			m_isFriendsRoom = _isFriendsRoom;
			m_numberOfPlayers = _numberOfPlayers;
			m_friends = _friends;
			if (_friendsIDs != null)
			{
				m_friendsIDs = new List<string>();
				for (int i = 0; i < _friendsIDs.Count; i++)
				{
					m_friendsIDs.Add(_friendsIDs[i]);
				}
			}

			if (m_isFriendsRoom)
			{
				m_numberOfPlayers = m_friendsIDs.Count;
			}

			NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(m_numberOfPlayers);
            if (_loadNextScreen)
            {
                if (GetScreenGameOptions().Length > 0)
                {
                    if (AlphaAnimationNameStack != -1)
                    {
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, AlphaAnimationNameStack }, GetScreenGameOptions(), UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, ParamsScreenGameOptions);
                    }
                    else
                    {
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, GetScreenGameOptions(), UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, ParamsScreenGameOptions);
                    }
                }
                else
                {
                    CreateOrJoinRoomInServer(false);
                }
            }
        }

		// -------------------------------------------
		/* 
		 * Create for real the room in server
		 */
		public void CreateOrJoinRoomInServer(bool _checkScreenGameOptions, bool _considerAssetBundle = true)
		{
            bool checkLoadGameScene = true;
            if ((UIEventController.Instance.URLAssetBundle.Length > 0) && (_considerAssetBundle))
            {
                checkLoadGameScene = false;
            }

            if (NetworkEventController.Instance.MenuController_LoadNumberOfPlayers() == MultiplayerConfiguration.VALUE_FOR_JOINING)
            {
                if (_checkScreenGameOptions && (GetScreenGameOptions().Length > 0))
                {
                    if (AlphaAnimationNameStack != -1)
                    {
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, AlphaAnimationNameStack }, GetScreenGameOptions(), UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, ParamsScreenGameOptions);
                    }
                    else
                    {
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, GetScreenGameOptions(), UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, ParamsScreenGameOptions);
                    }
                }
                else
                {
                    if (checkLoadGameScene)
                    {
                        if (!YourNetworkTools.GetIsLocalGame())
                        {
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                            JoinARoomInServer();
                        }
                        else
                        {
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                            NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
                        }
                    }
                }
            }
            else
            {
                if (checkLoadGameScene)
                {
                    if (!YourNetworkTools.GetIsLocalGame())
                    {
                        if (m_isFriendsRoom)
                        {
                            NetworkEventController.Instance.MenuController_CreateNewFacebookRoom(m_friends, m_friendsIDs, m_extraData);
                        }
                        else
                        {
                            // Debug.LogError("CREATE NEW ROOM IN SERVER[" + m_extraData + "][" + m_numberOfPlayers + "]+++++++++++");
                            Debug.LogError("CREATE NEW ROOM IN SERVER[" + m_numberOfPlayers + "]+++++++++++");
                            CreateRoomInServer(m_numberOfPlayers, m_extraData, _checkScreenGameOptions);
                        }
                    }
                    else
                    {
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                        NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
                    }
                }
            }
        }

		// -------------------------------------------
		/* 
		* Client has selected a room to join
		*/
		public void JoinARoomInServer()
		{
			if (NetworkEventController.Instance.IsLobbyMode)
			{
#if ENABLE_PHOTON
                NetworkEventController.Instance.MenuController_JoinRoomOfLobby(MultiplayerConfiguration.LoadRoomNameInServer(""), "null", "");
#else
                // JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
				NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
#else
                NetworkEventController.Instance.MenuController_JoinRoomOfLobby(MultiplayerConfiguration.LoadRoomNumberInServer(-1), "null", "");
#endif
#endif
            }
			else
			{
				// JOIN ROOM IN FACEBOOK
#if ENABLE_BALANCE_LOADER
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
				NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
#else
				NetworkEventController.Instance.MenuController_JoinRoomForFriends(MultiplayerConfiguration.LoadRoomNumberInServer(-1), "null", "");
#endif
			}
		}

        // -------------------------------------------
        /* 
		* GetScreenGameOptions
		*/
        protected virtual string GetScreenGameOptions()
        {
            return ScreenGameOptions;
        }
    }
}