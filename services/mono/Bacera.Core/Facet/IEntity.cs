namespace Bacera
{
    public interface IIdentityGetter<out TPk> where TPk : IEquatable<TPk> { TPk Id { get; } }
    public interface IIdentitySetter<in TPk> where TPk : IEquatable<TPk> { TPk Id { set; } }

    public interface IEntity : IEntity<long> { }
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface IEntity<TPk> : IIdentityGetter<TPk>, IIdentitySetter<TPk> where TPk : IEquatable<TPk> { }
}
