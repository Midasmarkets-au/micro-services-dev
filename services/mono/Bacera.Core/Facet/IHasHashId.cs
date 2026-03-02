namespace Bacera;

public interface IHasHashId
{
    public string HashId { get; }
    static Func<long, string> HashEncode { get; set; } = null!;
    static Func<string, long> HashDecode { get; set; } = null!;
}