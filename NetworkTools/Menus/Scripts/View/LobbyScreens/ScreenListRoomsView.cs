using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * ScreenListRoomsView
	 * 
	 * Display the list of rooms available to join
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenListRoomsView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_LIST_ROOMS";

        // ----------------------------------------------
        // PUBLIC CONSTANTS
        // ----------------------------------------------	
        public const bool ENABLE_FILTERING = true;

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public GameObject RoomLobbyItemPrefab;
        public GameObject SearchIcon;
        public GameObject JoinIcon;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
		private Transform m_container;
		private GameObject m_grid;

		private Button m_joinRoom;
		private Button m_buttonBack;
		private List<ItemLobbyRoomView> m_rooms = new List<ItemLobbyRoomView>();

        private string m_screenBack = "";
        private InputField m_nameRoom;
        private Text m_buttonText;
        private bool m_filledList = false;

        private List<GameObject> m_roomsGO = new List<GameObject>();


        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

            if (_list.Length > 0)
            {
                if (_list[0] != null)
                {
                    m_screenBack = (string)_list[0];
                }                
            }

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");

			m_joinRoom = m_container.Find("Button_Join").GetComponent<Button>();
            m_buttonText = m_container.Find("Button_Join/Text").GetComponent<Text>();
            m_buttonText.text = LanguageController.Instance.GetText("screen.lobby.join.the.selected.room");
			m_joinRoom.onClick.AddListener(OnJoinRoom);

			m_buttonBack = m_container.Find("Button_Back").GetComponent<Button>();
			m_buttonBack.onClick.AddListener(BackPressed);

			m_grid = m_container.Find("ScrollList/Grid").gameObject;

            if (m_container.Find("RoomName") != null)
            {
                m_nameRoom = m_container.Find("RoomName").GetComponent<InputField>();
                if (m_nameRoom != null)
                {
                    m_nameRoom.onValueChanged.AddListener(OnNameRoom);
                }
            }

            if (!ENABLE_FILTERING || (m_nameRoom == null))
            {
                if (m_nameRoom != null) m_nameRoom.gameObject.SetActive(false);
                m_joinRoom.gameObject.SetActive(true);
                if (SearchIcon != null) SearchIcon.SetActive(false);
                if (JoinIcon != null) JoinIcon.SetActive(true);
                m_filledList = true;

                // JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
			UIEventController.Instance.DelayUIEvent(MenuScreenController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE, 0.1f);
			CommsHTTPConfiguration.GetListRooms(true, "PLAYER_LOBBY");
#else
                LoadRooms(NetworkEventController.Instance.RoomsLobby);
#endif
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
		 * LoadRooms
		 */
        private void LoadRooms(List<ItemMultiTextEntry> _rooms, string _nameRoom = "")
        {
            ClearPreviousRooms();

            for (int i = 0; i < _rooms.Count; i++)
            {
                ItemMultiTextEntry room = _rooms[i];
                string nameRoomItem = "";
#if ENABLE_BALANCE_LOADER
				nameRoomItem = (string)room.Items[1];
#else
                nameRoomItem = (string)room.Items[2];
#endif
                string tname = nameRoomItem.ToLower();
                string oname = _nameRoom.ToLower();
                if ((tname.IndexOf(oname) != -1) || !ENABLE_FILTERING)
                {
                    GameObject instance = Utilities.AddChild(m_grid.transform, RoomLobbyItemPrefab);
                    m_roomsGO.Add(instance);

                    // JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
				instance.GetComponent<ItemLobbyRoomView>().Initialization(int.Parse(room.Items[0]), room.Items[1], room.Items[2], int.Parse(room.Items[3]), room.Items[4]);
#else
                    instance.GetComponent<ItemLobbyRoomView>().Initialization(int.Parse(room.Items[1]), room.Items[2], MultiplayerConfiguration.SOCKET_SERVER_ADDRESS, MultiplayerConfiguration.PORT_SERVER_ADDRESS, room.Items[3]);
#endif
                    m_rooms.Add(instance.GetComponent<ItemLobbyRoomView>());
                }

            }

            if (ENABLE_FILTERING)
            {
                if (m_rooms.Count == 0)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.lobby.list.no.room.name"), null, "");
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_RELOAD_SCREEN_DATA, true);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * OnNameRoom
		 */
        private void OnNameRoom(string _name)
        {
            if (_name.Length >= 6)
            {
                m_joinRoom.gameObject.SetActive(true);
                if (SearchIcon != null) SearchIcon.SetActive(true);
                if (JoinIcon != null) JoinIcon.SetActive(false);
                m_filledList = false;
                m_buttonText.text = LanguageController.Instance.GetText("screen.lobby.search.room.name");
            }
            else
            {
                m_joinRoom.gameObject.SetActive(false);
                ClearPreviousRooms();
            }
        }

        // -------------------------------------------
        /* 
		 * ClearPreviousRooms
		 */
        private void ClearPreviousRooms()
        {
            foreach (GameObject item in m_roomsGO)
            {
                GameObject.Destroy(item);
            }
            m_roomsGO.Clear();
            m_rooms.Clear();
        }

        // -------------------------------------------
        /* 
		 * BackPressed
		 */
        private void BackPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            if (m_screenBack.Length > 0)
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, m_screenBack, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
            }			
		}

        // -------------------------------------------
        /* 
		 * OnJoinRoom
		 */
        private void OnJoinRoom()
        {
            if (!m_filledList)
            {
                // JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
			UIEventController.Instance.DelayUIEvent(MenuScreenController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE, 0.1f);
			CommsHTTPConfiguration.GetListRooms(true, "PLAYER_LOBBY");
#else
                if (m_nameRoom != null)
                {
                    LoadRooms(NetworkEventController.Instance.RoomsLobby, m_nameRoom.text);
                }
                else
                {
                    LoadRooms(NetworkEventController.Instance.RoomsLobby);
                }
#endif
                m_joinRoom.gameObject.SetActive(false);
            }
            else
            {
                ItemLobbyRoomView roomSelected = null;
                for (int i = 0; i < m_rooms.Count; i++)
                {
                    if (m_rooms[i].Selected)
                    {
                        roomSelected = m_rooms[i];
                    }
                }

                if (roomSelected != null)
                {
                    PlayerPrefs.SetString(ScreenCreateRoomView.PLAYERPREFS_YNT_ROOMNAME, roomSelected.DisplayName);
                    NetworkEventController.Instance.MenuController_SaveRoomNumberInServer(roomSelected.Room);
                    NetworkEventController.Instance.MenuController_SaveRoomNameInServer(roomSelected.DisplayName);
                    MenuScreenController.Instance.ExtraData = roomSelected.ExtraData;

                    // JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
				NetworkEventController.Instance.MenuController_SaveIPAddressServer(roomSelected.IPAddress);
				NetworkEventController.Instance.MenuController_SavePortServer(roomSelected.Port);
#endif
                    JoinGamePressed();
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.you.should.select.an.item"), null, "");
                }
            }
        }

        // -------------------------------------------
        /* 
		 * JoinGamePressed
		 */
        private void JoinGamePressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(MultiplayerConfiguration.VALUE_FOR_JOINING);
			MenuScreenController.Instance.CreateOrJoinRoomInServer(true);
		}

		// -------------------------------------------
		/*
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

            if (_nameEvent == ItemLobbyRoomView.EVENT_ITEM_ROOM_LOBBY_SELECTED)
            {
                ItemLobbyRoomView itemLobbyRoomView = (ItemLobbyRoomView)_list[0];
                for (int i = 0; i < m_rooms.Count; i++)
                {
                    if (m_rooms[i] == itemLobbyRoomView)
                    {
                        m_rooms[i].Selected = true;
                        m_buttonText.text = LanguageController.Instance.GetText("screen.lobby.join.the.selected.room");
                        m_joinRoom.gameObject.SetActive(true);
                        if (SearchIcon != null) SearchIcon.SetActive(false);
                        if (JoinIcon != null) JoinIcon.SetActive(true);
                        m_filledList = true;
                    }
                    else
                    {
                        m_rooms[i].Selected = false;
                    }
                }
            }
            if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS)
            {
                if (m_nameRoom != null)
                {
                    LoadRooms(NetworkEventController.Instance.RoomsLobby, m_nameRoom.text);
                }
                else
                {
                    LoadRooms(NetworkEventController.Instance.RoomsLobby);
                }
            }
            if (_nameEvent == MenuScreenController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE)
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
			}
			if (_nameEvent == GetListRoomsHTTP.EVENT_CLIENT_HTTP_LIST_OF_GAME_ROOMS)
			{
				UIEventController.Instance.DispatchUIEvent(MenuScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
				if (_list.Length == 1)
				{
                    if (m_nameRoom != null)
                    {
                        LoadRooms((List<ItemMultiTextEntry>)_list[0], m_nameRoom.text);
                    }
                    else
                    {
                        LoadRooms((List<ItemMultiTextEntry>)_list[0]);
                    }
				}
				else
				{
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN,ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.room.list.not.retrieved"), null, "");
				}
			}
		}
	}
}