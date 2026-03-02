namespace Bacera.Gateway;

public interface ILeadable : ILeadable<long, int>
{
}

public interface ILeadable<out TC> : ILeadable<long, TC> where TC : struct, IConvertible
{
}

public interface ILeadable<out T, out TC>
    where T : struct, IConvertible
    where TC : struct, IConvertible
{
    T Id { get; }
    TC Status { get; }
    DateTime UpdatedOn { get; }
}