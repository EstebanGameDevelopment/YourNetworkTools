#if ENABLE_PHOTON
using Photon.Pun;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	* 
	* ActorNetwork
	* 
	* Base class of the common properties of a game's actor
	* 
	* @author Esteban Gallardo
	*/
	public class ActorNetwork : MonoBehaviour
	{
		// ----------------------------------------------
		// PROTECTED MEMBERS
		// ----------------------------------------------	
		private NetworkID m_networkID;
		private string m_eventNameObjectCreated = "";

#if ENABLE_PHOTON
        private PhotonView m_photonView;
#endif

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public NetworkID NetworkID
		{
			get
			{
				if (YourNetworkTools.Instance.IsLocalGame)
				{
					Initialize();
				}
				if (m_networkID == null)
				{
					m_networkID = this.gameObject.GetComponent<NetworkID>();
				}
				return m_networkID;
			}
		}
		public string EventNameObjectCreated
		{
			set { m_eventNameObjectCreated = value; }
		}

#if ENABLE_PHOTON
        // -------------------------------------------
        /* 
		* Awake
		*/
        public void Awake()
        {
            if (!YourNetworkTools.Instance.IsLocalGame)
            {
                m_photonView = GetComponent<PhotonView>();

                if (m_photonView.InstantiationData != null)
                {
                    NetworkID.NetID = (int)(m_photonView.InstantiationData[0]);
                    NetworkID.UID = (int)(m_photonView.InstantiationData[1]);
                }
            }
        }
#endif

        // -------------------------------------------
        /* 
		 * Report the event in the system when a new player has been created.
		 * 
		 * The player could have been created by a remote client so we should throw an event
		 * so that the controller will be listening to it.
		 */
        void Start()
		{
			if (m_eventNameObjectCreated == "")
			{
				Debug.LogError("ReportCreationObject::YOU SHOULD DEFINE IN THE CONSTRUCTOR THE EVENT TO REPORT THE CREATION OF THE GAME OBJECT IN THE SYSTEM");
			}
			NetworkEventController.Instance.DispatchLocalEvent(m_eventNameObjectCreated, this.gameObject);
            NetworkEventController.Instance.DispatchLocalEvent(YourNetworkTools.EVENT_YOURNETWORKTOOLS_CREATED_GAMEOBJECT, this.gameObject);
            if (IsMine())
			{
                NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_LOCAL_CREATION_CONFIRMATION, this.gameObject);
			}
			else
			{
				NetworkEventController.Instance.PriorityDelayNetworkEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_REMOTE_CREATION_CONFIRMATION, 0.01f, NetworkID.GetID());
			}
            NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
#if ENABLE_PHOTON
            if (!YourNetworkTools.Instance.IsLocalGame)
            {
                this.gameObject.transform.parent = YourNetworkTools.Instance.gameObject.transform;
            }
#endif           
        }

        // -------------------------------------------
        /* 
		* Initialize the identification of the network object
		*/
        public void Initialize()
		{
			if (m_networkID == null)
			{
				if (this.gameObject.GetComponent<NetworkID>() == null)
				{
					this.gameObject.AddComponent<NetworkID>();
				}
				m_networkID = this.gameObject.GetComponent<NetworkID>();
#if !DISABLE_UNET_COMMS
				m_networkID.NetID = this.gameObject.GetComponent<NetworkWorldObjectData>().NetID;
				m_networkID.UID = this.gameObject.GetComponent<NetworkWorldObjectData>().UID;
#endif
			}
		}

        // -------------------------------------------
        /* 
		 * Check it the actor belongs to the current player
		 */
        public bool IsMine()
        {
            if (MultiplayerConfiguration.LoadNumberOfPlayers() != 1)
            {
                return (YourNetworkTools.Instance.GetUniversalNetworkID() == NetworkID.NetID);
            }
            else
            {
                return true;
            }
        }

		// -------------------------------------------
		/* 
		 * Will dispatch an event that will destroy the object in all the network
		 */
		void OnDestroy()
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("[ActorNetwork] ++SEND++ SIGNAL FOR AUTODESTRUCTION");
#endif
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;

            if (MultiplayerConfiguration.LoadNumberOfPlayers() != 1)
            {
                NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST, NetworkID.NetID.ToString(), NetworkID.UID.ToString());
            }
        }

		// -------------------------------------------
		/* 
		 * SetSinglePlayerNetworkID
		 */
		public void SetSinglePlayerNetworkID(int _NetID, int _UID)
        {
			m_networkID = this.gameObject.GetComponent<NetworkID>();
			m_networkID.SetID(_NetID, _UID);
		}

		// -------------------------------------------
		/* 
		 * OnNetworkEvent
		 */
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST)
			{
				if ((NetworkID.NetID == int.Parse((string)_list[0]))
					&& (NetworkID.UID == int.Parse((string)_list[1])))
				{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("[ActorNetwork] --RECEIVE-- SIGNAL FOR AUTODESTRUCTION");
#endif
					GameObject.Destroy(this.gameObject);
				}
			}
		}
	}
}