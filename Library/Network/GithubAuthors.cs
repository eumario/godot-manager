using System;
using Godot;
using GodotManager.Library.Models;
using GodotManager.Library.Util;

namespace GodotManager.Library.Network;

public class GithubAuthors
{
    public delegate void AuthorInfoHandler(AuthorInfo authorInfo);

    public event AuthorInfoHandler AuthorFetched;
    public event EventHandler AuthorFetchCompleted;
    private readonly Uri _authorUri =
        new("https://raw.githubusercontent.com/godotengine/godot-website/master/_data/authors.yml");

    public void RefreshAuthors()
    {
        var authors = new DownloadInstance(_authorUri);
        authors.Failed += (_, _) =>
        {
            // TODO: Handle Failed
        };
        authors.Cancelled += (_, _) =>
        {
            // TODO: Handle Cancelled
        };
        authors.ProgressChanged += (sender, progress) =>
        {
            // TODO: Handle Progress
        };
        authors.Completed += (sender, bytes) =>
        {
            var entries = bytes.GetStringFromUtf8().Split("\n");
            var author = new AuthorInfo();
            foreach (var (line, lineNo) in entries.WithIndex())
            {
                if (line.StartsWith("- name: "))
                    author.Name = line.Replace("- name: ", "").Replace("\"", "").Replace("'", "");

                if (line.StartsWith("  image: "))
                    author.AvatarUrl = line.Replace("  image: ", "");

                if (!author.HasAll()) continue;
                AuthorFetched?.Invoke(author);
                author = new AuthorInfo();
            }
            AuthorFetchCompleted?.Invoke(this, EventArgs.Empty);
        };

        authors.StartDownload();
    }
}