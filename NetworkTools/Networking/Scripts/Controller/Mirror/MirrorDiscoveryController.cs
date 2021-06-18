// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;
#if ENABLE_MIRROR
using Mirror;
using Mirror.Discovery;
#endif
using UnityEngine.SceneManagement;
using YourCommonTools;
using System.Collections.Generic;
using System;

namespace YourNetworkingTools
{

    /******************************************
	 * 
	 * MirrorDiscoveryController
	 * 
	 * @author Esteban Gallardo
	 */
#if ENABLE_MIRROR
    [RequireComponent(typeof(NetworkDiscovery))]
#endif
    public class MirrorDiscoveryController :
#if ENABLE_MIRROR
        NetworkManager
#else
        MonoBehaviour
#endif
    {

        public const string EVENT_MIRRORDISCOVERYCONTROLLER_TIMEOUT_DISCOVERY = "EVENT_MIRRORDISCOVERYCONTROLLER_TIMEOUT_DISCOVERY";

#if ENABLE_MIRROR
        private NetworkDiscovery m_networkDiscovery;
        private bool m_discovering = false;

        // -------------------------------------------
        /* 
		 * Start
		 */
        public override void Start()
        {
            base.Start();

            if (m_networkDiscovery == null)
            {
                m_networkDiscovery = GetComponent<NetworkDiscovery>();
                m_networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
            }

            m_discovering = true;
            m_networkDiscovery.StartDiscovery();

            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);

            BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_MIRRORDISCOVERYCONTROLLER_TIMEOUT_DISCOVERY, 5);

            Debug.LogError("%%%%%%%%%% MirrorDiscoveryController::START SEARCHING FOR A SERVER...");
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        public override void OnDestroy()
        {
            base.OnDestroy();

            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;

            if (CommunicationsController.Instance.IsServer)
            {
                NetworkServer.Shutdown();
            }
            else
            {
                NetworkClient.Shutdown();
            }

            m_networkDiscovery.OnServerFound.RemoveListener(OnDiscoveredServer);
            m_networkDiscovery = null;
        }

        // -------------------------------------------
        /* 
		 * OnDiscoveredServer
		 */
        public void OnDiscoveredServer(ServerResponse info)
        {
            if (m_discovering)
            {
                m_discovering = false;
                BasicSystemEventController.Instance.ClearBasicSystemEvents(EVENT_MIRRORDISCOVERYCONTROLLER_TIMEOUT_DISCOVERY);
                CommunicationsController.Instance.IsServer = false;
                StartClient(info.uri);
                Debug.LogError("%%%%%%%%%% MirrorDiscoveryController::STARTED AS A CLIENT (MIRROR) CONNECTED TO SERVER[" + info.EndPoint.Address.ToString() + "].");
            }
        }

        // -------------------------------------------
        /* 
		 * OnBasicSystemEvent
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_MIRRORDISCOVERYCONTROLLER_TIMEOUT_DISCOVERY)
            {
                if (m_discovering)
                {
                    m_discovering = false;
                    m_networkDiscovery.StopDiscovery();

                    CommunicationsController.Instance.IsServer = true;
                    StartHost();
                    m_networkDiscovery.AdvertiseServer();
                    NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REGISTER_ALL_NETWORK_PREFABS);
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(CommunicationsController.EVENT_COMMSCONTROLLER_SET_UP_IS_SERVER);

                    Debug.LogError("%%%%%%%%%% MirrorDiscoveryController::STARTED AS A SERVER (MIRROR).");
                }
            }
        }
#endif
    }
}