namespace Bacera.Gateway.Agent;

public class ReferredUserVerificationViewModel
{
    public DateTime UpdatedOn { get; set; } = DateTime.MinValue;
    public VerificationStatusTypes Status { get; set; } = VerificationStatusTypes.Incomplete;
    public bool IsEmpty => UpdatedOn == DateTime.MinValue && Status == VerificationStatusTypes.Incomplete;
    public static ReferredUserVerificationViewModel Empty() => new();
}