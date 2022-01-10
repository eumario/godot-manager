using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HTTPResponse : Object {
	public int ResponseCode;
	public Dictionary Headers;
	public byte[] BodyRaw;
	public string Body;
	public bool Cancelled;

	private HTTPClient client;
	private Object call_from;
	private bool bBinary;

	public async Task FromClient(Object call_from, HTTPClient client, bool binary = false) {
		this.client = client;
		this.call_from = call_from;
		ResponseCode = client.GetResponseCode();
		Headers = client.GetResponseHeadersAsDictionary();
		Cancelled = false;
		bBinary = binary;
		Task res = GetBody();
		while (!res.IsCompleted) {
			await this.IdleFrame();
		}
	}

	private async Task GetBody() {
		List<byte> rb = new List<byte>();
		while (client.GetStatus() == HTTPClient.Status.Body)
		{
			// if (Cancelled) {
			// 	GD.Print("Cancelled is true, breaking out!");
			// 	return;
			// }
			client.Poll();
			byte[] chunk = client.ReadResponseBodyChunk();
			if (chunk.Length == 0)
				await this.IdleFrame();
			else {
				rb.AddRange(chunk);
				call_from.EmitSignal("chunk_received", chunk.Length);
			}
		}
		BodyRaw = rb.ToArray();
#pragma warning disable CS0168
		if (!bBinary) {
			try {
				Body = BodyRaw.GetStringFromUTF8();
			} catch (System.Exception ex) {
				// We don't care about exceptions from this, as we may not be getting strings
				// but raw bytes, such as zip files, executables, etc, etc.
			}
		}
#pragma warning restore CS0168
	}
}