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
	 * ScreenSetUpAddressView
	 * 
	 * Screen where we can create a room
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenSetUpAddressView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SETUP_SERVER";

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREENCREATEROOM_CREATE_RANDOM_NAME = "EVENT_SCREENCREATEROOM_CREATE_RANDOM_NAME";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
#if !ENABLE_MY_OFUSCATION || UNITY_EDITOR
        public const string PLAYERPREFS_YNT_IP = "PLAYERPREFS_YNT_IP";
        public const string PLAYERPREFS_YNT_PORT = "PLAYERPREFS_YNT_PORT";
#else
        public const string PLAYERPREFS_YNT_IP = "^PLAYERPREFS_YNT_IP^";
        public const string PLAYERPREFS_YNT_PORT = "^PLAYERPREFS_YNT_PORT^";
#endif

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
            m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.setup.server.description");

            m_container.Find("IPTitle").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.setup.server.ip");
			m_container.Find("PortNumberTitle").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.setup.server.portnumber");

            m_container.Find("IPAddress").GetComponent<InputField>().text = PlayerPrefs.GetString(PLAYERPREFS_YNT_IP, "");
            m_container.Find("PortAddress").GetComponent<InputField>().text = PlayerPrefs.GetString(PLAYERPREFS_YNT_PORT, "");

            GameObject createGame = m_container.Find("Button_ConfirmAddress").gameObject;
			createGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.setup.server.confirm");
			createGame.GetComponent<Button>().onClick.AddListener(CreateRoom);

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
		 * CreateRoom
		 */
		private void CreateRoom()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			string ipAddress = m_container.Find("IPAddress").GetComponent<InputField>().text;
			int portNumber = int.Parse(m_container.Find("PortAddress").GetComponent<InputField>().text);
			if (ipAddress.Length < 5)
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.lobby.setup.server.invalid.ip"), null, "");
			}
			else
			{
				if (portNumber < 2000)
				{
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.lobby.setup.server.invalid.portnumber"), null, "");
                }
				else
				{
                    PlayerPrefs.SetString(PLAYERPREFS_YNT_IP, ipAddress);
                    PlayerPrefs.SetString(PLAYERPREFS_YNT_PORT, portNumber.ToString());
                    MultiplayerConfiguration.SaveIPAddressServer(ipAddress);
                    MultiplayerConfiguration.SavePortServer(portNumber);
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenRemoteModeView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                    Destroy();
                }
			}
		}

		// -------------------------------------------
		/* 
		 * Exit button pressed
		 */
		private void BackPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
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
