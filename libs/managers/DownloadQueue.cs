using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using Queue = System.Collections.Queue;

public class DownloadQueue : Node {
	[Signal]
	public delegate void download_completed(ImageDownloader dld);

	[Signal]
	public delegate void queue_finished();

	Queue<ImageDownloader> queued;
	Array<ImageDownloader> active;
	int iMaxDownloads;
	Timer checkActive;

	public IList<ImageDownloader> Queued => queued.ToList();

	public DownloadQueue(int maxDownloads = 5) {
		queued = new Queue<ImageDownloader>();
		active = new Array<ImageDownloader>();
		iMaxDownloads = maxDownloads;
		checkActive = new Timer();
		checkActive.Autostart = false;
		checkActive.WaitTime = 0.5f;
		checkActive.OneShot = false;
		AddChild(checkActive);
	}

	public override void _Ready()
	{
		this.OnReady();
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

	[SignalHandler("timeout", nameof(checkActive))]
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