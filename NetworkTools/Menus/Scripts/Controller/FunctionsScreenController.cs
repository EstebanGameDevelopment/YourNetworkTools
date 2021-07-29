using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourCommonTools;
#if ENABLE_YOURVRUI
using YourVRUI;
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
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE  = "EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE";
        public const string EVENT_MENUEVENTCONTROLLER_CREATED_NEW_GAME      = "EVENT_MENUEVENTCONTROLLER_CREATED_NEW_GAME";
        public const string EVENT_MENUEVENTCONTROLLER_JOIN_EXISTING_GAME    = "EVENT_MENUEVENTCONTROLLER_JOIN_EXISTING_GAME";

        // ----------------------------------------------
        // PUBLIC CONSTANTS
        // ----------------------------------------------	
        public const string BLOCKCHAIN_TAG_BEGIN = "<blockchain>";
        public const string BLOCKCHAIN_TAG_END = "</blockchain>";

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
		private bool m_isFriendsRoom = false;
		private int m_numberOfPlayers = -1;
		private string m_friends;
		private List<string> m_friendsIDs;
		private string m_extraData = "";

        private string m_extraDataBlockchain = "";
        private decimal m_priceBlockchainService = 0;
        private string m_currencySelected = "";
        private string m_publicKeyAddressProvider = "";

        private bool m_checkDefaultMirror = true;

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
#if ENABLE_WORLDSENSE || ENABLE_OCULUS || ENABLE_HTCVIVE || ENABLE_PICONEO
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

            if (Application.isEditor)
            {
                Application.runInBackground = true;
            }

            if (DebugMode)
			{
				Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
			}

#if !ENABLE_OCULUS && !ENABLE_WORLDSENSE && !ENABLE_HTCVIVE && !ENABLE_PICONEO
            Screen.orientation = ScreenOrientation.Portrait;
#endif

			LanguageController.Instance.Initialize();
			SoundsController.Instance.Initialize();

            if (ServerIPAdress.Length > 0) MultiplayerConfiguration.SaveIPAddressServer(ServerIPAdress);
            if (ServerPortNumber != -1) MultiplayerConfiguration.SavePortServer(ServerPortNumber);

            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

#if ENABLE_WORLDSENSE || ENABLE_OCULUS || ENABLE_HTCVIVE || ENABLE_PICONEO
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
		 * Manager of global events
		 */
        protected override void OnUIEvent(string _nameEvent, params object[] _list)
		{
            if (!PreProcessScreenEvents(_nameEvent, _list)) return;

#if ENABLE_YOURVRUI
            ProcessConnectionEvents(_nameEvent, _list);

            ProcessVRUIScreens(_nameEvent, _list);
#else
            base.OnUIEvent(_nameEvent, _list);

            ProcessConnectionEvents(_nameEvent, _list);
#endif

            if (_nameEvent == EVENT_APP_LOST_FOCUS)
            {
#if ENABLE_WORLDSENSE || ENABLE_PICONEO
                if ((bool)_list[0])
                {
                    Application.Quit();
                }
#endif
            }

            if (LogoApp != null)
            {
                if (_nameEvent == ScreenBaseView.EVENT_SCREENBASE_OPENED)
                {
                    UIEventController.Instance.DelayUIEvent(ScreenController.EVENT_SCREENCONTROLLER_REPLACE_LOGO, 0.001F, LogoApp);
                }
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