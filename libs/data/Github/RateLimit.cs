using Godot;
using Godot.Collections;
using DateTime = System.DateTime;

namespace Github {
	public class RateLimit : Godot.Object {
		public int Limit;
		public int Remaining;
		public int Used;
		public DateTime Reset;
	}
}