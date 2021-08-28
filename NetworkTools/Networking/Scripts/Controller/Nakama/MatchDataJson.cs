/*
Copyright 2021 Heroic Labs

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#if ENABLE_NAKAMA
using System.Collections.Generic;
using Nakama.TinyJson;
using UnityEngine;
using YourCommonTools;

namespace YourNetworkingTools
{
	public static class MatchDataJson
	{
		public const string EVENTNAME_KEY = "eventName";
		public const string ORIGIN_KEY = "origin";
		public const string TARGET_KEY = "target";
		public const string DATA_KEY = "data";

		public const string NETID_KEY = "netid";
		public const string UID_KEY = "uid";
		public const string INDEX_KEY = "index";
		public const string POSITION_KEY = "position";
		public const string ROTATION_KEY = "rotation";
		public const string SCALE_KEY = "scale";

		public static string AssignUIDS(string[] _uids)
		{
			var values = new Dictionary<string, string>();

			for (int i = 0; i < _uids.Length; i++)
            {
				values.Add(_uids[i], i.ToString());
			}

			return values.ToJson();
		}

		public static string Message(string _eventName, int _origin, int _target, params object[] _data)
		{
			string finalData = "";
			for (int i = 0; i < _data.Length; i++)
            {
				if (finalData.Length > 0)
                {
					finalData += ClientTCPEventsController.TOKEN_SEPARATOR_EVENTS;
				}

				finalData += (string)_data[i];
			}

			var values = new Dictionary<string, string>()
			{
				{ EVENTNAME_KEY, _eventName },
				{ ORIGIN_KEY, _origin.ToString() },
				{ TARGET_KEY, _target.ToString() },
				{ DATA_KEY, finalData }
			};

			return values.ToJson();
		}

		public static string Transform(int _netID, int _uid, int _index, Vector3 _position, Vector3 _rotation, Vector3 _scale)
		{
			var values = new Dictionary<string, string>()
			{
				{ NETID_KEY, _netID.ToString() },
				{ UID_KEY, _uid.ToString() },
				{ INDEX_KEY, _index.ToString() },
				{ POSITION_KEY, Utilities.Vector3ToString(_position) },
				{ ROTATION_KEY, Utilities.Vector3ToString(_rotation) },
				{ SCALE_KEY, Utilities.Vector3ToString(_scale) }
			};

			return values.ToJson();
		}
	}
}
#endif