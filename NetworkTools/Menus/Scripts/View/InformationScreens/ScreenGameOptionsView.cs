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
	 * ScreenGameOptionsView
	 * 
	 * We will use this screen to set the game options
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenGameOptionsView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_GAME_OPTIONS";

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

			GameObject playInVRGame = m_container.Find("Button_EnableVR").gameObject;
			playInVRGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.as.vr.game");
			playInVRGame.GetComponent<Button>().onClick.AddListener(PlayInVRPressed);

			GameObject playWithGyroscopeGame = m_container.Find("Button_EnableGyroscope").gameObject;
			playWithGyroscopeGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.with.gyroscope");
			playWithGyroscopeGame.GetComponent<Button>().onClick.AddListener(PlayWithGyroscopePressed);

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
			GameObject.Destroy(this.gameObject);
			return false;
		}

		// -------------------------------------------
		/* 
		 * PlayInVRPressed
		 */
		private void PlayInVRPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			CardboardLoaderVR.SaveEnableCardboard(true);
			MenuScreenController.Instance.CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
			NetworkEventController.Instance.MenuController_LoadGameScene(MenuScreenController.Instance.TargetGameScene);
		}

		// -------------------------------------------
		/* 
		 * JoinGamePressed
		 */
		private void PlayWithGyroscopePressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			CardboardLoaderVR.SaveEnableCardboard(false);
			MenuScreenController.Instance.CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
			NetworkEventController.Instance.MenuController_LoadGameScene(MenuScreenController.Instance.TargetGameScene);
		}

		// -------------------------------------------
		/* 
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);
		}
	}
}
