using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;
#if ENABLE_USER_SERVER
using UserManagement;
#endif

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenAppSettingsView
	 * 
	 * Main Menu Screen with the option to play a local or a remote game
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenAppSettingsView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SETTINGS";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENAPPSETTINGS_REFRESH = "EVENT_SCREENAPPSETTINGS_REFRESH";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject[] Avatars;
		public GameObject[] Levels;
		public GameObject[] LocalOrRemote;
		public GameObject[] Profiles;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private Text m_textLocalOrRemote;
		private Text m_textProfile;
		private Text m_textAvatar;
		private Text m_textNumberPlayers;
		private Text m_textRoomName;
		private Text m_textEmailAddress;
		private Text m_textLevel;

		private GameObject m_roomNameINGame;
		private GameObject m_avatarInGame;
		private GameObject m_loginCloud;
		private GameObject m_levelInGame;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content/ScrollPage/Page");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");

			// LOCAL OR REMOTE
			GameObject remotePartyGame = m_container.Find("Button_LocalOrRemote").gameObject;
			m_textLocalOrRemote = remotePartyGame.transform.Find("Text").GetComponent<Text>();
            remotePartyGame.GetComponent<Button>().onClick.AddListener(OnLocalOrRemotePartyGame);

			// PROFILE
			GameObject profileInGame = m_container.Find("Button_Profile").gameObject;
			profileInGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.settings.play.as.profile");
			m_textProfile = profileInGame.transform.Find("Value").GetComponent<Text>();
			profileInGame.GetComponent<Button>().onClick.AddListener(OnSelectProfileGame);

			// AVATAR
			m_avatarInGame = m_container.Find("Button_Avatar").gameObject;
			m_avatarInGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.settings.your.avatar");
			m_textAvatar = m_avatarInGame.transform.Find("Value").GetComponent<Text>();
			m_avatarInGame.GetComponent<Button>().onClick.AddListener(OnSelectAvatarGame);

			// NUMBER
			GameObject numberOfPlayersGame = m_container.Find("Button_NumberPlayers").gameObject;
			numberOfPlayersGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.settings.number.of.players");
			m_textNumberPlayers = numberOfPlayersGame.transform.Find("Value").GetComponent<Text>();
			numberOfPlayersGame.GetComponent<Button>().onClick.AddListener(OnPlayerNumberGame);

			// ROOM NAME
			m_roomNameINGame = m_container.Find("Button_RoomName").gameObject;
			m_roomNameINGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.settings.room.name");
			m_textRoomName = m_roomNameINGame.transform.Find("Value").GetComponent<Text>();
			m_roomNameINGame.GetComponent<Button>().onClick.AddListener(OnSetRoomName);

			// LEVEL
			m_levelInGame = m_container.Find("Button_Level").gameObject;
			m_levelInGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.settings.your.level");
			m_textLevel = m_levelInGame.transform.Find("Value").GetComponent<Text>();
			m_levelInGame.GetComponent<Button>().onClick.AddListener(OnSelectLevelGame);

			// CLOUD
			if (m_root.transform.Find("Content/Button_Cloud") != null)
            {
				m_loginCloud = m_root.transform.Find("Content/Button_Cloud").gameObject;
				m_loginCloud.GetComponent<Button>().onClick.AddListener(OnCloudLogin);
			}
#if !ENABLE_USER_SERVER
			m_loginCloud.SetActive(false);
