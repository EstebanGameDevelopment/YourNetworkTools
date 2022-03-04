using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenEnableARCore
	 * 
	 * Enable ARCore to use 6DOF for positioning
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenEnableARCore : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_ENABLE_ARCORE";

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREENARCORE_ENABLED_ARCORE = "EVENT_SCREENARCORE_ENABLED_ARCORE";

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

			GameObject playWithARCore = m_container.Find("Button_WithARCore").gameObject;
			playWithARCore.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.with.arcore");
			playWithARCore.GetComponent<Button>().onClick.AddListener(PlayWithARCore);

			GameObject playWithoutARCore = m_container.Find("Button_WithoutARCore").gameObject;
			playWithoutARCore.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.without.arcore");
			playWithoutARCore.GetComponent<Button>().onClick.AddListener(PlayWithoutARCore);

			MultiplayerConfiguration.SaveEnableBackground(true);

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
		private void PlayWithARCore()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MultiplayerConfiguration.SaveGoogleARCore(MultiplayerConfiguration.GOOGLE_ARCORE_ENABLED);
            if (MenuScreenController.Instance.EnableAppOrganization)
            {
                UIEventController.Instance.DispatchUIEvent(EVENT_SCREENARCORE_ENABLED_ARCORE, true);
                GoBackPressed();
            }
            else
            {
				MenuScreenController.Instance.LoadGameScene(this);
			}
        }

		// -------------------------------------------
		/* 
		* JoinGamePressed
		*/
		private void PlayWithoutARCore()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MultiplayerConfiguration.SaveGoogleARCore(MultiplayerConfiguration.GOOGLE_ARCORE_DISABLED);
			if (MenuScreenController.Instance.EnableAppOrganization)
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENARCORE_ENABLED_ARCORE, false);
				GoBackPressed();
			}
			else
			{
				MenuScreenController.Instance.LoadGameScene(this);
			}
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