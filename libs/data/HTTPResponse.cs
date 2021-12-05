using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HTTPResponse : Object {
	public int ResponseCode;
	public Dictionary Headers;
	public string Body;

	private HTTPClient client;
	private Node call_from;

	public async Task FromClient(Node call_from, HTTPClient client) {
		this.client = client;
		this.call_from = call_from;
		ResponseCode = client.GetResponseCode();
		Headers = client.GetResponseHeadersAsDictionary();
		Task res = GetBody();
		while (!res.IsCompleted) {
			await this.IdleFrame();
		}
	}

	private async Task GetBody() {
		List<byte> rb = new List<byte>();
		while (client.GetStatus() == HTTPClient.Status.Body)
		{
			client.Poll();
			byte[] chunk = client.ReadResponseBodyChunk();
			if (chunk.Length == 0)
				await this.IdleFrame();
			else {
				rb.AddRange(chunk);
				call_from.EmitSignal("chunk_received", chunk.Length);
			}
		}
		Body = rb.ToArray().GetStringFromUTF8();
	}
}