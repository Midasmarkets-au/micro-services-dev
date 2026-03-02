using Bacera.Gateway.Web;
using Bacera.Gateway.Web.Request;

namespace Bacera.Gateway.ViewModels.Tenant;

public class AccountDetailsFormViewModel
{
    public sealed class Account
    {
        public AccountRoleTypes Role { get; set; }
        public AccountTypes AccountType { get; set; }

        public CurrencyTypes CurrencyId { get; set; }
        public FundTypes FundType { get; set; }
        public string Group { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string SalesName { get; set; } = string.Empty;
        public string SalesCode { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string OpenFor { get; set; } = string.Empty;

        public string Consent { get; set; } = "Yes";
        public string Pep { get; set; } = "No";
    }

    public sealed class User
    {
        public DateTime Birthday { get; set; }
        public string Citizen { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string IdType { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
    }

    public sealed class Verification
    {
        public VerificationStartedDTO? Started { get; set; }
        public VerificationInfoDTO? Info { get; set; }
        public VerificationFinancialDTO? Financial { get; set; }
        public VerificationQuizDTO.AnswerDTO[]? Quiz { get; set; }
        public VerificationAgreementDTO? Agreement { get; set; }
        public IList<VerificationDocumentMedium>? Document { get; set; }
    }
}