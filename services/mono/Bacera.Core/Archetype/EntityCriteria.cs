namespace Bacera
{
    public abstract class EntityCriteria<T> : EntityCriteria<T, long> where T : class, IEntity<long>
    {
    }

    public abstract class BaseEntityCriteria<T> : BaseEntityCriteria<T, long> where T : class, IEntity<long>
    {
    }

    public abstract class BaseEntityCriteria<T, TPk> : Criteria<T>
        where T : class, IEntity<TPk> where TPk : IEquatable<TPk>
    {
        protected override bool OnEarlyBreak()
        {
            return false;
        }

        protected override IQueryable<T> Pagination(IQueryable<T> source)
        {
            try
            {
                if (Page < 1 && Size < 1) return source;
                Total = source.Count();
                Page = Page < 1 ? 1 : Page;
                Size = Size < 1 ? 20 : Size;
                PageCount = (int)Math.Ceiling(Total / (decimal)Size);
                HasMore = PageCount > Page;
                return source.Skip((Page - 1) * Size).Take(Size);
            }
            catch
            {
                return source;
            }
        }
    }

    public abstract class EntityCriteria<T, TPk> : BaseEntityCriteria<T, TPk>, IEntityCriteria<T, TPk>
        where T : class, IEntity<TPk> where TPk : IEquatable<TPk>
    {
        public TPk Id { get; set; } = default!;
        public IList<TPk> Ids { get; set; } = new List<TPk>();

        protected override bool OnEarlyBreak()
        {
            Self.Add(x => ((IIdentityGetter<TPk>)x).Id.Equals(Id), Id.IsTruthy())
                .Add(x => Ids.ToList().Contains(((IIdentityGetter<TPk>)x).Id), Ids.IsTangible());
            // disable earlyBreak for security reason
            return false;
            //return ID.IsTruthy() || IDs.IsTangible();
        }

        // protected override IQueryable<T> Pagination(IQueryable<T> source)
        // {
        //     try
        //     {
        //         if (Page < 1 && Size < 1) return source;
        //         Total = source.Count();
        //         Page = Page < 1 ? 1 : Page;
        //         Size = Size < 1 ? 20 : Size;
        //         PageCount = (int)Math.Ceiling(Total / (decimal)Size);
        //         HasMore = PageCount > Page;
        //         return source.Skip((Page - 1) * Size).Take(Size);
        //     }
        //     catch
        //     {
        //         return source;
        //     }
        // }
    }
}