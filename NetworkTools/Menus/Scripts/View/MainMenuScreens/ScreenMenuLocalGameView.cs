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
	 * ScreenMenuLocalGameView
	 * 
	 * Screen where we create a local game, you start playing as the deaf and you join to an existing game as the blind
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMenuLocalGameView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_LOCAL_GAME";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		protected GameObject m_root;
		protected Transform m_container;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			NetworkEventController.Instance.MenuController_SetLobbyMode(false);
			NetworkEventController.Instance.MenuController_SetNameRoomLobby("");

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

            if (LanguageController.Instance.ExistsText("message.organize.game.conn"))
            {
                m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.organize.game.conn");
            }
            else
            {
#if !ALTERNATIVE_TITLE
				m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");
#else
				m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.mobile.title");
#endif
			}

			GameObject createGame = m_container.Find("Button_CreateGame").gameObject;
			createGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.local.game.start.a.new.game");
			createGame.GetComponent<Button>().onClick.AddListener(CreateGamePressed);

			GameObject joinGame = m_container.Find("Button_JoinGame").gameObject;
			joinGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.local.game.join.local.game");
			joinGame.GetComponent<Button>().onClick.AddListener(JoinGamePressed);

            if (m_container.Find("Button_Back") != null)
            {
                m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);
            }

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
		 * CreateGamePressed
		 */
		public void CreateGamePressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			if (MenuScreenController.Instance.ForceFixedPlayers != -1)
			{
				MenuScreenController.Instance.CreateRoomInServer(MenuScreenController.Instance.ForceFixedPlayers, MultiplayerConfiguration.LoadExtraData());
			}
			else
			{
                if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenMenuNumberPlayersView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuNumberPlayersView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }                
			}
		}

		// -------------------------------------------
		/* 
		 * JoinGamePressed
		 */
		private void JoinGamePressed()
		{
            UIEventController.Instance.DispatchUIEvent(MenuScreenController.EVENT_MENUEVENTCONTROLLER_JOIN_EXISTING_GAME);
            SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(MultiplayerConfiguration.VALUE_FOR_JOINING);
			MenuScreenController.Instance.CreateOrJoinRoomInServer(true);
		}

		// -------------------------------------------
		/* 
		 * Exit button pressed
		 */
		private void BackPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
            }
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
                    BackPressed();
                }
            }

        }
    }
}
