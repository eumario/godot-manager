using Godot;
using Godot.Collections;

namespace Github {
	public class RateLimit : Godot.Object {
		public int Limit;
		public int Remaining;
		public int Used;
		public System.DateTime Reset;
	}
}