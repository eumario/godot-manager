using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotManager.Library.Models;
using Octokit;

namespace GodotManager.Library.Managers;

public class GithubManager
{
    public static async Task FetchReleases(string org, string repo)
    {
        var context = MainWindow.GetInstance()!.Context;
        var conn = new Connection(
            new ProductHeaderValue($"Godot-Manager.0.3.0"));
        var client = new GitHubClient(conn);
        var res = await client.Repository.Release.GetAll(org, repo);
        foreach (var er in res.Select(x => EngineRelease.FromRelease(x, repo)).OrderBy(v => v.Version))
        {
            if (context.EngineReleases.Any(x => x.Version == er.Version && x.Repo == er.Repo))
                continue;
            context.EngineReleases.Add(er);
        }

        await context.SaveChangesAsync();
    }
}