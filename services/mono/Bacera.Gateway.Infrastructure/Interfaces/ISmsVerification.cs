namespace Bacera.Gateway.Interfaces;

public interface ISmsVerification
{
    Task<bool> VerificationWithoutLimit(string to);
    Task<bool> Verification(string to, string limitKey);
    Task<Tuple<bool, string>> VerificationCheck(string to, string code);
    bool HasReachedLimit(string limitKey);
}