// Could not get LibGit2Sharp to work. This is a temporary way around using it.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Godot;
using Godot.Collections;
namespace GitTools
{
    class Util{
        public static String GetGit()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var output = new Godot.Collections.Array();
                int exit_code = OS.Execute("where", new string[] { "git" }, true, output);
                if (exit_code == 0)
                {
                    return (output[0] as string).StripEdges();
                }
            }
            else// if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) //Which can be used on linux and mac?
            {
                var output = new Godot.Collections.Array();
                int exit_code = OS.Execute("which", new string[] { "git" }, true, output);
                if (exit_code == 0)
                {
                    return (output[0] as string).StripEdges();
                }
            }
            return null;
        }
    }
    public class Runner
    {
        public string GitPath { get; }
        public string WorkingDirectory { get; }
        public Runner(string gitPath, string workingDirectory = null)
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

    public class Repository
    {
        public string Path;
        private Runner Process;
        private String GitPath;
        public Repository(String path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var output = new Godot.Collections.Array();
                int exit_code = OS.Execute("where", new string[] { "git" }, true, output);
                if (exit_code == 0)
                {
                    GitPath = (output[0] as string).StripEdges();
                }
            }
            else// if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) //Which can be used on linux and mac?
            {
                var output = new Godot.Collections.Array();
                int exit_code = OS.Execute("which", new string[] { "git" }, true, output);
                if (exit_code == 0)
                {
                    GitPath = (output[0] as string).StripEdges();
                }
            }

            if (GitPath != null)
            {
                Process = new Runner(GitPath, Path);
                Path = path;
            }
        }

        public string status()
        {
            return Process.Run("status");
        }
    }
}