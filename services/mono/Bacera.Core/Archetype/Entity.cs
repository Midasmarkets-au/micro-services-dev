namespace Bacera
{
    public abstract class Entity
    {
        public long Id { get; set; }
    }

    public abstract class Entity<T> : Entity<T, long> where T : Entity<T>
    {
    }

    public abstract class Entity<T, TPk> : IEntity<TPk>, IEquatable<T>
        where T : Entity<T, TPk> where TPk : IEquatable<TPk>
    {
        public TPk Id { get; set; } = default!;
        public bool Equals(T? other) => other != null && Id.Equals(other.Id);
    }
}