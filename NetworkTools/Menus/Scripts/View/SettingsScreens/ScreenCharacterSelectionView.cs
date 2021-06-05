using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;
#if ENABLE_YOURVRUI
using YourVRUI;
#endif

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * ScreenCharacterSelectionView
	 * 
	 * Select the avatar character we want
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenCharacterSelectionView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_CHARACTER_SELECTION";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENCHARACTERSELECTION_SELECTED_CHARACTER = "EVENT_SCREENCHARACTERSELECTION_SELECTED_CHARACTER";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject ImageItemPrefab;
		public Sprite[] CharacterSprites;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private GameObject m_slotCharactersList;

		private Button m_select;
		private Button m_buttonBack;

		private int m_indexSelected = -1;

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

			m_select = m_container.Find("Button_Select").GetComponent<Button>();
			m_container.Find("Button_Select/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.character.selection.confirmation");
			m_select.onClick.AddListener(SelectedCharacter);

			m_slotCharactersList = m_container.Find("ScrollingArea").gameObject;
			List<ItemMultiObjectEntry> characters = new List<ItemMultiObjectEntry>();
			for (int i = 0; i < CharacterSprites.Length; i++)
			{
				characters.Add(new ItemMultiObjectEntry(this.gameObject, i, LanguageController.Instance.GetText("player.name." + CharacterSprites[i].name), CharacterSprites[i]));
			}			
			m_slotCharactersList.GetComponent<SlotManagerView>().Initialize(6, characters, ImageItemPrefab);

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);

            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_RELOAD_SCREEN_DATA);

			/*
            if (SixDOFConfiguration.TOTAL_CHARACTERS == 1)
            {
                m_indexSelected = 0;
                if (YourVRUIScreenController.Instance != null)
                {
                    m_container.gameObject.SetActive(false);
                }
                Invoke("SelectedCharacter", 0.1f);
            }
			*/
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

			m_slotCharactersList.GetComponent<SlotManagerView>().Destroy();

			UIEventController.Instance.UIEvent -= OnMenuEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			return false;
		}


        // -------------------------------------------
        /* 
		 * SelectedCharacter
		 */
        private void SelectedCharacter()
		{
            if (m_indexSelected == -1)
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.you.should.select.an.item"), null, "");
            }
            else
            {
                SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
				MultiplayerConfiguration.SaveCharacter6DOF(m_indexSelected);
				MultiplayerConfiguration.SaveLevel6DOF(0);
				NextScreen();
			}
		}

		// -------------------------------------------
		/*
		* NextScreen
		*/
		private void NextScreen()
        {
			if (NetworkEventController.Instance.MenuController_LoadNumberOfPlayers() == MultiplayerConfiguration.VALUE_FOR_JOINING)
            {
				LoadGame(false);
			}
			else
            {
				LoadGame(true);
			}				
		}

		// -------------------------------------------
		/*
		* LoadGame
		*/
		private void LoadGame(bool _setLevel)
        {
			if (MenuScreenController.Instance.EnableAppOrganization)
            {
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENCHARACTERSELECTION_SELECTED_CHARACTER, m_indexSelected);
				GoBackPressed();
			}
			else
            {
#if UNITY_STANDALONE
                CardboardLoaderVR.Instance.SaveEnableCardboard(false);
                MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
                Destroy();
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
#else
				MultiplayerConfiguration.SaveGoogleARCore(MultiplayerConfiguration.GOOGLE_ARCORE_DISABLED);
				if (YourVRUIScreenController.Instance == null)
				{
					CardboardLoaderVR.Instance.SaveEnableCardboard(false);
				}
				else
				{
					CardboardLoaderVR.Instance.SaveEnableCardboard(true);
				}
				MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
				Destroy();
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
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

			if (_nameEvent == ItemImageView.EVENT_ITEM_IMAGE_SELECTED)
			{
				m_indexSelected = (int)_list[2];
			}
		}
	}
}