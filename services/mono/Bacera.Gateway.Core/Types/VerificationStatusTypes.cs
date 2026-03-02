namespace Bacera.Gateway;

public enum VerificationStatusTypes
{
    Incomplete = 0,
    AwaitingReview = 1,
    UnderReview = 2,
    AwaitingApprove = 3,
    Approved = 4,
    Rejected = 5,

    AwaitingAddressVerify = 6,
    AwaitingCodeVerify = 7,
    CodeVerified = 8,
}