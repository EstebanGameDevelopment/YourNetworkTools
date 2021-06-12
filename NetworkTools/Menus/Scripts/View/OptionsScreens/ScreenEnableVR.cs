using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YourCommonTools;
using YourNetworkingTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenEnableVR
	 * 
	 * Enable VR or gyroscope mode
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenEnableVR : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_ENABLE_VR";

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string SUB_EVENT_ENABLEVR_CONFIRMATION_DOWNLOAD_ASSETBUNDLE = "SUB_EVENT_ENABLEVR_CONFIRMATION_DOWNLOAD_ASSETBUNDLE";
        
        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
		protected Transform m_container;

		// -------------------------------------------
		/* 
		* Constructor
		*/
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");

			GameObject playInVRGame = m_container.Find("Button_EnableVR").gameObject;
			playInVRGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.as.vr.game");
			playInVRGame.GetComponent<Button>().onClick.AddListener(PlayInVRPressed);

			GameObject playWithGyroscopeGame = m_container.Find("Button_EnableGyroscope").gameObject;
			playWithGyroscopeGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.with.gyroscope");
			playWithGyroscopeGame.GetComponent<Button>().onClick.AddListener(PlayWithGyroscopePressed);

            UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);
		}

		// -------------------------------------------
		/* 
		* GetGameObject
		*/
		public GameObject GetGameObject()
		{
			return this.gameObject;
		}

		// -------------------------------------------
		/* 
		* Destroy
		*/
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnMenuEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		* PlayInVRPressed
		*/
		private void PlayInVRPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            CardboardLoaderVR.Instance.SaveEnableCardboard(true);
#if ENABLE_GOOGLE_ARCORE
            if (!MenuScreenController.Instance.AskToEnableBackgroundARCore || (MultiplayerConfiguration.LoadGoogleARCore(-1) != MultiplayerConfiguration.GOOGLE_ARCORE_ENABLED))
            {
                FinalLoadGameWithAssets();
            }
            else
            {
                if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenEnableBackground.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, m_nameOfScreen);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEnableBackground.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, m_nameOfScreen);
                }
            }
#else
            if (MenuScreenController.Instance.RequestPermissionAssetBundleDownload)
            {
                RequestDownloadAssetBundle();
            }
            else
            {
                FinalLoadGameWithAssets();
            }
#endif
        }

        // -------------------------------------------
        /* 
		* JoinGamePressed
		*/
        private void PlayWithGyroscopePressed()
		{
            SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            CardboardLoaderVR.Instance.SaveEnableCardboard(false);
#if ENABLE_GOOGLE_ARCORE
            if (!MenuScreenController.Instance.AskToEnableBackgroundARCore || (MultiplayerConfiguration.LoadGoogleARCore(-1) != MultiplayerConfiguration.GOOGLE_ARCORE_ENABLED))
            {
                FinalLoadGameWithAssets();
            }
            else
            {
                if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenEnableBackground.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, m_nameOfScreen);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEnableBackground.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, m_nameOfScreen);
                }
            }
#else
            if (MenuScreenController.Instance.RequestPermissionAssetBundleDownload)
            {
                RequestDownloadAssetBundle();
            }
            else
            {
                FinalLoadGameWithAssets();
            }
#endif
        }


        // -------------------------------------------
        /* 
		* FinalLoadGameWithAssets
		*/
        private void FinalLoadGameWithAssets()
        {
            if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
            }
            else
            {
                MenuScreenController.Instance.LoadGameScene(this);
            }
        }

        // -------------------------------------------
        /* 
		* RequestDownloadAssetBundle
		*/
        private void RequestDownloadAssetBundle()
        {
            if (!MenuScreenController.Instance.RequestPermissionAssetBundleDownload)
            {
                FinalLoadGameWithAssets();
            }
            else
            {
                if (AssetbundleController.Instance.CheckAssetsCached())
                {
                    FinalLoadGameWithAssets();
                }
                else
                {
                    if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                    {
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_INFORMATION_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.need.download.assetbundle"), null, SUB_EVENT_ENABLEVR_CONFIRMATION_DOWNLOAD_ASSETBUNDLE);
                    }
                    else
                    {
                        UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.need.download.assetbundle"), null, SUB_EVENT_ENABLEVR_CONFIRMATION_DOWNLOAD_ASSETBUNDLE);
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		* OnMenuBasicEvent
		*/
        protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

            if (_nameEvent == MenuScreenController.EVENT_CONFIRMATION_POPUP)
            {
                string subEvent = (string)_list[2];
                if (subEvent == SUB_EVENT_ENABLEVR_CONFIRMATION_DOWNLOAD_ASSETBUNDLE)
                {
                    if ((bool)_list[1])
                    {
                        FinalLoadGameWithAssets();
                    }
                    else
                    {
                        if (MenuScreenController.Instance.AlphaAnimationNameStack != -1)
                        {
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_INFORMATION_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, MenuScreenController.Instance.AlphaAnimationNameStack }, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.need.no.download.permitted"), null, "");
                        }
                        else
                        {
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.need.no.download.permitted"), null, "");
                        }
                    }
                }
            }
        }
    }
}