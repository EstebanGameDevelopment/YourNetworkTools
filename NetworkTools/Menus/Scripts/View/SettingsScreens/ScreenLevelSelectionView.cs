using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;
using YourNetworkingTools;
#if ENABLE_YOURVRUI
using YourVRUI;
#endif

namespace YourNetworkingTools
{
    /******************************************
	 * 
	 * ScreenLevelSelectionView
	 * 
	 * Select the house we want
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenLevelSelectionView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_LEVEL_SELECTION";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public const string EVENT_SCREENLEVELSELECTION_SELECTED_LEVEL = "EVENT_SCREENLEVELSELECTION_SELECTED_LEVEL";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject ImageItemPrefab;
		public Sprite[] LevelSprites;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private GameObject m_slotLevelList;

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
			m_container.Find("Button_Select/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.house.selection.confirmation");
			m_select.onClick.AddListener(SelectedLevel);

			m_slotLevelList = m_container.Find("ScrollingArea").gameObject;
			List<ItemMultiObjectEntry> houses = new List<ItemMultiObjectEntry>();
			for (int i = 0; i < LevelSprites.Length; i++)
			{
				houses.Add(new ItemMultiObjectEntry(this.gameObject, i, LanguageController.Instance.GetText(LevelSprites[i].name), LevelSprites[i]));
			}			
			m_slotLevelList.GetComponent<SlotManagerView>().Initialize(6, houses, ImageItemPrefab);

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);

            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_RELOAD_SCREEN_DATA);

			/*
            if (SixDOFConfiguration.TOTAL_LEVELS == 1)
            {
                m_indexSelected = 0;
                if (YourVRUIScreenController.Instance != null)
                {
                    m_container.gameObject.SetActive(false);
                }
                Invoke("SelectedLevel", 0.1f);
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

			m_slotLevelList.GetComponent<SlotManagerView>().Destroy();

			UIEventController.Instance.UIEvent -= OnMenuEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			return false;
		}


        // -------------------------------------------
        /* 
		 * SelectedLevel
		 */
        private void SelectedLevel()
		{
            if (m_indexSelected == -1)
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.you.should.select.an.item"), null, "");
            }
            else
            {
                SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
				MultiplayerConfiguration.SaveLevel6DOF(m_indexSelected);
				NextScreen();
            }
        }

		// -------------------------------------------
		/*
		* NextScreen
		*/
		private void NextScreen()
		{
			if (MenuScreenController.Instance.EnableAppOrganization)
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_SCREENLEVELSELECTION_SELECTED_LEVEL, m_indexSelected);
				GoBackPressed();
			}
			else
            {
#if ENABLE_GOOGLE_ARCORE
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEnableARCore.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#else
				MenuScreenController.Instance.LoadGameScene(this);
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