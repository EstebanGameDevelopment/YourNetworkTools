﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{
    /******************************************
     * 
     * GrabNetworkObject
     * 
     * @author Esteban Gallardo
     */
    public class GrabNetworkObject : MonoBehaviour
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_GRABOBJECT_REQUEST_RAYCASTING = "EVENT_GRABOBJECT_REQUEST_RAYCASTING";
        public const string EVENT_GRABOBJECT_RESPONSE_RAYCASTING = "EVENT_GRABOBJECT_RESPONSE_RAYCASTING";

        public const string EVENT_GRABOBJECT_TAKE_OBJECT = "EVENT_GRABOBJECT_TAKE_OBJECT";
        public const string EVENT_GRABOBJECT_RELEASE_OBJECT = "EVENT_GRABOBJECT_RELEASE_OBJECT";
        public const string EVENT_GRABOBJECT_UPDATE_OBJECT = "EVENT_GRABOBJECT_UPDATE_OBJECT";
        public const string EVENT_GRABOBJECT_DISABLE_OBJECT = "EVENT_GRABOBJECT_DISABLE_OBJECT";

        // ----------------------------------------------
        // PUBLIC CONSTANTS
        // ----------------------------------------------	
        public const float TIMEOUT_TO_UPDATE = 0.1f;

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public bool Enabled = true;
        public bool InterpolateOrientation = true;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_target;
        private GameObject m_referencePosition;
        private GameObject m_referenceForward;
        private float m_timeoutTransform = 0;
        private float m_timeoutToIgnore = 0;

        private bool m_frozen = false;

        // -------------------------------------------
        /* 
		* Start
		*/
        public void Start()
        {
            m_referencePosition = new GameObject();
            m_referencePosition.transform.parent = this.gameObject.transform.parent;
            m_referencePosition.name = "refpos_" + this.gameObject.name;

            m_referenceForward = new GameObject();
            m_referenceForward.transform.parent = this.gameObject.transform.parent;
            m_referenceForward.name = "reffwd_" + this.gameObject.name;

            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
            NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
        }

        // -------------------------------------------
        /* 
		* OnDestroy
		*/
        public void OnDestroy()
        {
            UIEventController.Instance.UIEvent -= OnUIEvent;
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
            NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
        }

        // -------------------------------------------
        /* 
		* ActivationPhysics
		*/
        public void ActivationPhysics(bool _activation)
        {
            this.gameObject.GetComponent<Rigidbody>().useGravity = _activation;
            this.gameObject.GetComponent<Rigidbody>().isKinematic = !_activation;
            this.gameObject.GetComponent<BoxCollider>().isTrigger = !_activation;
        }

        // -------------------------------------------
        /* 
		* DisableGrabNetwork
		*/
        public void DisableGrabNetwork()
        {
            if (!m_frozen)
            {
                NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_GRABOBJECT_DISABLE_OBJECT, 0.01f, this.gameObject.name);
            }            
        }

        // -------------------------------------------
        /* 
		* OnUIEvent
		*/
        private void OnUIEvent(string _nameEvent, object[] _list)
        {
            if (m_frozen)
            {
                return;
            }
            if (m_timeoutToIgnore > 0)
            {
                return;
            }

            if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
            {
                if (Enabled)
                {
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_GRABOBJECT_REQUEST_RAYCASTING, this.gameObject);
                }
            }
            if (_nameEvent == KeysEventInputController.ACTION_BUTTON_UP)
            {
                if (Enabled)
                {
                    if (m_target != null)
                    {
                        m_target = null;
                        ActivationPhysics(true);
                        m_timeoutToIgnore = 0.5f;
                        NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_GRABOBJECT_RELEASE_OBJECT, 0.01f, this.gameObject.name, YourNetworkTools.Instance.GetUniversalNetworkID().ToString(), Utilities.GetTimestamp().ToString());
                        // NetworkEventController.Instance.DispatchNetworkEvent(EVENT_GRABOBJECT_RELEASE_OBJECT, this.gameObject.name, YourNetworkTools.Instance.GetUniversalNetworkID().ToString());
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		* OnBasicSystemEvent
		*/
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (m_frozen) return;

            if (_nameEvent == EVENT_GRABOBJECT_RESPONSE_RAYCASTING)
            {
                GameObject collidedObject = (GameObject)_list[0];
                if (this.gameObject == collidedObject)
                {
                    m_target = (GameObject)_list[1];
                    ActivationPhysics(false);
                    NetworkEventController.Instance.PriorityDelayNetworkEvent(EVENT_GRABOBJECT_TAKE_OBJECT, 0.01f, this.gameObject.name, YourNetworkTools.Instance.GetUniversalNetworkID().ToString());
                    // NetworkEventController.Instance.DispatchNetworkEvent(EVENT_GRABOBJECT_TAKE_OBJECT, this.gameObject.name, YourNetworkTools.Instance.GetUniversalNetworkID().ToString());
                }
            }
        }

        // -------------------------------------------
        /* 
		* OnNetworkEvent
		*/
        private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, object[] _list)
        {
            if (_nameEvent == EVENT_GRABOBJECT_TAKE_OBJECT)
            {
                if (this.gameObject.name == (string)_list[0])
                {
                    int networkIDOwner = int.Parse((string)_list[1]);
                    this.gameObject.GetComponent<NetworkedObject>().ReleasedTimestamp = -1;
                    if (YourNetworkTools.Instance.GetUniversalNetworkID() != networkIDOwner)
                    {
                        Enabled = false;
                        ActivationPhysics(false);
                    }
                    if (this.gameObject.GetComponent<NetworkedObject>() != null)
                    {
                        this.gameObject.GetComponent<NetworkedObject>().OwnNetworkObject(networkIDOwner);
                        this.gameObject.GetComponent<NetworkedObject>().ActivateNetworkUpdateTransform(true);
                    }
                }
            }
            if (_nameEvent == EVENT_GRABOBJECT_RELEASE_OBJECT)
            {
                if (this.gameObject.name == (string)_list[0])
                {
                    m_timeoutToIgnore = 0.5f;
                    Enabled = true;
                    ActivationPhysics(true);
                    this.gameObject.GetComponent<NetworkedObject>().ReleasedTimestamp = long.Parse((string)_list[2]);
                    if (this.gameObject.GetComponent<NetworkedObject>().NetIDOwner == YourNetworkTools.Instance.GetUniversalNetworkID())
                    {
                        this.gameObject.GetComponent<NetworkedObject>().ActivateNetworkUpdateTransform(false, 5);
                    }                    
                }
            }
            if (_nameEvent == EVENT_GRABOBJECT_UPDATE_OBJECT)
            {
                if (this.gameObject.name == (string)_list[0])
                {
                    if (this.gameObject.GetComponent<NetworkedObject>() != null)
                    {
                        if (this.gameObject.GetComponent<NetworkedObject>().IsOwner())
                        {
                            Vector3 newPos = Utilities.StringToVector3((string)_list[1]);
                            InterpolatorController.Instance.InterpolatePosition(m_referencePosition, newPos, TIMEOUT_TO_UPDATE);
                            if (InterpolateOrientation)
                            {
                                Vector3 newForward = Utilities.StringToVector3((string)_list[2]);
                                InterpolatorController.Instance.InterpolateForward(m_referenceForward, newForward, TIMEOUT_TO_UPDATE);
                            }
                        }
                    }
                }
            }
            if (_nameEvent == EVENT_GRABOBJECT_DISABLE_OBJECT)
            {
                if (this.gameObject.name == (string)_list[0])
                {
                    m_frozen = true;
                }
            }
        }

        // -------------------------------------------
        /* 
		* Update
		*/
        private void Update()
        {
            if (Enabled)
            {
                if (m_timeoutToIgnore > 0)
                {
                    m_timeoutToIgnore -= Time.deltaTime;
                }

                if (m_target != null)
                {
                    if (this.gameObject.GetComponent<NetworkedObject>() != null)
                    {
                        if (this.gameObject.GetComponent<NetworkedObject>().IsOwner())
                        {
                            this.gameObject.transform.position = m_target.transform.position + m_target.transform.forward.normalized;
                            if (InterpolateOrientation) this.gameObject.transform.forward = m_target.transform.forward;
                        }
                    }
                }
            }
            else
            {
                if (this.gameObject.GetComponent<NetworkedObject>().IsOwner())
                {
                    this.gameObject.transform.position = m_referencePosition.transform.position;
                    if (InterpolateOrientation) this.gameObject.transform.forward = m_target.transform.forward;
                }
            }
        }
    }
}