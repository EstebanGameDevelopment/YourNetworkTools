using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourCommonTools;
#if ENABLE_YOURVRUI
using YourVRUI;
#endif

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * MenuScreenController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal
	 * 
	 * @author Esteban Gallardo
	 */
	public class MenuScreenController : FunctionsScreenController
    {
        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static MenuScreenController instance;

		public static MenuScreenController Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(MenuScreenController)) as MenuScreenController;
				}
				return instance;
			}
		}
    }
}