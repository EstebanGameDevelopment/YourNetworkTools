using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YourCommonTools;
using YourNetworkingTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenAmicGameOrganizationView
	 * 
	 * Main Menu Screen with the option to play a local or a remote game
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenGameOrganizationView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_GAME_ORGANIZATION";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENMAIN_LOCAL_OR_REMOTE_PARTY = "EVENT_SCREENMAIN_LOCAL_OR_REMOTE_PARTY";
		public const string SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP = "SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");

			/*
			switch (MenusScreenAmicController.InstanceApp.SelectedCategory)
            {
				case MenusScreenAmicController.CATEGORIES_APP.EDUCATION:
					m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.game.organization.party.education");
					break;

				case MenusScreenAmicController.CATEGORIES_APP.WELLNESS:
					m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.game.organization.party.wellness");
					break;

				case MenusScreenAmicController.CATEGORIES_APP.SOCIAL:
					m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.game.organization.party.social");
					break;
            }
			*/

            if (m_container.Find("Button_LocalParty") != null)
            {
                GameObject localPartyGame = m_container.Find("Button_LocalParty").gameObject;
#if UNITY_STANDALONE
                localPartyGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.singleplayer.party");
#else
				localPartyGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.local.party");
#endif
				localPartyGame.GetComponent<Button>().onClick.AddListener(OnLocalPartyGame);
            }


            if (m_container.Find("Button_RemoteParty") != null)
            {
                GameObject remotePartyGame = m_container.Find("Button_RemoteParty").gameObject;
                remotePartyGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.remote.party");
                remotePartyGame.GetComponent<Button>().onClick.AddListener(OnRemotePartyGame);
            }

			SoundsController.Instance.PlayLoopSound(SoundsConfiguration.SOUND_MAIN_MENU);

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);			
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
		 * OnLocalPartyGame
		 */
		private void OnLocalPartyGame()
		{
			if (MenuScreenController.Instance.EnableAppOrganization)
            {
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENMAIN_LOCAL_OR_REMOTE_PARTY, true);
				GoBackPressed();
            }
			else
            {
				NetworkEventController.Instance.MenuController_SetLocalGame(true);
				SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);

#if UNITY_STANDALONE
			CardboardLoaderVR.Instance.SaveEnableCardboard(false);
			MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_DISABLED);
			MultiplayerConfiguration.SaveGoogleARCore(MultiplayerConfiguration.GOOGLE_ARCORE_DISABLED);
			MenuScreenController.Instance.ScreenGameOptions = ScreenCharacterSelectionView.SCREEN_NAME;
			MenuScreenController.Instance.CreateRoomInServer(1, MultiplayerConfiguration.LoadExtraData());
#else
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuLocalGameView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#endif
			}
		}

		// -------------------------------------------
		/* 
		 * JoinGamePressed
		 */
		private void OnRemotePartyGame()
		{
			if (MenuScreenController.Instance.EnableAppOrganization)
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENMAIN_LOCAL_OR_REMOTE_PARTY, false);
				GoBackPressed();
			}
			else
			{
#if UNITY_STANDALONE
			MenuScreenController.Instance.ScreenGameOptions = ScreenDirectorModeView.SCREEN_NAME;
#endif

				NetworkEventController.Instance.MenuController_SetLocalGame(false);
				SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
				NetworkEventController.Instance.MenuController_SetLobbyMode(true);
				if (NetworkEventController.Instance.IsConnected)
				{
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
				}
				else
				{
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.lobby.connecting.wait"), null, "");
					// NO CONNECT TCP, GO TO LOBBY
#if ENABLE_BALANCE_LOADER
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#else
					NetworkEventController.Instance.MenuController_InitialitzationSocket(-1, 0);
#endif
				}
			}
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
		 * ImageScanARCore
		 */
        private void ImageScanARCore()
        {
            SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            List<PageInformation> pages = new List<PageInformation>();
            pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.arcore.scan.title"), LanguageController.Instance.GetText("screen.scan.image.arcore"), MenuScreenController.Instance.ScanImageARCore, ""));
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenInformationView.SCREEN_INFORMATION_IMAGE, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, pages);
        }        

        // -------------------------------------------
        /* 
		 * Exit button pressed
		 */
        private void ExitPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN,ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.do.you.want.exit"), null, SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP);
		}

		// -------------------------------------------
		/* 
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
            base.OnMenuEvent(_nameEvent, _list);

            if (this.gameObject.activeSelf)
            {
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
                {
                    ExitPressed();
                }
            }
            if (_nameEvent == MenuScreenController.EVENT_CONFIRMATION_POPUP)
			{
				string subEvent = (string)_list[2];
				if (subEvent == SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP)
				{
					if ((bool)_list[1])
					{
						Application.Quit();
					}
				}
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM)
			{
				NetworkEventController.Instance.MenuController_LoadGameScene(MenuScreenController.Instance.TargetGameScene);
			}
            if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
#if DISABLE_CREATE_ROOM
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenListRoomsView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#else
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#endif
            }
        }
    }
}