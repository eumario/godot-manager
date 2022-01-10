using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public class DownloadQueue : Node {
	[Signal]
	public delegate void download_completed(ImageDownloader dld);

	[Signal]
	public delegate void queue_finished();

	System.Collections.Queue queued;
	Array<ImageDownloader> active;
	int iMaxDownloads;
	Timer checkActive;

	public DownloadQueue(int maxDownloads = 5) {
		queued = new System.Collections.Queue();
		active = new Array<ImageDownloader>();
		iMaxDownloads = maxDownloads;
		checkActive = new Timer();
		checkActive.Autostart = false;
		checkActive.WaitTime = 0.5f;
		checkActive.OneShot = false;
		checkActive.Connect("timeout", this, "OnCheckActive");
		AddChild(checkActive);
	}

	public void Push(ImageDownloader dld) {
		queued.Enqueue(dld);
	}

	public void StartDownload() {
		ActivateNext();
	}

	public void ActivateNext() {
		if (queued.Count == 0)
			return;
		ImageDownloader dld = queued.Dequeue() as ImageDownloader;
		if (active.Count == 0)
			checkActive.Start();
		active.Add(dld);
		var task = dld.StartDownload();
		if (task.IsFaulted)
			GD.Print($"Exception Occurred: {task.Exception}");
		dld.ActiveTask = task;
	}

	async void OnCheckActive() {
		Array<ImageDownloader> remove = new Array<ImageDownloader>();
		foreach(ImageDownloader adld in active) {
			await this.IdleFrame();
			if (adld.ActiveTask.IsCompleted) {
				EmitSignal("download_completed", adld);
				remove.Add(adld);
			}
		}

		foreach(ImageDownloader adld in remove) {
			active.Remove(adld);
		}

		while (queued.Count != 0 && active.Count < iMaxDownloads) {
			ActivateNext();
		}
		
		if (active.Count == 0) {
			EmitSignal("queue_finished");
			checkActive.Stop();
		}
	}

	public void PrintQueue() {
		foreach(ImageDownloader dld in queued) {
			GD.Print($"Queued: {dld.Url}");
		}
	}
}