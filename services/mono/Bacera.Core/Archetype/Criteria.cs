namespace Bacera
{
    public interface ICriteria
    {
        // Marker
    }

    public interface ICriteria<T> : ICriteria
    {
        IQueryable<T> PagedFilter(IQueryable<T> source);
        IQueryable<T> Filter(IQueryable<T> source);
    }

    partial class CoreExt
    {
        public static IQueryable<T> PagedApplyTo<T>(this ICriteria<T> me, Func<IQueryable<T>> source)
            => me.PagedApplyTo(source());

        public static IQueryable<T> PagedApplyTo<T>(this ICriteria<T> me, IQueryable<T> source)
            => me.PagedFilter(source);

        public static IQueryable<T> ApplyTo<T>(this ICriteria<T> me, IQueryable<T> source)
            => me.Filter(source);

        public static IEnumerable<T> PagedApplyTo<T>(this ICriteria<T> me, IEnumerable<T> source)
            => me.PagedApplyTo(source.AsQueryable());
    }

    public abstract class Criteria : ICriteria, IPagination
    {
        public virtual int Page { get; set; } = 1;
        public virtual int Size { get; set; } = 20;
        public virtual int Total { get; set; } = 0;
        public virtual int PageCount { get; set; } = 1;

        public virtual bool HasMore { get; set; } = false;
        public virtual string? SortField { get; set; }

        /// <summary>
        /// True: DESC (default), False = ASC
        /// </summary>
        public virtual bool SortFlag { get; set; } = true;
    }
}