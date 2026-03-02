using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bacera.Gateway;
public class VerificationCategoryTypes
{
    public const string Started = "started";
    public const string Info = "info";
    public const string Financial = "financial";
    public const string Quiz = "quiz";
    public const string Agreement = "agreement";
    public const string Document = "document";
    public const string KycForm = "kyc";

    public static readonly string[] All = {
        Started,
        Info,
        Financial,
        Quiz,
        Agreement,
        Document
    };
    public static readonly string[] NoQuiz = {
        Started,
        Info,
        Financial,
        Agreement,
        Document
    };
}
