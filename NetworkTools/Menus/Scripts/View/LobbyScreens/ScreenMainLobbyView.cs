﻿using System;
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
	 * ScreenMainLobbyView
	 * 
	 * Screen where we can create a room or join an existing one
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMainLobbyView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_MAIN_LOBBY";

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

			GameObject createGame = m_container.Find("Button_CreateRoom").gameObject;
			createGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.create.room");
			createGame.GetComponent<Button>().onClick.AddListener(CreateRoom);

			GameObject joinGame = m_container.Find("Button_ListRooms").gameObject;
			joinGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.join.room");
			joinGame.GetComponent<Button>().onClick.AddListener(ListOfRooms);

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);

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
		 * PlayWithFriends
		 */
		private void CreateRoom()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenCreateRoomView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		 * GoToLobby
		 */
		private void ListOfRooms()
		{
            SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
#if ENABLE_PHOTON
            PhotonController.Instance.GetListRooms();
#else
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenListRoomsView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#endif
        }

        // -------------------------------------------
        /* 
		 * Exit button pressed
		 */
        private void BackPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            NetworkEventController.Instance.Destroy();
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenRemoteModeView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
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
                if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS)
                {
#if ENABLE_PHOTON
                    UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenListRoomsView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#endif
                }
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
                {
                    BackPressed();
                }
            }

        }
    }
}
