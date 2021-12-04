using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace Github {
	[JsonObject(MemberSerialization.OptIn)]
	public class Author : Object {
		[JsonProperty]
		public string Login;
		[JsonProperty]
		public int Id;
		[JsonProperty]
		public string NodeId;
		[JsonProperty]
		public string AvatarUrl;
		[JsonProperty]
		public string GravatarId;
		[JsonProperty]
		public string Url;
		[JsonProperty]
		public string FollowersUrl;
		[JsonProperty]
		public string FollowingUrl;
		[JsonProperty]
		public string GistsUrl;
		[JsonProperty]
		public string StarredUrl;
		[JsonProperty]
		public string SubscriptionsUrl;
		[JsonProperty]
		public string OrganizationsUrl;
		[JsonProperty]
		public string ReposUrl;
		[JsonProperty]
		public string EventsUrl;
		[JsonProperty]
		public string ReceivedEventsUrl;
		[JsonProperty]
		public string Type;
		[JsonProperty]
		public bool SiteAdmin;

		public static Author FromJson(string data) {
			return JsonConvert.DeserializeObject<Author>(data,DefaultSettings.defaultJsonSettings);
		}
	}
}