using System.IO;

namespace GodotManager.Library.Utility;

public static class StreamExtensions
{
    public static char ReadChar(this Stream stream) => (char)stream.ReadByte();
}