using System.Linq.Expressions;

namespace Bacera
{
    public abstract class Criteria<T> : Criteria, ICriteria<T>, ICriteriaPool<T>
    {
        private IQueryable<T>? _source;


        internal ICriteriaPool<T> Self => this;
        protected virtual bool OnEarlyBreak() => false;

        protected virtual void OnCollecting()
        {
        }

        protected virtual void OnCollected()
        {
        }

        protected abstract void OnCollect(ICriteriaPool<T> pool);

        IQueryable<T> ICriteria<T>.PagedFilter(IQueryable<T> source)
        {
            _source = source;
            OnCollecting();
            if (!OnEarlyBreak())
                OnCollect(Self);
            OnCollected();
            if (string.IsNullOrEmpty(SortField))
                return Pagination(_source);

            if (SortField.Split(",").Length <= 1)
                return Pagination(_source.OrderBy(SortField, SortFlag));

            var i = 0;
            foreach (var field in SortField.Split(","))
            {
                _source = i == 0
                    ? _source.ExtOrderBy(field, SortFlag)
                    : _source.ThenOrderBy(field, SortFlag);
                i++;
            }

            return Pagination(_source);
        }

        IQueryable<T> ICriteria<T>.Filter(IQueryable<T> source)
        {
            _source = source;
            OnCollecting();
            if (!OnEarlyBreak())
                OnCollect(Self);
            OnCollected();
            if (string.IsNullOrEmpty(SortField))
                return _source;

            if (SortField.Split(",").Length <= 1)
                return _source.OrderBy(SortField, SortFlag);

            var i = 0;
            foreach (var field in SortField.Split(","))
            {
                _source = i == 0
                    ? _source.ExtOrderBy(field, SortFlag)
                    : _source.ThenOrderBy(field, SortFlag);
                i++;
            }

            return _source;
        }


        protected abstract IQueryable<T> Pagination(IQueryable<T> source);

        ICriteriaPool<T> ICriteriaPool<T>.Add(Expression<Func<T, bool>> predicate)
        {
            _source = _source!.Where(predicate);
            return this;
        }

        ICriteriaPool<T> ICriteriaPool<T>.Add(Expression<Func<T, bool>> predicate, bool @if)
            => @if ? ((ICriteriaPool<T>)this).Add(predicate) : this;

        ICriteriaPool<T> ICriteriaPool<T>.With(Action<ICriteriaPool<T>> pipe)
        {
            pipe(this);
            return this;
        }
    }
}