using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_USER_SERVER
using UserManagement;
#endif
using YourCommonTools;
using YourNetworkingTools;

namespace YourNetworkingTools
{
    /******************************************
	 * 
	 * ScreenAppMainUserView
	 * 
	 * Initial screen where we will be able to
     * select between register a new user or load an existing
     * user
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenAppMainUserView : ScreenBaseView, IBasicView
    {
        public const string SCREEN_NAME = "SCREEN_MAIN_USER";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        public const string SUB_EVENT_MAINUSER_CONFIRMATION_LOGOUT = "SUB_EVENT_MAINUSER_CONFIRMATION_LOGOUT";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
        private Transform m_container;

        private GameObject m_loginUserGO;
        private GameObject m_editProfileGO;
        private GameObject m_logoutProfileGO;

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
        {
            base.Initialize(_list);

            m_root = this.gameObject;
            m_container = m_root.transform.Find("Content");

#if !ALTERNATIVE_TITLE
            m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");
#else
			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.mobile.title");
#endif

            if (m_container.Find("Login_User") != null)
            {
                m_loginUserGO = m_container.Find("Login_User").gameObject;
                m_loginUserGO.GetComponent<Button>().onClick.AddListener(OnLoginUser);
                m_loginUserGO.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.user.option.login");
            }

            if (m_container.Find("Edit_Profile") != null)
            {
                m_editProfileGO = m_container.Find("Edit_Profile").gameObject;
                m_editProfileGO.GetComponent<Button>().onClick.AddListener(OnEditProfileUser);
                m_editProfileGO.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.user.edit.profile.login");
            }

            if (m_container.Find("Logout_User") != null)
            {
                m_logoutProfileGO = m_container.Find("Logout_User").gameObject;
                m_logoutProfileGO.GetComponent<Button>().onClick.AddListener(OnLogoutUser);
                m_logoutProfileGO.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.user.option.logout");
            }

            RefreshData();

            UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
        }

        // -------------------------------------------
        /* 
		 * OnLogoutUser
		 */
        private void RefreshData()
        {
#if ENABLE_USER_SERVER
            if (UsersController.Instance.CurrentUser.Email.Length == 0)
            {
                m_container.transform.Find("CurrentUser").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.no.current.user");
                m_loginUserGO.SetActive(true);
                m_editProfileGO.SetActive(false);
                m_logoutProfileGO.SetActive(false);
            }
            else
            {
                m_container.transform.Find("CurrentUser").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.current.user") + " " + UsersController.Instance.CurrentUser.Email;
                m_loginUserGO.SetActive(false);
                m_editProfileGO.SetActive(true);
                m_logoutProfileGO.SetActive(true);
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * OnLogoutUser
		 */
        private void OnLogoutUser()
        {
            SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.do.you.want.exit"), null, SUB_EVENT_MAINUSER_CONFIRMATION_LOGOUT);
        }

        // -------------------------------------------
        /* 
		 * OnLoginUser
		 */
        private void OnEditProfileUser()
        {
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
#if ENABLE_USER_SERVER
            UIEventController.Instance.DelayUIEvent(UsersController.EVENT_USER_CALL_CONSULT_SINGLE_RECORD, 0.2f, UsersController.Instance.CurrentUser.Id);
#endif
        }

        // -------------------------------------------
        /* 
		 * OnLoginUser
		 */
        private void OnLoginUser()
        {
#if ENABLE_USER_SERVER
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoginUserView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#endif
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
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

            return false;
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
                if (subEvent == SUB_EVENT_MAINUSER_CONFIRMATION_LOGOUT)
                {
                    if ((bool)_list[1])
                    {
#if ENABLE_USER_SERVER
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(UsersController.EVENT_USER_RESET_LOCAL_DATA);
#endif
                        RefreshData();
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
#if ENABLE_USER_SERVER
            if (_nameEvent == UsersController.EVENT_USER_RESULT_FORMATTED_SINGLE_RECORD)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_WAIT);
                if ((_list == null) || (_list.Length == 0))
                {
                    string titleInfoError = LanguageController.Instance.GetText("message.error");
                    string descriptionInfoError = LanguageController.Instance.GetText("screen.list.user.error.retrieving");
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, titleInfoError, descriptionInfoError, null, "");
                    return;
                }

                if (ScreenController.InstanceBase.AlphaAnimationNameStack != -1)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, ScreenController.InstanceBase.AlphaAnimationNameStack }, ScreenProfileView.SCREEN_NAME_DISPLAY, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, (UserModel)_list[0]);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, null, ScreenProfileView.SCREEN_NAME_DISPLAY, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, (UserModel)_list[0]);
                }
            }
#endif
        }
    }
}
