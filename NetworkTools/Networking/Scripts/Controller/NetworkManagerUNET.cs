﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;
#if ENABLE_MIRROR
using Mirror;
#else
using UnityEngine.Networking;
#endif
using UnityEngine.SceneManagement;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * NetworkManagerUNET
	 * 
	 * Custom network manager
	 * 
	 * @author Esteban Gallardo
	 */
	public class NetworkManagerUNET :
#if !DISABLE_UNET_COMMS && !ENABLE_MIRROR
        NetworkManager
#else
        MonoBehaviour
#endif
    {
#if !DISABLE_UNET_COMMS && !ENABLE_MIRROR
        // -------------------------------------------
        /* 
		 * Process the disconnection
		 */
        public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);
			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS);
		}
#endif
    }
}