using System.Collections.Generic;
using Godot;

namespace GodotManager.Library.Models;

public partial class InstallPack : RefCounted
{
    public string Tag = "";
    public List<string> Sources = [];
    public List<string> Dests = [];
    public int CurrentStep { get; private set; }
    public int TotalSteps => Sources.Count;
    public bool IsReady { get; set; }
}