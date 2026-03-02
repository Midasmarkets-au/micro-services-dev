using System.Text.Encodings.Web;

namespace Bacera.Gateway.Core.Types;

public static class PaymentResponseActionTypes
{
    public const string None = "None";
    public const string Post = "Post";
    public const string Get = "Get";
    public const string Redirect = "Redirect";
    public const string QrCode = "QrCode";
    public const string PayPal = "PayPal";
    public const string PopupInstruction = "PopupInstruction";
}

