using Godot;

public class SignalManager : Node {
	static SignalManager _instance;


	// Possible C# Attribute for Singleton by Godot method of Singletons?   Must Research!
	private SignalManager() {
		if (GetTree().Root.HasNode("SignalManager"))
			_instance = GetTree().Root.GetNode<SignalManager>("SignalManager");
		else {
			_instance = this;
			GetTree().Root.AddChild(this);
		}
	}

	public static SignalManager Instance {
		get {
			if (_instance == null)
				_instance = new SignalManager();
			return _instance;
		}
	}

	public static Error Connect(string Event, Object Listener, string MethodCallback) {
		return ((Object)Instance).Connect(Event, Listener, MethodCallback);
	}

	public static new void EmitSignal(string Event, params object[] Args) {
		((Object)Instance).EmitSignal(Event, Args);
	}

#region Custom Signals
	[Signal]
	public delegate void page_changed();
#endregion

}