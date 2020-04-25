﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * ItemLobbyRoomView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ItemLobbyRoomView : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_ITEM_ROOM_LOBBY_SELECTED = "EVENT_ITEM_ROOM_LOBBY_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private int m_room;
		private string m_ipAddress;
		private int m_port;
		private Text m_text;
		private Image m_background;
		private bool m_selected = false;
		private GameObject m_selector;
        private string m_extraData;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public int Room
		{
			get { return m_room; }
		}
        public string DisplayName
        {
            get { return m_text.text; }
        }
        public string IPAddress
		{
			get { return m_ipAddress; }
		}
		public int Port
		{
			get { return m_port; }
		}
		public virtual bool Selected
		{
			get { return m_selected; }
			set
			{
				m_selected = value;
				if (m_selected)
				{
					m_background.color = Color.cyan;
				}
				else
				{
					m_background.color = Color.white;
				}
			}
		}
        public string ExtraData
        {
            get { return m_extraData; }
        }

		// -------------------------------------------
		/* 
		 * Initialization
		 */
		public void Initialization(int _room, string _facebookName, string _ipAddress, int _port, string _extraData)
		{
			m_room = _room;
			m_ipAddress = _ipAddress;
			m_port = _port;
			m_text = transform.Find("Text").GetComponent<Text>();
			m_background = transform.GetComponent<Image>();
			transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);
			m_text.text = _facebookName;
            m_extraData = _extraData;
        }

		// -------------------------------------------
		/* 
		 * ButtonPressed
		 */
		public void ButtonPressed()
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_ITEM_ROOM_LOBBY_SELECTED, this);
		}
	}
}