using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GodotManager.Library.Data;
using GodotManager.Library.Data.POCO.Internal;

namespace GodotManager.Library.Managers.Tuxfamily;

public class Godot : ISiteManager<TuxfamilyVersion>
{
    public delegate void ReleaseCountEventHandler(int count);

    public delegate void ReleaseProgressEventHandler(int curr, int max);

    public event ReleaseCountEventHandler ReleaseCount;
    public event ReleaseProgressEventHandler ReleaseProgress;

    private static TuxfamilyClient CreateConnection()
    {
        if (!Database.Settings.UseProxy) return new TuxfamilyClient(new HttpClient());
        return new TuxfamilyClient(
            new HttpClient(new HttpClientHandler()
            {
                Proxy = new WebProxy(Database.Settings.ProxyHost, Database.Settings.ProxyPort),
                UseProxy = Database.Settings.UseProxy
            })
        );
    }

    private async Task<bool> HasNewRelease()
    {
        var conn = CreateConnection();
        var res = await conn.GetLatestVersions();
        var newRelease = false;
        foreach (var release in res)
        {
            if (Database.HasLatestRelease(release.Major))
            {
                if (Database.HasTuxfamilyVersion(release.Release) ||
                    Database.HasTuxfamilyVersion(release.Prerelease)) continue;
                newRelease = true;
                Database.UpdateLatestRelease(release);
            }
            else
            {
                newRelease = true;
                Database.AddLatestRelease(release);
            }
        }

        return newRelease;
    }

    public async Task<bool> NewReleaseAvailable()
    {
        return await HasNewRelease();
    }

    public async Task<TuxfamilyVersion> GetLatestRelease()
    {
        var releases = Database.GetAllLatest();
        TuxfamilyVersion newestRelease = null;
        TuxfamilyVersion newestPrerelease = null;
        
        foreach (var release in releases)
        {
            if (Database.HasTuxfamilyVersion(release.Release))
                newestRelease = Database.GetTuxfamilyVersion(release.Release);
            
            if (Database.HasTuxfamilyVersion(release.Prerelease))
                newestPrerelease = Database.GetTuxfamilyVersion(release.Prerelease);
        }

        return await new Task<TuxfamilyVersion>(() =>
        {
            var versions = new[] { newestRelease, newestPrerelease };
            return versions.OrderBy(x => x?.SemVersion).First();
        });
    }
    
    public async Task<TuxfamilyVersion> GetLatestRelease(int major)
    {
        var release = Database.GetLatest(major);
        var newestRelease = Database.GetTuxfamilyVersion(release.Release);
        var newestPrerelease = Database.GetTuxfamilyVersion(release.Prerelease);
        
        return await new Task<TuxfamilyVersion>(() =>
        {
            var versions = new[] { newestRelease, newestPrerelease };
            return versions.OrderBy(x => x?.SemVersion).First();
        });
    }

    public async Task<IReadOnlyList<TuxfamilyVersion>> GetReleases()
    {
        var conn = CreateConnection();
        var res = await conn.GetVersions();
        return res;
    }

    public async Task UpdateDatabase()
    {
        var releases = await GetReleases();
        var count = releases.Count;
        var curr = 0;
        ReleaseCount?.Invoke(count);

        foreach (var release in releases)
        {
            curr++;
            ReleaseProgress?.Invoke(curr, count);
            if (Database.HasTuxfamilyVersion(release.Id))
                continue;
            Database.AddTuxfamilyVersion(release);
        }
    }
}