using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class VerificationItem
{
    public virtual object? Data
    {
        get
        {
            try
            {
                return JsonConvert.DeserializeObject<object>(Content) ?? new { };
            }
            catch
            {
                return new { };
            }
        }
    }
}