using Godot;
using Godot.Sharp.Extras;
using System.Diagnostics;
using Action = System.Action;
using ActionStack = System.Collections.Generic.Stack<System.Action>;
using ArgumentException = System.ArgumentException;
using InvalidOperationException = System.InvalidOperationException;
using Dir = System.IO.Directory;
using SFile = System.IO.File;

public class UpdateWindow : Control
{

    [NodePath("bg/cc/vb/Label")]
    Label _messageText = null;

    [NodePath("bg/cc/vb/ProgressBar")]
    ProgressBar _updateProgress = null;

    [NodePath("WaitTimer")]
    Timer _waitTimer = null;

    /*********************************************
     * Steps:
     * :Windows/Linux:
     * 2: Remove Old Files
     * 3: Copy executable to parent directory
     * 4: (Linux Only) chmod a+x GodotManager.x86_64
     * 5: Copy data_GodotManager to parent directory
     * 6: Launch parent directory GodotManager executable.
     *
     * :MacOS:
     * 2: Remove Old Files
     * 3: Copy Contents of update/GodotManager.app/Contents to parent directory (Mac OS)
     * 4: chmod a+x Contents/MacOS/GodotManager
     * 5: Launch parent directory Contents/MacOS/GodotManager
     *
     * Re-Launched GodotManager, Check for update folder, and remove it when found.
     *********************************************/

    ActionStack steps;
    int maxStep = 0;
    int delay = 5;

    ProcessStartInfo launchGM_psi;

    public override void _Ready()
    {
        this.OnReady();
        InitActions();
    }

    void InitActions() {
        steps = new ActionStack();

        string exe_path = OS.GetExecutablePath();
        
        //************************
        // Windows/Linux Update Script
        //************************
        #if GODOT_WINDOWS || GODOT_UWP || GODOT_LINUXBSD || GODOT_X11
        
        //************************
        // Launch GodotManager
        //************************
        steps.Push(() => {
            _messageText.Text = "Finished copying files, launching GodotManager...";
            launchGM_psi = new ProcessStartInfo();
            launchGM_psi.FileName = exe_path.GetParentFolder().Join(exe_path.GetFile()).NormalizePath();
            launchGM_psi.Arguments = $"--update-complete {Process.GetCurrentProcess().Id}";
            launchGM_psi.WorkingDirectory = exe_path.GetParentFolder().NormalizePath();
            launchGM_psi.UseShellExecute = false;
            launchGM_psi.CreateNoWindow = true;

            //Process proc = Process.Start(psi);
        });
        
        //************************
        // Copy over update/data_GodotManager to update/../data_GodotManager
        //************************
        steps.Push(() => {
            _messageText.Text = "Copying data files from update to install location...";
            Util.CopyDirectory(exe_path.GetBaseDir().Join("data_Godot-Manager").NormalizePath(),
                                exe_path.GetParentFolder().Join("data_Godot-Manager").NormalizePath(), true);
        });

        #if GODOT_LINUXBSD || GODOT_X11
        //************************
        // Ensure GodotManager.x86_64 is executable
        //************************
        steps.Push(() => {
            _messageText.Text = "Setting binary to be executable...";
            Util.Chmod(exe_path.GetParentFolder().Join(exe_path.GetFile()).NormalizePath(), 0755);
        });
        #endif

        //************************
        // Copy GodotManager.[exe|x86_64] to update/../GodotManager.[exe|x86_64]
        //************************
        steps.Push(() => {
            _messageText.Text = "Copying executable binary over...";
            Util.CopyTo(exe_path,exe_path.GetParentFolder().Join(exe_path.GetFile()));
        });

        //************************
        // Remove Old Files
        //************************
        steps.Push(() => {
            _messageText.Text = "Removing old files...";
            Dir.Delete(exe_path.GetParentFolder().Join("data_Godot-Manager").NormalizePath(),true);
            SFile.Delete(exe_path.GetParentFolder().Join(exe_path.GetFile()));
        });

        //************************
        // Mac OS Update Script
        //************************
        #elif GODOT_MACOS || GODOT_OSX

        //************************
        // Launch parent Directory Contents/MacOS/GodotManager
        //************************
        steps.Push(() => {
            _messageText.Text = "Finished copying files, launching GodotManager...";
            launchGM_psi = new ProcessStartInfo();
            launchGM_psi.FileName = exe_path.GetParentFolder().GetParentFolder().GetBaseDir().Join("Contents","MacOS",exe_path.GetFile()).NormalizePath();
            launchGM_psi.Arguments = $"--update-complete {Process.GetCurrentProcess().Id}";
            launchGM_psi.WorkingDirectory = exe_path.GetParentFolder().GetParentFolder().GetBaseDir();
            launchGM_psi.UseShellExecute = false;
            launchGM_psi.CreateNoWindow = true;

            //Process proc = Process.Start(psi);
        });

        //************************
        // chmod a+x Contents/MacOS/GodotManager
        //************************
        steps.Push(() => {
            _messageText.Text = "Setting binary to be executable...";
            Util.Chmod(exe_path.GetParentFolder().GetParentFolder().GetBaseDir().Join(exe_path.GetFile()).NormalizePath(), 0755);
        });

        //************************
        // xattr -cr "GodotManager.app"
        //************************
        steps.Push(() => {
            _messageText.Text = "Clearing Security bits...";
            Util.XAttr(exe_path.GetParentFolder().GetParentFolder().GetBaseDir(), "-cr");
        })

        //************************
        // Copy update/GodotManager.app/Contents to ../Contents
        //************************
        steps.Push(() => {
            _messageText.Text = "Copying App bundle contents to install location...";
            Util.CopyDirectory(exe_path.GetParentFolder(), exe_path.GetParentFolder().GetParentFolder().GetBaseDir().Join("Contents"),true);
        });

        //************************
        // Remove Contents folder so we can update.
        //************************
        steps.Push(() => {
            _messageText.Text = "Removing Old Version of Godot Manager...";
            Dir.Delete(exe_path.GetParentFolder().GetParentFolder().GetBaseDir().Join("Contents"),true);
        });
        #endif

        maxStep = steps.Count;
        _updateProgress.MinValue = 0;
        _updateProgress.MaxValue = maxStep;
        _updateProgress.Value = 0;
    }

