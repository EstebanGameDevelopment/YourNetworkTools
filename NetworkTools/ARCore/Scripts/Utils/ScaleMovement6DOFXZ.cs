using System;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{

    /******************************************
	 * 
	 * ScaleMovement6DOFXZ
	 * 
	 * Scales the movement on XZ
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScaleMovement6DOFXZ : MonoBehaviour
	{
        // ----------------------
        // PRIVATE MEMBERS
        // ----------------------
        private float m_currentScaleMovementXZ = -1;
        private Text m_textScale;
        private GameObject m_buttonUp;
        private GameObject m_buttonDown;

        private bool m_pressedUp = false;
        private bool m_pressedDown = false;

        private float m_timeAcumButton = 0;

        private bool m_hasBeenInitialized = false;

        // -------------------------------------------
        /* 
		* Start
		*/
        void Awake()
        {
            m_buttonUp = this.gameObject.transform.Find("Up").gameObject;
            m_buttonDown = this.gameObject.transform.Find("Down").gameObject;
            m_textScale = this.gameObject.transform.Find("Scale").GetComponent<Text>();
            m_textScale.text = "";

            NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
        }

        // -------------------------------------------
        /* 
		* OnDestroy
		*/
        void OnDestroy()
        {
            NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
            UIEventController.Instance.UIEvent -= OnUIEvent;
        }

        // -------------------------------------------
        /* 
		* OnEnable
		*/
        void OnEnable()
        {
            NetworkEventController.Instance.DispatchLocalEvent(CloudGameAnchorController.EVENT_6DOF_REQUEST_SCALE_MOVEMENT_XZ);
        }

        // -------------------------------------------
        /* 
		* OnUIEvent
		*/
        private void OnUIEvent(string _nameEvent, object[] _list)
        {
            if (!m_hasBeenInitialized) return;

            if (_nameEvent == CustomButton.BUTTON_PRESSED_DOWN)
            {
                if (m_buttonUp == (GameObject)_list[0])
                {
                    m_pressedUp = true;
                    m_timeAcumButton = 0;
                }
                if (m_buttonDown == (GameObject)_list[0])
                {
                    m_pressedDown = true;
                    m_timeAcumButton = 0;
                }
            }
            if (_nameEvent == CustomButton.BUTTON_RELEASE_UP)
            {
                if (m_buttonUp == (GameObject)_list[0])
                {
                    m_pressedUp = false;
                    m_timeAcumButton = 0;
                    m_currentScaleMovementXZ += 1;
                    NetworkEventController.Instance.DispatchNetworkEvent(CloudGameAnchorController.EVENT_6DOF_UPDATE_SCALE_MOVEMENT_XZ, m_currentScaleMovementXZ.ToString());
                }
                if (m_buttonDown == (GameObject)_list[0])
                {
                    m_pressedDown = false;
                    m_timeAcumButton = 0;
                    m_currentScaleMovementXZ -= 1;
                    NetworkEventController.Instance.DispatchNetworkEvent(CloudGameAnchorController.EVENT_6DOF_UPDATE_SCALE_MOVEMENT_XZ, m_currentScaleMovementXZ.ToString());
                }
            }
        }

        // -------------------------------------------
        /* 
		* OnNetworkEvent
		*/
        private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, object[] _list)
        {
            if (_nameEvent == CloudGameAnchorController.EVENT_6DOF_UPDATE_SCALE_MOVEMENT_XZ)
            {
                m_currentScaleMovementXZ = float.Parse((string)_list[0]);
                m_textScale.text = m_currentScaleMovementXZ.ToString();
                if (!m_hasBeenInitialized)
                {
                    m_hasBeenInitialized = true;
                }
            }
        }

        // -------------------------------------------
        /* 
		* Update
		*/
        void Update()
        {
            if (!m_hasBeenInitialized) return;

            if (m_pressedUp || m_pressedDown)
            {
                m_timeAcumButton += Time.deltaTime;
                if (m_timeAcumButton > 1.5f)
                {
                    if (m_timeAcumButton > 1.7f)
                    {
                        m_timeAcumButton = 1.5f;
                        if ((m_currentScaleMovementXZ > 0) && (m_currentScaleMovementXZ < 99))
                        {
                            if (m_pressedDown)
                            {
                                m_currentScaleMovementXZ -= 1;
                            }
                            else
                            {
                                m_currentScaleMovementXZ += 1;
                            }
                            NetworkEventController.Instance.DispatchNetworkEvent(CloudGameAnchorController.EVENT_6DOF_UPDATE_SCALE_MOVEMENT_XZ, m_currentScaleMovementXZ.ToString());
                        }
                    }
                }
            }
        }
    }
}