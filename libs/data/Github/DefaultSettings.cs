using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Github {
	public class DefaultSettings {
		public static DefaultContractResolver contractResolver = new DefaultContractResolver {
			NamingStrategy = new SnakeCaseNamingStrategy()
		};

		public static JsonSerializerSettings defaultJsonSettings = new JsonSerializerSettings {
			ContractResolver = contractResolver,
			DateFormatHandling = DateFormatHandling.IsoDateFormat,
			Formatting = Formatting.Indented
		};
	}
}