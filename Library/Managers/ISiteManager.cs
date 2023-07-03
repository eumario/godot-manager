using System.Collections.Generic;
using System.Threading.Tasks;

namespace GodotManager.Library.Managers;

public interface ISiteManager<T>
{
    public Task<bool> NewReleaseAvailable();
    public Task<T> GetLatestRelease();
    public Task<IReadOnlyList<T>> GetReleases();

    public Task UpdateDatabase();
}