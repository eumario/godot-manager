using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;
using GodotManager.Library.Utility;
using Octokit;
using Octokit.Internal;

namespace GodotManager.Library.Managers.Github;

public class Godot : ISiteManager<Release>
{
    public delegate void ReleaseCountEventHandler(int count);

    public delegate void ReleaseProgressEventHandler(int curr, int max);

    public event ReleaseCountEventHandler ReleaseCount;
    public event ReleaseProgressEventHandler ReleaseProgress;
    
    private static GitHubClient CreateConnection()
    {
        Connection conn = null;
        // Handle Proxy if available
        if (Database.Settings.UseProxy)
        {
            var proxy = new WebProxy(Database.Settings.ProxyHost, Database.Settings.ProxyPort);
            conn = new Connection(
                new ProductHeaderValue($"Godot-Manager.{Versions.GodotManager}.{Platform.GetName()}"),
                new HttpClientAdapter(() => HttpMessageHandlerFactory.CreateDefault(proxy)));
        }
        else
        {
            conn = new Connection(
                new ProductHeaderValue($"Godot-Manager.{Versions.GodotManager}.{Platform.GetName()}"));
        }

        return new GitHubClient(conn);
    }

    public async Task<bool> NewReleaseAvailable()
    {
        var conn = CreateConnection();
        var res = await conn.Repository.Release.GetLatest("godotengine", "godot");
        return !Database.HasGithubVersion(res);
    }
    
    public async Task<Release> GetLatestRelease()
    {
        var conn = CreateConnection();
        var res = await conn.Repository.Release.GetLatest("godotengine", "godot");
        return res;
    }

    public async Task<IReadOnlyList<Release>> GetReleases()
    {
        var conn = CreateConnection();
        var res = await conn.Repository.Release.GetAll("godotengine", "godot");
        return res;
    }

    public async Task UpdateDatabase()
    {
        var releases = await GetReleases();
        var count = releases.Count;
        var curr = 0;
        ReleaseCount?.Invoke(releases.Count);
        foreach (var release in releases)
        {
            curr++;
            ReleaseProgress?.Invoke(curr, count);
            if (Database.HasGithubVersion(release))
                continue;
            Database.AddGithubVersion(GithubVersion.FromRelease(release, "godot"));
        }
    }
}