    public async void StartUpdate(string[] args) {
        while (steps == null)
            await this.IdleFrame();
        while (steps.Count <= 0)
            await this.IdleFrame();

        if (args[0] == "--update" && args.Length > 1) {
            int pid = -1;
            if (int.TryParse(args[1], out pid)) {
                _messageText.Text = "Waiting for Godot Manager to exit...";
                AwaitManagerExit(pid);
                ProcessUpdate();
                _messageText.Text = $"Update Complete, starting Godot Manager in ({delay}) seconds...";
                _waitTimer.Start();
            } else {
                GetParent<SceneManager>().RunMainWindow();
            }
        } else if (args[0] == "--update-complete" && args.Length > 1) {
            int pid = -1;
            if (int.TryParse(args[1], out pid)) {
                _messageText.Text = "Cleaning up...";
                AwaitManagerExit(pid);
                CleanupUpdate();
                GetParent<SceneManager>().RunMainWindow();
            }
        } else {
            GetParent<SceneManager>().RunMainWindow();
        }
    }

    [SignalHandler("timeout", nameof(_waitTimer))]
    void WaitTimer() {
        delay -= 1;
        if (delay <= 0) {
            Process proc = Process.Start(launchGM_psi);
            GetTree().Quit(0);
        } else {
            _messageText.Text = $"Update Complete, starting Godot Manager in ({delay}) seconds...";
            _waitTimer.Start();
        }
    }
    
    void ProcessUpdate() {
        while (steps.Count > 0) {
            Action step = steps.Pop();
            _updateProgress.Value = maxStep - steps.Count;
            step.Invoke();
        }
    }

    void CleanupUpdate() {
        string path = Util.GetUpdateFolder();
        if (Dir.Exists(path)) {
            Dir.Delete(path,true);
        }
    }

    async void AwaitManagerExit(int pid) {
        Process proc;

        try {
            proc = Process.GetProcessById(pid);
        } catch (ArgumentException) {
            // MSDN: The process specified by the processId parameter is not running. The identifier might be expired.
            return;
        } catch (InvalidOperationException) {
            // MSDN: The process was not started by this object.  (???)
            OS.Alert("Process was not started by this Object.","Update Process Failed.");
            GetTree().Quit(-1);
            return;
        }

        while (!proc.HasExited)
            await this.IdleFrame();
    }
}
