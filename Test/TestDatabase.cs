using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotManager.Library.Models;
using Microsoft.EntityFrameworkCore;
using Octokit;
using Octokit.Internal;
using AppContext = GodotManager.Library.Database.AppContext;

namespace GodotManager.Test;
[Tool, GlobalClass]
public partial class TestDatabase : EditorScript
{
    [GodotOverride]
    public async void OnRun()
    {
        var dbExists = FileAccess.FileExists("res://Test/test_db.sqlite3");
        await using var context = AppContext.InitDatabase(ProjectSettings.GlobalizePath("res://Test/test_db.sqlite3"));
        if (!dbExists)
            await SeedDatabase(context);
        foreach (var release in context.EngineReleases.Where(x => x.Repo == "godot-builds")
                     .OrderByDescending(x => x.Version))
        {
            var standard = release.StandardUrl;
            var dotnet = release.DotnetUrl;
            GD.Print($"Version: {release.Version} - Standard: {standard} - Dotnet: {dotnet}");
        }

        GD.Print("Done");
    }

    public async Task SeedDatabase(AppContext context)
    {
        var conn = new Connection(
            new ProductHeaderValue($"Godot-Manager.0.3.0.Test"));
        var client = new GitHubClient(conn);
        GD.Print("Fetching releases from github.com/godotengine/godot...");
        var res = await client.Repository.Release.GetAll("godotengine", "godot");
        foreach (var release in res)
        {
            var er = EngineRelease.FromRelease(release, "godot");
            context.EngineReleases.Add(er);
        }
        context.SaveChanges();
        GD.Print("Releases fetched, and stored in database.");
        
        GD.Print("Fetching releases from github.com/godotengine/godot-builds...");
        res = await client.Repository.Release.GetAll("godotengine", "godot-builds");
        foreach (var release in res)
        {
            var er = EngineRelease.FromRelease(release, "godot-builds");
            context.EngineReleases.Add(er);
        }

        context.SaveChanges();
        GD.Print("Releases fetched, and stored in database.");
    }

    public override partial void _Run();
}
