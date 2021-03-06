using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourNetworkingTools
{
    /******************************************
	 * 
	 * MultiplayerConfiguration
	 * 
	 * We keep the global information 
	 * 
	 * @author Esteban Gallardo
	 */
    public static class MultiplayerConfiguration
    {
        public const bool DEBUG_MODE = true;

        public const string NUMBER_OF_LEVELS_COOCKIE = "NUMBER_OF_LEVELS_COOCKIE";

        public const string NUMBER_OF_PLAYERS_COOCKIE = "NUMBER_OF_PLAYERS_COOCKIE";
        public const int VALUE_FOR_JOINING = -1000;
        public const int ROOM_NUMBER_TO_JOIN_THE_LAST_CREATED_ROOM = 100000000;

        public const string SOCKET_SERVER_ADDRESS = "localhost";
        public const int PORT_SERVER_ADDRESS = 8745;

        public const string BALANCE_LOADER_CREATE_NEW_ROOM = "http://localhost:8080/yournetworkingtools/CreateNewRoomHTTP.php";
        public const string BALANCE_LOADER_GET_LIST_ROOMS = "http://localhost:8080/yournetworkingtools/GetListRoomsHTTP.php";

        public const string IP_ADDRESS_COOCKIE = "IP_ADDRESS_COOCKIE";
        public const string PORT_ADDRESS_COOCKIE = "PORT_ADDRESS_COOCKIE";
        public const string CREATE_ROOM_COOCKIE = "CREATE_ROOM_COOCKIE";
        public const string LIST_ROOMS_COOCKIE = "LIST_ROOMS_COOCKIE";
        public const string ROOM_NUMBER_COOCKIE = "ROOM_NUMBER_COOCKIE";
        public const string ROOM_NAME_COOCKIE = "ROOM_NAME_COOCKIE";
        public const string MACHINE_ID_HOST_ROOM_COOCKIE = "MACHINE_ID_HOST_ROOM_COOCKIE";

        public const string FACEBOOK_FRIENDS_COOCKIE = "FACEBOOK_FRIENDS_COOCKIE";
        public const string NAME_ROOM_LOOBY_COOCKIE = "NAME_ROOM_LOOBY_COOCKIE";
        public const string IS_ROOM_LOOBY_COOCKIE = "IS_ROOM_LOOBY_COOCKIE";

        public const string EXTRA_DATA_COOCKIE = "EXTRA_DATA_COOCKIE";

        public const string GOOGLE_ARCORE_COOCKIE = "GOOGLE_ARCORE_COOCKIE";
        public const string AR_ENABLE_BACKGROUND = "AR_ENABLE_BACKGROUND";

        public const string DIRECTOR_MODE_COOCKIE = "DIRECTOR_MODE_COOCKIE";
        public const string SPECTATOR_MODE_COOCKIE = "SPECTATOR_MODE_COOCKIE";

        public const string BUFFER_SIZE_SEND_COOCKIE = "BUFFER_SIZE_SEND_COOCKIE";
        public const string BUFFER_SIZE_RECEIVE_COOCKIE = "BUFFER_SIZE_RECEIVE_COOCKIE";
        public const string TIMEOUT_SEND_COOCKIE = "TIMEOUT_SEND_COOCKIE";
        public const string TIMEOUT_RECEIVE_COOCKIE = "TIMEOUT_RECEIVE_COOCKIE";

        public const string CHARACTER_SELECTED_COOCKIE = "CHARACTER_SELECTED_COOCKIE";
        public const string LEVEL_SELECTED_COOCKIE = "LEVEL_SELECTED_COOCKIE";

        public const string HUMAN_NAME = "HUMAN_";
        public const string DIRECTOR_NAME = "HUMAN_DIRECTOR_";        

        public const int GOOGLE_ARCORE_DISABLED = 0;
        public const int GOOGLE_ARCORE_ENABLED = 1;

        public const int DIRECTOR_MODE_DISABLED = 0;
        public const int DIRECTOR_MODE_ENABLED = 1;

        public const int SPECTATOR_MODE_DISABLED = 0;
        public const int SPECTATOR_MODE_ENABLED = 1;

        // -------------------------------------------
        /* 
		 * Will save the data for the game scene to load it
		 */
        public static void SaveNumberOfPlayers(int _players)
        {
            PlayerPrefs.SetInt(NUMBER_OF_PLAYERS_COOCKIE, _players);
            NumberOfPlayers = -2;
        }

        public static int NumberOfPlayers = -2;

        // -------------------------------------------
        /* 
		 * Will load the data in the game to decide if it must create a game or join an existing one
		 */
        public static int LoadNumberOfPlayers()
        {
            if (NumberOfPlayers == -2)
            {
                NumberOfPlayers = PlayerPrefs.GetInt(NUMBER_OF_PLAYERS_COOCKIE, -1);
            }
#if IGNORE_SINGLEPLAYER
            if (NumberOfPlayers == 1)
            {
                return 10;
            }
            else
            {
                return NumberOfPlayers;
            }
#else
            return NumberOfPlayers;
#endif

        }

        // -------------------------------------------
        /* 
		 * Will save the data the total number of level that has the app
		 */
        public static void SaveTotalNumberOfLevels(int _levels)
        {
            PlayerPrefs.SetInt(NUMBER_OF_LEVELS_COOCKIE, _levels);
        }

        // -------------------------------------------
        /* 
		 * Will load the the total number of levels that has the app
		 */
        public static int LoadTotalNumberOfLevels()
        {
            return PlayerPrefs.GetInt(NUMBER_OF_LEVELS_COOCKIE, -1);
        }

        // -------------------------------------------
        /* 
		 * Will save the data of the IP address of the server
		 */
        public static void SaveIPAddressServer(string _ipAddress)
        {
            PlayerPrefs.SetString(IP_ADDRESS_COOCKIE, _ipAddress);
        }

        // -------------------------------------------
        /* 
		 * Will load the IP address of the server
		 */
        public static string LoadIPAddressServer()
        {
            return PlayerPrefs.GetString(IP_ADDRESS_COOCKIE, SOCKET_SERVER_ADDRESS);
        }

        // -------------------------------------------
        /* 
		 * Will save the port of the server
		 */
        public static void SavePortServer(int _port)
        {
            PlayerPrefs.SetInt(PORT_ADDRESS_COOCKIE, _port);
        }

        // -------------------------------------------
        /* 
		 * Will load the port server address
		 */
        public static int LoadPortServer()
        {
            return PlayerPrefs.GetInt(PORT_ADDRESS_COOCKIE, PORT_SERVER_ADDRESS);
        }

        // -------------------------------------------
        /* 
		 * Will save the room number to use in the server
		 */
        public static void SaveRoomNumberInServer(int _room)
        {
            PlayerPrefs.SetInt(ROOM_NUMBER_COOCKIE, _room);
        }

        // -------------------------------------------
        /* 
		 * Will load the port server address
		 */
        public static int LoadRoomNumberInServer(int _defaultRoom)
        {
            return PlayerPrefs.GetInt(ROOM_NUMBER_COOCKIE, _defaultRoom);
        }

        // -------------------------------------------
        /* 
		 * Will save the room number to use in the server
		 */
        public static void SaveRoomNameInServer(string _room)
        {
            PlayerPrefs.SetString(ROOM_NAME_COOCKIE, _room);
        }

        // -------------------------------------------
        /* 
		 * Will load the port server address
		 */
        public static string LoadRoomNameInServer(string _defaultRoom)
        {
            return PlayerPrefs.GetString(ROOM_NAME_COOCKIE, _defaultRoom);
        }
        

        // -------------------------------------------
        /* 
		 * Will save the assigned name room for the lobby
		 */
        public static void SaveNameRoomLobby(string _nameRoom)
        {
            PlayerPrefs.SetString(NAME_ROOM_LOOBY_COOCKIE, _nameRoom);
        }

        // -------------------------------------------
        /* 
		 * Will load the assigned name room for the lobby
		 */
        public static string LoadNameRoomLobby()
        {
            string nameRoom = PlayerPrefs.GetString(NAME_ROOM_LOOBY_COOCKIE, "");
            return nameRoom;
        }

        // -------------------------------------------
        /* 
		 * Will save if we are in the lobby
		 */
        public static void SaveIsRoomLobby(bool _value)
        {
            PlayerPrefs.SetInt(IS_ROOM_LOOBY_COOCKIE, (_value ? 1 : 0));
        }

        // -------------------------------------------
        /* 
		 * Will load if we are in the lobby
		 */
        public static bool LoadIsRoomLobby()
        {
            return PlayerPrefs.GetInt(IS_ROOM_LOOBY_COOCKIE, 0) == 1;
        }


        // -------------------------------------------
        /* 
		 * Will save the invited friends to the game
		 */
        public static void SaveFriendsGame(string _friends)
        {
            PlayerPrefs.SetString(FACEBOOK_FRIENDS_COOCKIE, _friends);
        }

        // -------------------------------------------
        /* 
		 * Will load the friends of the game
		 */
        public static string LoadFriendsGame()
        {
            return PlayerPrefs.GetString(FACEBOOK_FRIENDS_COOCKIE, "");
        }

        // -------------------------------------------
        /* 
		 * Will save the id of the machine which host the room
		 */
        public static void SaveMachineIDServer(int _idMachineHostRoom)
        {
            PlayerPrefs.SetInt(MACHINE_ID_HOST_ROOM_COOCKIE, _idMachineHostRoom);
        }

        // -------------------------------------------
        /* 
		 * Will load the friends of the game
		 */
        public static int LoadMachineIDServer(int _idMachineHostRoom)
        {
            return PlayerPrefs.GetInt(MACHINE_ID_HOST_ROOM_COOCKIE, _idMachineHostRoom);
        }

        // -------------------------------------------
        /* 
		 * Will save information about to enable or not ARCore
		 */
        public static void SaveGoogleARCore(int _enableGoogleARCore)
        {
            PlayerPrefs.SetInt(GOOGLE_ARCORE_COOCKIE, _enableGoogleARCore);
        }

        // -------------------------------------------
        /* 
		 * Will load the information about to enable or not ARCore
		 */
        public static int LoadGoogleARCore(int _enableGoogleARCore)
        {
            return PlayerPrefs.GetInt(GOOGLE_ARCORE_COOCKIE, _enableGoogleARCore);
        }

        // -------------------------------------------
        /* 
		 * Will save generic additional data
		 */
        public static void SaveExtraData(string _extraData)
        {
            PlayerPrefs.SetString(EXTRA_DATA_COOCKIE, _extraData);
        }

        // -------------------------------------------
        /* 
		 * Will load the generic additional data
		 */
        public static string LoadExtraData()
        {
            string extraData = PlayerPrefs.GetString(EXTRA_DATA_COOCKIE, "");
            return extraData;
        }

        // -------------------------------------------
        /* 
		 * Will save the director mode activation
		 */
        public static void SaveDirectorMode(int _directorMode)
        {
            PlayerPrefs.SetInt(DIRECTOR_MODE_COOCKIE, _directorMode);
        }

        // -------------------------------------------
        /* 
		 * Will load the director mode activation
		 */
        public static int LoadDirectorMode(int _directorMode)
        {
            return PlayerPrefs.GetInt(DIRECTOR_MODE_COOCKIE, _directorMode);
        }

        // -------------------------------------------
        /* 
		 * Will save the spectator mode activation
		 */
        public static void SaveSpectatorMode(int _spectatorMode)
        {
            PlayerPrefs.SetInt(SPECTATOR_MODE_COOCKIE, _spectatorMode);
        }

        // -------------------------------------------
        /* 
		 * Will load the spectator mode activation
		 */
        public static int LoadSpectatorMode(int _spectatorMode)
        {
            return PlayerPrefs.GetInt(SPECTATOR_MODE_COOCKIE, _spectatorMode);
        }


        // -------------------------------------------
        /* 
         * Will save balance loader create room PHP url
         */
        public static void BalanceLoaderSaveCreateRoomPHP(string _createRoomPHP)
        {
            PlayerPrefs.SetString(CREATE_ROOM_COOCKIE, _createRoomPHP);
        }

        // -------------------------------------------
        /* 
         * Will load balance loader create room PHP url
         */
        public static string BalanceLoaderLoadCreateRoomPHP()
        {
            return PlayerPrefs.GetString(CREATE_ROOM_COOCKIE, BALANCE_LOADER_CREATE_NEW_ROOM);
        }

        // -------------------------------------------
        /* 
         * Will save balance loader list rooms PHP url
         */
        public static void BalanceLoaderSaveListRoomsPHP(string _listRoomsPHP)
        {
            PlayerPrefs.SetString(LIST_ROOMS_COOCKIE, _listRoomsPHP);
        }

        // -------------------------------------------
        /* 
         * Will load balance loader list rooms PHP url
         */
        public static string BalanceLoaderLoadListRoomsPHP()
        {
            return PlayerPrefs.GetString(LIST_ROOMS_COOCKIE, BALANCE_LOADER_GET_LIST_ROOMS);
        }


        // -------------------------------------------
        /* 
         * Will save the buffer size for the tcp socket connection
         */
        public static void SaveBufferSizeSend(int _bufferSizeSend)
        {
            PlayerPrefs.SetInt(BUFFER_SIZE_SEND_COOCKIE, _bufferSizeSend);
        }

        // -------------------------------------------
        /* 
         * Will load the buffer size for the tcp socket connection
         */
        public static int LoadBufferSizeSend()
        {
            return PlayerPrefs.GetInt(BUFFER_SIZE_SEND_COOCKIE, 65536);
        }

        // -------------------------------------------
        /* 
        * Will save the buffer size for the tcp socket connection
        */
        public static void SaveBufferSizeReceive(int _bufferSizeReceive)
        {
            PlayerPrefs.SetInt(BUFFER_SIZE_RECEIVE_COOCKIE, _bufferSizeReceive);
        }

        // -------------------------------------------
        /* 
        * Will load the buffer size for the tcp socket connection
        */
        public static int LoadBufferSizeReceive()
        {
            return PlayerPrefs.GetInt(BUFFER_SIZE_RECEIVE_COOCKIE, 65536);
        }

        // -------------------------------------------
        /* 
        * Will save the timeout to wait for the tcp socket connection
        */
        public static void SaveTimeoutReceive(int _timeoutSend)
        {
            PlayerPrefs.SetInt(TIMEOUT_RECEIVE_COOCKIE, _timeoutSend);
        }

        // -------------------------------------------
        /* 
        * Will save the timeout to wait for the tcp socket connection
        */
        public static int LoadTimeoutReceive()
        {
            return PlayerPrefs.GetInt(TIMEOUT_RECEIVE_COOCKIE, 0);
        }

        // -------------------------------------------
        /* 
        * Will save the timeout to wait for the tcp socket connection
        */
        public static void SaveTimeoutSend(int _timeoutSend)
        {
            PlayerPrefs.SetInt(TIMEOUT_SEND_COOCKIE, _timeoutSend);
        }

        // -------------------------------------------
        /* 
        * Will save the timeout to wait for the tcp socket connection
        */
        public static int LoadTimeoutSend()
        {
            return PlayerPrefs.GetInt(TIMEOUT_SEND_COOCKIE, 0);
        }

        // -------------------------------------------
        /* 
		 * Enable the background for VR or disable for AR
		 */
        public static void SaveEnableBackground(bool _enableBackground)
        {
            PlayerPrefs.SetInt(AR_ENABLE_BACKGROUND, (_enableBackground ? 1 : 0));
        }

        // -------------------------------------------
        /* 
		 * Load enable the background for VR or disable for AR
		 */
        public static bool LoadEnableBackground()
        {
            return PlayerPrefs.GetInt(AR_ENABLE_BACKGROUND, -1) == 1;
        }


        // -------------------------------------------
        /* 
		 * Save character model to use
		 */
        public static void SaveCharacter6DOF(int _character)
        {
            PlayerPrefs.SetInt(CHARACTER_SELECTED_COOCKIE, _character);
        }

        // -------------------------------------------
        /* 
		 * Load character model to use
		 */
        public static int LoadCharacter6DOF()
        {
            return PlayerPrefs.GetInt(CHARACTER_SELECTED_COOCKIE, -1);
        }

        // -------------------------------------------
        /* 
		 * Save house model to use
		 */
        public static void SaveLevel6DOF(int _house)
        {
            PlayerPrefs.SetInt(LEVEL_SELECTED_COOCKIE, _house);
        }

        // -------------------------------------------
        /* 
		 * Load house model to use
		 */
        public static int LoadLevel6DOF()
        {
            return PlayerPrefs.GetInt(LEVEL_SELECTED_COOCKIE, -1);
        }

    }
}

