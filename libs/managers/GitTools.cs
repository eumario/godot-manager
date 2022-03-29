// Could not get LibGit2Sharp to work. This is a temporary way around using it.

using System;
using System.Diagnostics;
using System.IO;

namespace GitTools
{
    public class GitRunner
    {
        public string GitPath { get; }
        public string WorkingDirectory { get; }
        public GitRunner(string gitPath, string workingDirectory = null)
        {
            GitPath = gitPath ?? throw new ArgumentNullException(nameof(gitPath));
            WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(gitPath);
        }

        public string Run(string arguments)
        {
            var data = new ProcessStartInfo(GitPath, arguments)
            {
                UseShellExecute = false, // This might need to be different on linux?
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                WorkingDirectory = WorkingDirectory,
            };
            var process = new Process
            {
                StartInfo = data,
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }
    }
}