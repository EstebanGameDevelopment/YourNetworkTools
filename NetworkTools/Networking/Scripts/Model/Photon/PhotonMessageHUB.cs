using UnityEngine;
#if ENABLE_PHOTON
using Photon.Pun;
#endif

namespace YourNetworkingTools
{
    /******************************************
	 * 
	 * PhotonMessage
	 * 
	 * @author Esteban Gallardo
	 */
    public class PhotonMessageHUB : MonoBehaviour
    {
#if ENABLE_PHOTON
        public const bool DEBUG = false;

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static PhotonMessageHUB _instance;

        public static PhotonMessageHUB Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(PhotonMessageHUB)) as PhotonMessageHUB;
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        public const char PARAMS_SEPARATOR = '~';
        private PhotonView m_photonView;

        // -------------------------------------------
        /* 
		 * Awake
		 */
        public void Awake()
        {
            m_photonView = GetComponent<PhotonView>();
            DontDestroyOnLoad(this.gameObject);
        }

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }

        // -------------------------------------------
        /* 
		 * NetworkMessage
		 */
        [PunRPC]
        public void NetworkMessage(string _nameEvent, int _originNetworkID, int _targetNetworkID, string _data, PhotonMessageInfo info)
        {
            string[] eventParams = _data.Split(PARAMS_SEPARATOR);
            if (DEBUG) Debug.LogError("PhotonMessageHUB::NetworkMessage["+ _nameEvent + "]::data["+_data+"]");
            if (_originNetworkID == PhotonController.Instance.UniqueNetworkID)
            {
                // Debug.LogError("ClientTCPEventsController::EVENT[" + _nameEvent + "] IGNORED BECAUSE IT CAME FROM THIS ORIGIN");
                return;
            }
            NetworkEventController.Instance.DispatchCustomNetworkEvent(_nameEvent, true, _originNetworkID, _targetNetworkID, eventParams);
        }

        // -------------------------------------------
        /* 
		 * PrepareMessage
		 */
        public void PrepareMessage(string _nameEvent, int _originNetworkID, int _targetNetworkID, params string[] _list)
        {
            string data = "";
            for (int i = 0; i < _list.Length; i++)
            {
                data += _list[i];
                if (i < _list.Length - 1)
                {
                    data += PARAMS_SEPARATOR;
                }
            }
            if (DEBUG) Debug.LogError("PhotonMessageHUB::PrepareMessage[" + _nameEvent + "]::data[" + data + "]");
            m_photonView.RPC("NetworkMessage", RpcTarget.AllViaServer, _nameEvent, _originNetworkID, _targetNetworkID, data);
        }
#endif
    }
}
