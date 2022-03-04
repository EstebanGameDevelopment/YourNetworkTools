using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{

    /******************************************
	 * 
	 * ScreenRemoteOrLocalMode
	 * 
	 * Create the session as director or normal user
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenRemoteOrLocalMode : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_REMOTE_LOCAL";

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public Sprite[] Instructions;

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

#if !ALTERNATIVE_TITLE
            m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");
#else
			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.mobile.title");
#endif

            GameObject playLocalGame = m_container.Find("Button_Local").gameObject;
#if UNITY_WEBGL
            playLocalGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.choose.single.player");
#else
            playLocalGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.choose.local.game");
#endif
            playLocalGame.GetComponent<Button>().onClick.AddListener(PlayLocalGame);


            GameObject playRemote = m_container.Find("Button_Remote").gameObject;
#if UNITY_WEBGL
            playRemote.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.choose.multi.player");
#else
            playRemote.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.choose.remote.game");
#endif
            playRemote.GetComponent<Button>().onClick.AddListener(PlayRemoteGame);

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
		* PlayLocalGame
		*/
        private void PlayLocalGame()
        {
#if ENABLE_PHOTON
            if (GameObject.FindObjectOfType<PhotonController>() != null)
            {
                PhotonController.Instance.Destroy();
            }
#elif ENABLE_NAKAMA
            if (GameObject.FindObjectOfType<NakamaController>() != null)
            {
                NakamaController.Instance.Destroy();
            }
#endif

            NetworkEventController.Instance.MenuController_SetLocalGame(true);
            NetworkEventController.Instance.MenuController_SetLobbyMode(false);
            SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
#if UNITY_WEBGL
            MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_DISABLED);
            UIEventController.Instance.DispatchUIEvent(MenuScreenController.EVENT_MENUEVENTCONTROLLER_CREATED_NEW_GAME, 1);
            CardboardLoaderVR.Instance.SaveEnableCardboard(false);
            MenuScreenController.Instance.LoadCustomGameScreenOrCreateGame(false, 1, "", null);            
#else
            if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenMenuLocalGameView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuLocalGameView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, m_nameOfScreen);
            }
#endif
        }

        // -------------------------------------------
        /* 
		* PlayRemoteGame
		*/
        private void PlayRemoteGame()
        {
            NetworkEventController.Instance.MenuController_SetLocalGame(false);
            SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            NetworkEventController.Instance.MenuController_SetLobbyMode(true);
            if (NetworkEventController.Instance.IsConnected)
            {
                if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }
            }
            else
            {
                if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_INFORMATION_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.lobby.connecting.wait"), null, "");
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.lobby.connecting.wait"), null, "");
                }
                // NO CONNECT TCP, GO TO LOBBY
#if ENABLE_BALANCE_LOADER
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#else
                NetworkEventController.Instance.MenuController_InitialitzationSocket(-1, 0);
#endif
            }
        }

        // -------------------------------------------
        /* 
		* OnMenuBasicEvent
		*/
        protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

            if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
#if DISABLE_CREATE_ROOM
                if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenListRoomsView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenListRoomsView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }
#else
                if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }
#endif
            }
        }
    }
}