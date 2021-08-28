#if ENABLE_NAKAMA
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

namespace YourNetworkingTools
{
	public class NakamaPlayer
	{
		public string ID;
		public int UID = -1;
		public string MatchID;
		public IUserPresence UserPresence;

		public NakamaPlayer(string _ID, string _MatchID, IUserPresence _UserPresence)
        {
			ID = _ID;
			MatchID = _MatchID;
			UserPresence = _UserPresence;
		}

		public bool Equals(IUserPresence _userPresence)
        {
			return (UserPresence.SessionId == _userPresence.SessionId) &&
					(UserPresence.Username == _userPresence.Username) &&
					(UserPresence.UserId == _userPresence.UserId);
		}

		public bool Equals(IUserPresence _userPresence, string _matchID)
		{
			return (MatchID == _matchID) &&
					(UserPresence.SessionId == _userPresence.SessionId) &&
					(UserPresence.Username == _userPresence.Username) &&
					(UserPresence.UserId == _userPresence.UserId);
		}

		public bool Equals(NakamaPlayer _player)
		{
			return (MatchID == _player.MatchID) &&
					(UserPresence.SessionId == _player.UserPresence.SessionId) &&
					(UserPresence.Username == _player.UserPresence.Username) &&
					(UserPresence.UserId == _player.UserPresence.UserId);
		}
	}
}
#endif