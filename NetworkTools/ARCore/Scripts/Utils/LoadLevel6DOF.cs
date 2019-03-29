using System;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{
    /******************************************
	 * 
	 * LoadLevel6DOF
	 * 
	 * Load next or previous level
	 * 
	 * @author Esteban Gallardo
	 */
    public class LoadLevel6DOF : MonoBehaviour
	{
        // ----------------------
        // PRIVATE MEMBERS
        // ----------------------
        private int m_currentLevel = -1;
        private int m_totalNumberLevels = -1;
        private Text m_textLevel;
        private GameObject m_buttonNext;
        private GameObject m_buttonPrevious;

        private bool m_hasBeenInitialized = false;

        // -------------------------------------------
        /* 
		* Start
		*/
        void Awake()
        {
            m_buttonNext = this.gameObject.transform.Find("Next").gameObject;
            m_buttonNext.GetComponent<Button>().onClick.AddListener(OnNextLevel);
            m_buttonPrevious = this.gameObject.transform.Find("Previous").gameObject;
            m_buttonPrevious.GetComponent<Button>().onClick.AddListener(OnPreviousLevel);
            m_textLevel = this.gameObject.transform.Find("Level").GetComponent<Text>();
            m_textLevel.text = "";

            NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
        }

        // -------------------------------------------
        /* 
		* OnDestroy
		*/
        void OnDestroy()
        {
            NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
        }

        // -------------------------------------------
        /* 
		* OnEnable
		*/
        void OnEnable()
        {
            if (m_hasBeenInitialized) return;

            NetworkEventController.Instance.DispatchLocalEvent(CloudGameAnchorController.EVENT_6DOF_REQUEST_LEVEL_NUMBER);
        }

        // -------------------------------------------
        /* 
		* OnPreviousLevel
		*/
        private void OnPreviousLevel()
        {
            if (!m_hasBeenInitialized) return;

            m_currentLevel -= 1;
            if (m_currentLevel < 0) m_currentLevel = 0;
            NetworkEventController.Instance.DispatchNetworkEvent(CloudGameAnchorController.EVENT_6DOF_CHANGE_LEVEL, m_currentLevel.ToString());
            this.gameObject.SetActive(false);
        }

        // -------------------------------------------
        /* 
		* OnNextLevel
		*/
        private void OnNextLevel()
        {
            if (!m_hasBeenInitialized) return;

            m_currentLevel += 1;
            if (m_currentLevel >= m_totalNumberLevels) m_currentLevel = m_totalNumberLevels - 1;
            NetworkEventController.Instance.DispatchNetworkEvent(CloudGameAnchorController.EVENT_6DOF_CHANGE_LEVEL, m_currentLevel.ToString());
            this.gameObject.SetActive(false);
        }

        // -------------------------------------------
        /* 
		* OnNetworkEvent
		*/
        private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, object[] _list)
        {
            if (_nameEvent == CloudGameAnchorController.EVENT_6DOF_CHANGE_LEVEL)
            {
                m_currentLevel = int.Parse((string)_list[0]);
                m_textLevel.text = m_currentLevel.ToString();
            }
            if (_nameEvent == CloudGameAnchorController.EVENT_6DOF_RESPONSE_LEVEL_NUMBER)
            {
                if (!m_hasBeenInitialized)
                {
                    m_hasBeenInitialized = true;
                    m_currentLevel = int.Parse((string)_list[0]);
                    m_totalNumberLevels = int.Parse((string)_list[1]);
                    this.gameObject.SetActive((m_totalNumberLevels > 1));
                    m_textLevel.text = m_currentLevel.ToString();
                }
            }
        }

        // -------------------------------------------
        /* 
		* OnBasicSystemEvent
		*/
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == CloudGameAnchorController.EVENT_6DOF_CHANGED_LEVEL_COMPLETED)
            {
                this.gameObject.SetActive((m_totalNumberLevels > 1));
            }
        }
    }
}