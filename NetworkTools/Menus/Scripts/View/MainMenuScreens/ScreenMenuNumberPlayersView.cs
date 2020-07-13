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
	 * ScreenMenuNumberPlayersView
	 * 
	 * Screen where we define the initial number of players that will have the game
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMenuNumberPlayersView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_PLAYER_NUMBER";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
		private Transform m_container;

		private int m_finalNumberOfPlayers;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public int FinalNumberOfPlayers
        {
            get
            {
                string numberOfPlayers = m_container.Find("PlayerValue").GetComponent<InputField>().text;

                // NUMBER OF PLAYERS
                m_finalNumberOfPlayers = -1;
                if (!int.TryParse(numberOfPlayers, out m_finalNumberOfPlayers))
                {
                    m_finalNumberOfPlayers = -1;
                }

                return m_finalNumberOfPlayers;
            }
            set
            {
                if ((value > 0) && (value <= MenuScreenController.Instance.MaxPlayers))
                {
                    m_finalNumberOfPlayers = value;
                }
                else
                {
                    if (value <= 0)
                    {
                        m_finalNumberOfPlayers = 1;
                    }
                    else
                    {
                        m_finalNumberOfPlayers = MenuScreenController.Instance.MaxPlayers;
                    }
                }
                m_container.Find("PlayerValue").GetComponent<InputField>().text = m_finalNumberOfPlayers.ToString();
            }
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.player.number.create.new.game");

			m_container.Find("Button_Ok").GetComponent<Button>().onClick.AddListener(ConfirmNumberPlayers);
			m_container.Find("Button_Ok/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.player.number.create.new.game");

			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.player.number.description");

            if (m_container.Find("Button_Plus") != null)
            {
                m_container.Find("Button_Plus").GetComponent<Button>().onClick.AddListener(IncreasePlayerNumber);
            }

            if (m_container.Find("Button_Minus") != null)
            {
                m_container.Find("Button_Minus").GetComponent<Button>().onClick.AddListener(DecreasePlayerNumber);
            }

            UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);

            m_container.Find("PlayerValue").GetComponent<InputField>().text = "2";
            m_container.Find("PlayerValue").GetComponent<InputField>().onEndEdit.AddListener(OnEndEditNumber);
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
		 * OnEndEditNumber
		 */
        private void OnEndEditNumber(string arg0)
        {
            string numberOfPlayers = m_container.Find("PlayerValue").GetComponent<InputField>().text;

            // NUMBER OF PLAYERS
            int finalNumberOfPlayers = -1;
            if (int.TryParse(numberOfPlayers, out finalNumberOfPlayers))
            {
                FinalNumberOfPlayers = finalNumberOfPlayers;
            }
            else
            {
                m_container.Find("PlayerValue").GetComponent<InputField>().text = m_finalNumberOfPlayers.ToString();
            }
        }

        // -------------------------------------------
        /* 
		 * DecreasePlayerNumber
		 */
        private void DecreasePlayerNumber()
        {
            FinalNumberOfPlayers = FinalNumberOfPlayers - 1;
        }

        // -------------------------------------------
        /* 
		 * IncreasePlayerNumber
		 */
        private void IncreasePlayerNumber()
        {
            FinalNumberOfPlayers = FinalNumberOfPlayers + 1;
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
		 * ConfirmNumberPlayers
		 */
		private void ConfirmNumberPlayers()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);

			string numberOfPlayers = m_container.Find("PlayerValue").GetComponent<InputField>().text;


			// NUMBER OF PLAYERS
			m_finalNumberOfPlayers = -1;
			if (!int.TryParse(numberOfPlayers, out m_finalNumberOfPlayers))
			{
				m_finalNumberOfPlayers = -1;
			}
            UIEventController.Instance.DispatchUIEvent(MenuScreenController.EVENT_MENUEVENTCONTROLLER_CREATED_NEW_GAME, m_finalNumberOfPlayers);
            Destroy();
            MenuScreenController.Instance.LoadCustomGameScreenOrCreateGame(false, m_finalNumberOfPlayers, "", null);
		}
	}
}