using System.ComponentModel;
using System.Diagnostics;

namespace Bacera
{
    public static partial class CoreExt
    {
        public static IQueryable<T> PageBy<T, TC>(this IQueryable<T> me, TC criteria) where TC : IPagination
            => me.Skip((criteria.Page - 1) * criteria.Size).Take(criteria.Size);

        public static IEnumerable<T> PageBy<T, TC>(this IEnumerable<T> me, TC criteria) where TC : IPagination
            => criteria.Page > 0 ? me.Skip((criteria.Page - 1) * criteria.Size).Take(criteria.Size) : me;

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> me, string propertyName) =>
            OrderBy(me, propertyName, false);

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> me, string propertyName, bool desc) =>
            me.ExtOrderBy(propertyName, !desc);

        public static IQueryable<T> ThenOrderBy<T>(this IQueryable<T> me, string propertyName, bool desc) =>
            me.ExtThenOrderBy(propertyName, !desc);


        public static IQueryable<T> PagedFilterBy<T>(this IQueryable<T> me, Criteria<T> criteria) where T : IEntity
            => me.PagedFilterBy<T, long>(criteria);

        public static IQueryable<T> FilterBy<T>(this IQueryable<T> me, Criteria<T> criteria)
            where T : IEntity
            => me.FilterBy<T, long>(criteria);

        public static IQueryable<T> PagedFilterBy<T, TPk>(this IQueryable<T> me, Criteria<T> criteria)
            where T : IEntity<TPk>
            where TPk : IEquatable<TPk>
            => criteria.PagedApplyTo(me);

        public static IQueryable<T> FilterBy<T, TPk>(this IQueryable<T> me, Criteria<T> criteria)
            where T : IEntity<TPk>
            where TPk : IEquatable<TPk>
            => criteria.ApplyTo(me);
    }

    public interface IBoolish
    {
        bool AmIDefault();
    }

    /// <summary>
    /// Should implement default constructor, and enforce the constrain of T : new()
    /// </summary>
    public interface IValue : IBoolish, IEmptish
    {
        /* Semantic */
    }

    public interface IEmptish
    {
        bool AmIVacant();
    }

    static partial class CoreExt
    {
        public static bool IsTangible(this string me) => !string.IsNullOrWhiteSpace(me); // MUST PRESENT !!!

        //public static bool IsTangible(this IEmptish me) => (me != null && !me.AmIVacant());
        public static bool IsTangible<T>(this IEnumerable<T>? me) => (me != null && me.Any());
        public static bool IsTangible<T>(this ICollection<T>? me) => (me != null && me.Count != 0);
        public static bool IsTangible<T>(this T? me) where T : struct => me.HasValue;
        public static bool IsTangible(this DateTime me) => me >= DateTime.MinValue && me <= DateTime.MaxValue;


        public static bool IsFalsy(this bool me) => me == false;
        public static bool IsTruthy(this bool me) => me == true;

        public static bool IsFalsy(this byte me) => me == 0;
        public static bool IsTruthy(this byte me) => me != 0;

        public static bool IsFalsy(this short me) => me == 0;
        public static bool IsTruthy(this short me) => me != 0;

        public static bool IsFalsy(this int me) => me == 0;
        public static bool IsTruthy(this int me) => me != 0;

        public static bool IsFalsy(this long me) => me == 0L;
        public static bool IsTruthy(this long me) => me != 0L;

        public static bool IsFalsy(this float me) => me == 0F;
        public static bool IsTruthy(this float me) => me != 0F;

        public static bool IsFalsy(this double me) => me == 0D;
        public static bool IsTruthy(this double me) => me != 0D;

        public static bool IsFalsy(this decimal me) => me == decimal.Zero;
        public static bool IsTruthy(this decimal me) => me != decimal.Zero;

        public static bool IsFalsy(this char me) => me == char.MinValue;
        public static bool IsTruthy(this char me) => me != char.MinValue;

        public static bool IsFalsy(this string me) => string.IsNullOrEmpty(me);
        public static bool IsTruthy(this string me) => !string.IsNullOrEmpty(me);

        public static bool IsFalsy(this TimeSpan me) => me == TimeSpan.Zero;
        public static bool IsTruthy(this TimeSpan me) => me != TimeSpan.Zero;

        public static bool IsFalsy(this DateTime me) => me == DateTime.MinValue;
        public static bool IsTruthy(this DateTime me) => me != DateTime.MinValue;

        public static bool IsFalsy(this IBoolish? me) => me == null || me.AmIDefault();
        public static bool IsTruthy(this IBoolish? me) => me != null && !me.AmIDefault();

        public static bool IsFalsy<T>(this T? me) where T : struct => me.IsNull();
        public static bool IsTruthy<T>(this T? me) where T : struct => me.NotNull();

        public static bool IsEmpty(this string me) => string.Empty.Equals(me); // MUST PRESENT !!!
        public static bool IsEmpty(this IEmptish? me) => me != null && me.AmIVacant();
        public static bool IsEmpty<T>(this IEnumerable<T> me) => !me.Any();
        public static bool IsEmpty<T>(this ICollection<T> me) => me.Count == 0;

        public static bool NotEmpty(this string me) => !string.Empty.Equals(me); // MUST PRESENT !!!
        public static bool NotEmpty(this IEmptish? me) => me == null || !me.AmIVacant();
        public static bool NotEmpty<T>(this IEnumerable<T> me) => me.Any();
        public static bool NotEmpty<T>(this ICollection<T> me) => me.Count > 0;

        public static bool IsVacant(this string me) => string.IsNullOrWhiteSpace(me); // MUST PRESENT !!!
        public static bool IsVacant(this IEmptish? me) => (me == null || me.AmIVacant());
        public static bool IsVacant<T>(this IEnumerable<T>? me) => (me == null || !me.Any());
        public static bool IsVacant<T>(this ICollection<T>? me) => (me == null || me.Count == 0);
        public static bool IsVacant<T>(this T? me) where T : struct => !me.HasValue || me.Value.IsFalsy();

        public static bool IsFalsy<T>(this T me)
        {
            if (me == null) return true;
            if (me is string s) return s.Length == 0;
            if (me is IBoolish ami) return ami.AmIDefault();

            if (IsNullable(me)) return false;
            var dft = me.GetType().GetDefaultValue();
            return dft != null && dft.Equals(me);
        }

        public static bool IsTruthy<T>(this T me)
        {
            if (me == null) return false;
            if (me is string s) return s.Length != 0;

            if (me is IBoolish ami) return !ami.AmIDefault();

            if (IsNullable(me)) return true;
            var dft = me.GetType().GetDefaultValue();
            return dft != null && !dft.Equals(me);
        }

        public static bool IsNull<T>(this T? me) where T : class => me == null;
        public static bool NotNull<T>(this T? me) where T : class => me != null;

        public static bool IsNull<T>(this T? me) where T : struct => !me.HasValue;
        public static bool NotNull<T>(this T? me) where T : struct => me.HasValue;

#pragma warning disable IDE0060 // Remove unused parameter
        public static bool IsNullable<T>(T me)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var type = typeof(T); // a MUST here, since GetType always return underlying type!
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool CanAcceptNull(this Type me)
            => !me.IsValueType || Nullable.GetUnderlyingType(me) != null;

        public static object? GetDefaultValue(this Type me)
            => me.CanAcceptNull() ? null : Activator.CreateInstance(me);

        abstract partial class ValueBase<T> : Object<T>, IValue, IComparable<T>, IComparable
            where T : ValueBase<T>, new()
        {
            public static readonly T? Empty = new T();

            public static T? From(string tvp) => Empty?.OnFrom(tvp);
            protected virtual T? OnFrom(string tvp) => Empty;

            public virtual bool AmIVacant() => Equals(Empty);
            public virtual bool AmIDefault() => AmIVacant();

            public int CompareTo(object? obj) => CompareTo(obj as T);

            public int CompareTo(T? other) =>
                (other == null) ? 1 : ReferenceEquals(this, other) ? 0 : OnCompareTo(other);

            protected abstract int OnCompareTo(T other);
        }

        abstract partial class Object<T> : IEquatable<T> where T : Object<T>
        {
            [EditorBrowsable(EditorBrowsableState.Never)]
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            protected T Self => (T)this;

            [EditorBrowsable(EditorBrowsableState.Never)]
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            protected virtual bool TypeSensitive => true;

            public bool Equals(T? other)
                => PreEquals(this, other, TypeSensitive) && OnEquals(other);

            public sealed override bool Equals(object? obj) => Equals(obj as T);
            protected abstract bool OnEquals(T? other);

            public sealed override int GetHashCode() => OnGetHashCode();
            protected abstract int OnGetHashCode();
        }

        public static bool PreEquals(object? me, object? other, bool typeSensitive = true)
        {
            if (other == null || me == null) return false; // prevents null-equal. //
            if (ReferenceEquals(me, other)) return true;
            return !typeSensitive || me.GetType() == other.GetType();
        }
    }
}