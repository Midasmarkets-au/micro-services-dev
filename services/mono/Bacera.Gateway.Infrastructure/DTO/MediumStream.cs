namespace Bacera.Gateway;

public class MediumStream
{
    public Medium Medium { get; set; } = null!;
    public Stream Stream { get; set; } = null!;
    public bool IsEmpty() => Medium.Id == 0;
    public bool IsStreamEmpty() => Stream.Length == 0;

    public static MediumStream Create(Medium medium, Stream stream)
        => new() { Medium = medium, Stream = stream };
    public static MediumStream Empty() => new() { Medium = new Medium(), Stream = Stream.Null};
}
