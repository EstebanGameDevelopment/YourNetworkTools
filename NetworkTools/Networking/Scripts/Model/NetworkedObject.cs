namespace YourNetworkingTools
{
    using System;
    /******************************************
    * 
    * NetworkID
    * 
    * Unique identificator for network
    * 
    * @author Esteban Gallardo
    */
    using UnityEngine;
    using YourCommonTools;

    public class NetworkedObject : MonoBehaviour
	{
        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------
        public const string EVENT_NETWORKED_OBJECT_UPDATE = "EVENT_NETWORKED_OBJECT_UPDATE";
        public const string EVENT_NETWORKED_OBJECT_DESTROY = "EVENT_NETWORKED_OBJECT_DESTROY";
        public const string EVENT_NETWORKED_REQUEST_EXISTANCE = "EVENT_NETWORKED_REQUEST_EXISTANCE";
        public const string EVENT_NETWORKED_RESPONSE_EXISTANCE = "EVENT_NETWORKED_RESPONSE_EXISTANCE";
        public const string EVENT_NETWORKED_UPDATE_NETID = "EVENT_NETWORKED_UPDATE_NETID";
        public const string EVENT_NETWORKED_OBJECT_RESET_TO_INITIAL = "EVENT_NETWORKED_OBJECT_RESET_TO_INITIAL";
        public const string EVENT_NETWORKED_ACTIVATE_UPDATE_TRANFORM = "EVENT_NETWORKED_ACTIVATE_UPDATE_TRANFORM";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
        public string Name = "";
        public string VisualsName = "";
        public string Params = "";
        public int NetIDOwner = -1;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
        private float m_timeOut = 0;
        private Vector3 m_initialPosition;
        private Vector3 m_initialScale;
        private Quaternion m_initialRotation;
        private float m_timeoutReset = -1;
        private bool m_remoteUpdate = false;
        private long m_releasedTimestamp = -1;

        public long ReleasedTimestamp
        {
            get { return m_releasedTimestamp; }
            set { m_releasedTimestamp = value; }
        }

        // -------------------------------------------
        /* 
		* Initialize
		*/
        public void Initialize(bool _requestExistance = true)
        {
            NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);

            m_initialPosition = Utilities.Clone(this.transform.position);
            m_initialScale = Utilities.Clone(this.transform.localScale);
            m_initialRotation = Utilities.Clone(this.transform.rotation);

            if ((NetIDOwner == -1) && _requestExistance)
            {
                NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_NETWORKED_REQUEST_EXISTANCE, 0.1f, Name);
            }
        }

        // -------------------------------------------
        /* 
		* OnDestroy
		*/
        private void OnDestroy()
        {
            NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
        }

        // -------------------------------------------
        /* 
		* IsOwner
		*/
        public bool IsOwner()
        {
            return (YourNetworkTools.Instance.GetUniversalNetworkID() == NetIDOwner);
        }

        // -------------------------------------------
        /* 
		* OwnNetworkObject
		*/
        public void OwnNetworkObject(int _networkIDNewOwner = -1, bool _reportToNetwork = true)
        {
            if (_networkIDNewOwner == -1)
            {
                NetIDOwner = YourNetworkTools.Instance.GetUniversalNetworkID();
            }
            else
            {
                NetIDOwner = _networkIDNewOwner;
            }
            if (_reportToNetwork)
            {
                NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_NETWORKED_UPDATE_NETID, 0.05f, Name, NetIDOwner.ToString());
            }            
        }

        // -------------------------------------------
        /* 
		* OwnNetworkObject
		*/
        public void ActivateNetworkUpdateTransform(bool _enabled, float _delay = 0.05f)
        {
            NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_NETWORKED_ACTIVATE_UPDATE_TRANFORM, _delay, Name, _enabled.ToString());
        }

        // -------------------------------------------
        /* 
        * Awake
        */
        private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, object[] _list)
        {
            if (_nameEvent == EVENT_NETWORKED_ACTIVATE_UPDATE_TRANFORM)
            {
                string recvName = (string)_list[0];
                if (Name == recvName)
                {
                    m_remoteUpdate = bool.Parse((string)_list[1]);
                    if (m_remoteUpdate)
                    {
                        NetworkEventController.Instance.ClearNetworkEvents(EVENT_NETWORKED_ACTIVATE_UPDATE_TRANFORM);
                    }                    
                }
            }
            if (_nameEvent == EVENT_NETWORKED_OBJECT_RESET_TO_INITIAL)
            {
                if (IsOwner())
                {
                    m_timeoutReset = 1;
                }
            }
            if (_nameEvent == EVENT_NETWORKED_UPDATE_NETID)
            {
                string recvName = (string)_list[0];
                if (Name == recvName)
                {
                    NetIDOwner = int.Parse((string)_list[1]);
                }
            }
            if (_nameEvent == EVENT_NETWORKED_OBJECT_DESTROY)
            {
                string recvName = (string)_list[0];
                if (recvName == Name)
                {
                    GameObject.Destroy(this.gameObject);
                }
            }
            if (_nameEvent == EVENT_NETWORKED_RESPONSE_EXISTANCE)
            {
                string recvName = (string)_list[0];
                bool isExisting = bool.Parse((string)_list[1]);
                if (Name == recvName)
                {
                    if (NetIDOwner == -1)
                    {
                        if (isExisting)
                        {
                            NetIDOwner = int.Parse((string)_list[2]);
                        }
                        else
                        {
                            NetIDOwner = YourNetworkTools.Instance.GetUniversalNetworkID();
                        }
                    }
                }
            }
            if (YourNetworkTools.Instance.GetUniversalNetworkID() != _networkOriginID)
            {
                if (_nameEvent == EVENT_NETWORKED_OBJECT_UPDATE)
                {
                    string recvName = (string)_list[0];
                    // Debug.LogError("LOCAL NAME[" + Name + "] RECEIVED NAME["+ recvName + "]");
                    // Debug.LogError("RECEIVED INFO[" + (string)_list[2] + "]::[" + (string)_list[3] + "][" + (string)_list[4] + "][" + (string)_list[5] + "]");
                    if (Name == recvName)
                    {
                        long timestamp = long.Parse((string)_list[7]);
                        bool applyUpdate = false;
                        if (m_releasedTimestamp == -1)
                        {
                            applyUpdate = true;
                        }
                        else
                        {
                            if (m_releasedTimestamp < timestamp)
                            {
                                applyUpdate = true;
                            }
                        }

                        if (applyUpdate)
                        {
                            Vector3 sposition = Utilities.StringToVector3((string)_list[3]);
                            Quaternion srotation = Utilities.StringToQuaternion((string)_list[4]);
                            InterpolatorController.Instance.InterpolatePosition(this.gameObject, sposition, YourNetworkTools.Instance.TimeToUpdateNetworkedObjects, false);
                            InterpolatorController.Instance.InterpolateRotation(this.gameObject, srotation, YourNetworkTools.Instance.TimeToUpdateNetworkedObjects, false);
                            this.transform.localScale = Utilities.StringToVector3((string)_list[5]);
                            this.transform.gameObject.SetActive(bool.Parse((string)_list[6]));
                        }
                    }
                }
               
            }
        }

        // -------------------------------------------
        /* 
		* ActivationPhysics
		*/
        public void ActivationPhysics(bool _activation)
        {
            if (this.gameObject.GetComponent<Rigidbody>()!=null) this.gameObject.GetComponent<Rigidbody>().useGravity = _activation;
            if (this.gameObject.GetComponent<Rigidbody>()!=null) this.gameObject.GetComponent<Rigidbody>().isKinematic = !_activation;
            if (this.gameObject.GetComponent<BoxCollider>()!=null) this.gameObject.GetComponent<BoxCollider>().isTrigger = !_activation;
        }

        // -------------------------------------------
        /* 
		* Update
		*/
        private void Update()
        {
            if (NetIDOwner != -1)
            {
                if (NetIDOwner == YourNetworkTools.Instance.GetUniversalNetworkID())
                {
                    if (m_timeoutReset > 0)
                    {
                        m_timeoutReset -= Time.deltaTime;
                        if (m_timeoutReset <= 0)
                        {
                            ActivationPhysics(true);
                        }
                        else
                        {
                            ActivationPhysics(false);
                            this.transform.position = m_initialPosition;
                            this.transform.localScale = m_initialScale;
                            this.transform.rotation = m_initialRotation;
                        }
                    }                    

                    if (m_remoteUpdate)
                    {
                        m_timeOut += Time.deltaTime;
                        if (m_timeOut >= YourNetworkTools.Instance.TimeToUpdateNetworkedObjects)
                        {
                            m_timeOut = 0;
                            string sposition = Utilities.Vector3ToString(this.transform.position);
                            string srotation = Utilities.QuaternionToString(this.transform.rotation);
                            string sscale = Utilities.Vector3ToString(this.transform.localScale);
                            // Debug.LogError("SENDING INFO[" + Name + "]::[" + sposition + "][" + srotation + "][" + sscale + "]");
                            NetworkEventController.Instance.DispatchNetworkEvent(EVENT_NETWORKED_OBJECT_UPDATE, Name, VisualsName, Params, sposition, srotation, sscale, this.transform.gameObject.activeSelf.ToString(), Utilities.GetTimestamp().ToString());
                        }
                    }
                }
                else
                {
                    ActivationPhysics(false);
                }
            }
        }
    }
}