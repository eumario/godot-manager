using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using GodotManager.Library.Models;
using GodotManager.Library.Util;
using Microsoft.EntityFrameworkCore;

namespace GodotManager.Library.Database;

public class AppContext : DbContext
{
    public virtual DbSet<AuthorInfo> AuthorInfo { get; set; }
    public virtual DbSet<ProjectFile> ProjectFiles { get; set; }
    public virtual DbSet<EngineVersion> EngineVersions { get; set; }
    public virtual DbSet<EngineRelease> EngineReleases { get; set; }

    private string _path;

    public AppContext(string path)
    {
        _path = path;
    }

    public static AppContext InitDatabase(string path)
    {
        var context = new AppContext(path);
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA journal_mode='DELETE';");
        return context;
    }

    public bool IsLatestVersion(EngineRelease release)
    {
        var latest = EngineReleases.OrderByDescending(x => x.Version).FirstOrDefault(x => x.Repo == release.Repo);
        if (latest == null) return false;
        return release.Version >= latest.Version;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlite($"Data Source={_path}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // EngineRelease data object
        modelBuilder.Entity<EngineRelease>()
            .Property(e => e.Version)
            .HasConversion(v => v.ToNormalizedString(),
                v => SemanticVersion.Parse(v)
            );
        modelBuilder.Entity<EngineRelease>()
            .Property(e => e.StandardUrls)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<Dictionary<PlatformType, VersionUrl>>(v, JsonSerializerOptions.Default)
            );
        modelBuilder.Entity<EngineRelease>()
            .Property(e => e.DotnetUrls)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<Dictionary<PlatformType, VersionUrl>>(v, JsonSerializerOptions.Default)
            );
        modelBuilder.Entity<EngineRelease>()
            .Property(e => e.Sha512Sum)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<VersionUrl>(v, JsonSerializerOptions.Default)
            );
    }
}