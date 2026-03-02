namespace Bacera.Gateway;

public class PagedList<T> : List<T>
{
    public int Page { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int Total { get; private set; }
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    public PagedList(IEnumerable<T> items, int count, int page, int pageSize)
    {
        Total = count;
        PageSize = pageSize;
        Page = page;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }
    public static PagedList<T> ToPagedList(IQueryable<T> source, int page, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(items, count, page, pageSize);
    }
}