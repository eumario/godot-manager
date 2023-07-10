using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace GodotManager.Library.Utility;

public class SemVersionCompare : IComparer<SemanticVersion>
{
    private static SemVersionCompare _instance;
    public static SemVersionCompare Instance => _instance ??= new SemVersionCompare();
    
    private readonly Regex _numericMatch = new Regex(@"(\d+)");
    public int Compare(SemanticVersion x, SemanticVersion y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        var versionComparison = Comparer<Version>.Default.Compare(x.Version, y.Version);
        if (versionComparison != 0) return versionComparison;
        var xRegRes = _numericMatch.Match(x.SpecialVersion);
        var yRegRes = _numericMatch.Match(y.SpecialVersion);
        if (xRegRes.Success && yRegRes.Success) return int.Parse(xRegRes.Groups[1].Value).CompareTo(int.Parse(yRegRes.Groups[1].Value));;
        return string.Compare(x.SpecialVersion, y.SpecialVersion, StringComparison.Ordinal);
    }

    public int CompareTo(SemanticVersion x, SemanticVersion y) => Compare(x, y);
}