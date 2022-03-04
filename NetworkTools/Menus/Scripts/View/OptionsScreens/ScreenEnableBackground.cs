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
	 * ScreenEnableBackground
	 * 
	 * We will enable or not the background
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenEnableBackground : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_ENABLE_BACKGROUND";

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

			GameObject enableBackgroundVR = m_container.Find("Button_EnableBackgroundVR").gameObject;
			enableBackgroundVR.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.enable.background.vr");
			enableBackgroundVR.GetComponent<Button>().onClick.AddListener(EnableBackgroundVR);

            GameObject disableBackgroundAR = m_container.Find("Button_DisableBackgroundVR").gameObject;
			disableBackgroundAR.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.disable.background.ar");
			disableBackgroundAR.GetComponent<Button>().onClick.AddListener(DisableBackgroundAR);

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
		* PlayInVRPressed
		*/
		private void EnableBackgroundVR()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            MultiplayerConfiguration.SaveEnableBackground(true);

			MenuScreenController.Instance.LoadGameScene(this);
		}

		// -------------------------------------------
		/* 
		* JoinGamePressed
		*/
		private void DisableBackgroundAR()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            MultiplayerConfiguration.SaveEnableBackground(false);

			MenuScreenController.Instance.LoadGameScene(this);
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