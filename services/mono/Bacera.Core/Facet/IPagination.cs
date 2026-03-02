namespace Bacera
{
    public interface IPagination
    {
        int Page { get; }
        int Size { get; }
        int Total { get; }
        int PageCount { get; }
    }
}