#endif

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);

			RefreshProperties();
		}

        // -------------------------------------------
        /* 
		 * GetGameObject
		 */
        public GameObject GetGameObject()
		{
			return this.gameObject;
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;
			UIEventController.Instance.UIEvent -= OnMenuEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			return false;
		}


		// -------------------------------------------
		/* 
		 * RefreshProperties
		 */
		private void RefreshProperties()
        {
			// LOCAL OR REMOTE
			LocalOrRemote[0].SetActive(MenuScreenController.Instance.AppIsLocal);
			LocalOrRemote[1].SetActive(!MenuScreenController.Instance.AppIsLocal);
			m_textLocalOrRemote.text = (MenuScreenController.Instance.AppIsLocal?LanguageController.Instance.GetText("screen.main.menu.local.party"):LanguageController.Instance.GetText("screen.main.menu.remote.party"));

			// PROFILE
			for (int i = 0; i < Profiles.Length; i++) Profiles[i].SetActive(false);
			Profiles[(int)MenuScreenController.Instance.ProfileSelected].SetActive(true);
			switch (MenuScreenController.Instance.ProfileSelected)
			{
				case FunctionsScreenController.PROFILE_PLAYER.PLAYER:
					m_textProfile.text = LanguageController.Instance.GetText("word.customer");
					m_avatarInGame.SetActive(true);
					break;

				case FunctionsScreenController.PROFILE_PLAYER.DIRECTOR:
					m_textProfile.text = LanguageController.Instance.GetText("word.director");
					m_avatarInGame.SetActive(false);
					break;

				case FunctionsScreenController.PROFILE_PLAYER.SPECTATOR:
					m_textProfile.text = LanguageController.Instance.GetText("word.spectator");
					m_avatarInGame.SetActive(false);
					break;
			}

			// AVATAR
			for (int i = 0; i < Avatars.Length; i++) Avatars[i].SetActive(false);
			Avatars[MenuScreenController.Instance.AppIndexCharacterSelected].SetActive(true);
			m_textAvatar.text = LanguageController.Instance.GetText("player.name.Player" + MenuScreenController.Instance.AppIndexCharacterSelected + "m");

			// LEVEL
			for (int i = 0; i < Levels.Length; i++) Levels[i].SetActive(false);
			Levels[MenuScreenController.Instance.AppIndexLevelSelected].SetActive(true);
			m_textLevel.text = LanguageController.Instance.GetText("level.name.level" + MenuScreenController.Instance.AppIndexLevelSelected);

			// NUMBER
			m_textNumberPlayers.text = MenuScreenController.Instance.AppTotalNumberOfPlayers.ToString();

			// ROOM NAME
			m_roomNameINGame.SetActive(!MenuScreenController.Instance.AppIsLocal);
			if (m_roomNameINGame.activeSelf)
            {
				m_textRoomName.text = MenuScreenController.Instance.AppRoomName;
			}
		}

		// -------------------------------------------
		/* 
		 * OnLocalOrRemotePartyGame
		 */
		private void OnLocalOrRemotePartyGame()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			// UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenAmicGameOrganizationView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
			UIEventController.Instance.DispatchUIEvent(ScreenGameOrganizationView.EVENT_SCREENMAIN_LOCAL_OR_REMOTE_PARTY, !MenuScreenController.Instance.AppIsLocal);
		}

		// -------------------------------------------
		/* 
        * OnSelectAvatarGame
        */
		private void OnSelectAvatarGame()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenCharacterSelectionView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
        * OnSelectLevelGame
        */
		private void OnSelectLevelGame()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLevelSelectionView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
        * OnSelectProfileGame
        */
		private void OnSelectProfileGame()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenDirectorModeView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
        * OnPlayerNumberGame
        */
		private void OnPlayerNumberGame()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuNumberPlayersView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
        * OnSetRoomName
        */
		private void OnSetRoomName()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenCreateRoomView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
        * OnCloudLogin
        */
		private void OnCloudLogin()
		{
#if ENABLE_USER_SERVER
			if (CommsHTTPConstants.Instance.ThereIsConnection)
            {
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenAppMainUserView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
			}			
			else
            {
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.there.is.no.connection.with.database"), null, "");
			}
#endif
		}

		// -------------------------------------------
		/* 
        * InstructionsGamePressed
        */
		private void InstructionsGame()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			List<PageInformation> pages = new List<PageInformation>();
			pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.instructions.title"), LanguageController.Instance.GetText("screen.instructions.page.1"), MenuScreenController.Instance.Instructions[0], ""));
			pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.instructions.title"), LanguageController.Instance.GetText("screen.instructions.page.2"), MenuScreenController.Instance.Instructions[1], ""));
			pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.instructions.title"), LanguageController.Instance.GetText("screen.instructions.page.3"), MenuScreenController.Instance.Instructions[2], ""));
			pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.instructions.title"), LanguageController.Instance.GetText("screen.instructions.page.4"), MenuScreenController.Instance.Instructions[3], ""));
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, ScreenController.TOTAL_LAYERS_SCREENS - 1, null, ScreenInformationView.SCREEN_INFORMATION_IMAGE, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, pages);
		}

		// -------------------------------------------
		/* 
		 * Exit button pressed
		 */
		protected override void GoBackPressed()
		{
			if (MenuScreenController.Instance.AppRoomName.Length < 5)
            {
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.you.should.roomname.length"), null, "");
			}
			else
            {
				base.GoBackPressed();
			}			
		}

		// -------------------------------------------
		/* 
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
            base.OnMenuEvent(_nameEvent, _list);

			if (_nameEvent == EVENT_SCREENAPPSETTINGS_REFRESH)
            {
				RefreshProperties();
				MenuScreenController.Instance.SaveDataInCloud();
			}
        }
    }
}