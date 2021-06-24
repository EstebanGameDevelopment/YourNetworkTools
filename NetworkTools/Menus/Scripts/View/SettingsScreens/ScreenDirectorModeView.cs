using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{

    /******************************************
	 * 
	 * ScreenDirectorMode
	 * 
	 * Create the session as director or normal user
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenDirectorModeView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_DIRECTOR_MODE";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENDIRECTORMODE_SELECTED_PROFILE = "EVENT_SCREENDIRECTORMODE_SELECTED_PROFILE";

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

			GameObject playAsCustomer = m_container.Find("Button_Customer").gameObject;
			playAsCustomer.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.as.customer");
			playAsCustomer.GetComponent<Button>().onClick.AddListener(PlayAsCustomer);

			GameObject playAsDirector = m_container.Find("Button_Director").gameObject;
			playAsDirector.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.as.director");
			playAsDirector.GetComponent<Button>().onClick.AddListener(PlayAsDirector);

            GameObject playAsSpectator = m_container.Find("Button_Spectator").gameObject;
            if (playAsSpectator != null)
            {
                playAsSpectator.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.as.spectator");
                playAsSpectator.GetComponent<Button>().onClick.AddListener(PlayAsSpectator);
            }
            MultiplayerConfiguration.SaveSpectatorMode(MultiplayerConfiguration.SPECTATOR_MODE_DISABLED);

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
		* PlayAsCustomer
		*/
        private void PlayAsCustomer()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_DISABLED);
			if (MenuScreenController.Instance.EnableAppOrganization)
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENDIRECTORMODE_SELECTED_PROFILE, FunctionsScreenController.PROFILE_PLAYER.PLAYER);
				GoBackPressed();
			}
			else
			{
				NextscreenForPlayAsCustomer();
			}
        }

		// -------------------------------------------
		/* 
		* NextscreenForPlayAsCustomer
		*/
		protected virtual void NextscreenForPlayAsCustomer()
        {
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenCharacterSelectionView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		* PlayAsDirector
		*/
		private void PlayAsDirector()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_ENABLED);
			MultiplayerConfiguration.SaveGoogleARCore(MultiplayerConfiguration.GOOGLE_ARCORE_DISABLED);
			CardboardLoaderVR.Instance.SaveEnableCardboard(false);
			if (MenuScreenController.Instance.EnableAppOrganization)
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENDIRECTORMODE_SELECTED_PROFILE, FunctionsScreenController.PROFILE_PLAYER.DIRECTOR);
				GoBackPressed();
			}
			else
            {
				if (NetworkEventController.Instance.MenuController_LoadNumberOfPlayers() != MultiplayerConfiguration.VALUE_FOR_JOINING)
				{
					MultiplayerConfiguration.SaveCharacter6DOF(0);
					MultiplayerConfiguration.SaveLevel6DOF(0);
				}

				MenuScreenController.Instance.LoadGameScene(this);
			}
		}

		// -------------------------------------------
		/* 
		* PlayAsSpectator
		*/
		private void PlayAsSpectator()
        {
			if (MenuScreenController.Instance.EnableAppOrganization)
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENDIRECTORMODE_SELECTED_PROFILE, FunctionsScreenController.PROFILE_PLAYER.SPECTATOR);
				GoBackPressed();
			}
			else
			{
				if (NetworkEventController.Instance.MenuController_LoadNumberOfPlayers() == MultiplayerConfiguration.VALUE_FOR_JOINING)
				{
					MultiplayerConfiguration.SaveDirectorMode(MultiplayerConfiguration.DIRECTOR_MODE_ENABLED);
					MultiplayerConfiguration.SaveSpectatorMode(MultiplayerConfiguration.SPECTATOR_MODE_ENABLED);
					CardboardLoaderVR.Instance.SaveEnableCardboard(false);
					MultiplayerConfiguration.SaveGoogleARCore(MultiplayerConfiguration.GOOGLE_ARCORE_DISABLED);

					MenuScreenController.Instance.LoadGameScene(this);
				}
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