namespace Bacera.Gateway;

public interface IHasMatter : IIdentityGetter<long>
{
    public Matter IdNavigation { get; set; }
}