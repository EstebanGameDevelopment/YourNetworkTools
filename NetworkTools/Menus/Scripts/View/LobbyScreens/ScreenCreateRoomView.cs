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
	 * ScreenCreateRoomView
	 * 
	 * Screen where we can create a room
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenCreateRoomView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_CREATE_ROOM";

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREENCREATEROOM_CREATE_RANDOM_NAME = "EVENT_SCREENCREATEROOM_CREATE_RANDOM_NAME";
        public const string EVENT_SCREENCREATEROOM_SETUP_NAME = "EVENT_SCREENCREATEROOM_SETUP_NAME";
		
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string PLAYERPREFS_YNT_ROOMNAME = "PLAYERPREFS_YNT_ROOMNAME";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
		private Transform m_container;

        private Transform m_btnBack;

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

			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.create.with.description.room");
			m_container.Find("RoomName").GetComponent<InputField>().text = PlayerPrefs.GetString(PLAYERPREFS_YNT_ROOMNAME, "");

			GameObject createGame = m_container.Find("Button_CreateRoom").gameObject;
			if (MenuScreenController.Instance.EnableAppOrganization)
			{
				createGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.amics.setup.room.name");
			}
			else
			{
				createGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.create.with.name.room");
			}
			createGame.GetComponent<Button>().onClick.AddListener(CreateRoom);

            m_btnBack = m_container.Find("Button_Back");
            if (m_btnBack != null)
            {
                Button bkBtn = m_btnBack.GetComponent<Button>();
                if (bkBtn != null)
                {
                    bkBtn.onClick.AddListener(BackPressed);
                }
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
		 * CreateRoom
		 */
		private void CreateRoom()
		{
			string roomName = m_container.Find("RoomName").GetComponent<InputField>().text;
			if (roomName.Length < 5)
			{
				SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN,ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.lobby.no.name.in.create.room"), null, "");
			}
			else
			{
				PlayerPrefs.SetString(PLAYERPREFS_YNT_ROOMNAME, roomName);
				if (MenuScreenController.Instance.EnableAppOrganization)
				{
					UIEventController.Instance.DispatchUIEvent(EVENT_SCREENCREATEROOM_SETUP_NAME, roomName);
					GoBackPressed();
				}
				else
				{
					SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
					NetworkEventController.Instance.MenuController_SetNameRoomLobby(roomName);
					if (MenuScreenController.Instance.ForceFixedPlayers != -1)
					{
						MenuScreenController.Instance.LoadCustomGameScreenOrCreateGame(false, MenuScreenController.Instance.ForceFixedPlayers, "", null);
					}
					else
					{
						if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
						{
							UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenMenuNumberPlayersView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
						}
						else
						{
							UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuNumberPlayersView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, m_nameOfScreen);
						}
					}
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
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
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
                    if (m_btnBack != null)
                    {
                        BackPressed();
                    }                        
                }
            }
            if (_nameEvent == EVENT_SCREENCREATEROOM_CREATE_RANDOM_NAME)
            {
                m_container.Find("RoomName").GetComponent<InputField>().text = "NameRoom" + UnityEngine.Random.Range(1000, 100000);
            }
        }
    }
}
