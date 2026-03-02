using System.Linq.Expressions;

namespace Bacera
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ExtOrderBy<T>(this IQueryable<T> source, string property, bool asc)
        {
            return ApplyOrder<T>(source, property, asc ? "OrderBy" : "OrderByDescending");
        }

        public static IQueryable<T> ExtThenOrderBy<T>(this IQueryable<T> source, string property, bool asc)
        {
            return ApplyOrder<T>(source, property, asc ? "ThenBy" : "ThenByDescending");
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderBy");
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }

        public static IQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }

        public static IQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }

        static IQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            if (string.IsNullOrEmpty(property))
            {
                return source;
            }

            var props = property.Split('.');
            var type = typeof(T);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            var list = type.GetProperties();

            foreach (var prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                //var pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public);
                var pi = type.GetProperties()
                    .FirstOrDefault(x => x.Name.Equals(prop, StringComparison.CurrentCultureIgnoreCase));
                if (pi == null) continue;
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                              && method.IsGenericMethodDefinition
                              && method.GetGenericArguments().Length == 2
                              && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { source, lambda });
            return (IQueryable<T>)result!;
        }
    }
}