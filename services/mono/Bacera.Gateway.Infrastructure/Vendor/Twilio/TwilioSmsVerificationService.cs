using Bacera.Gateway.Interfaces;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace Bacera.Gateway.Services.Twilio;

public class TwilioSmsVerificationService : ISmsVerification
{
    private readonly string _serviceSid;
    private const int RequestPerMin = 2;
    private readonly List<Tuple<string, DateTime>> _requestLimit;

    public TwilioSmsVerificationService(string accountSid, string authToken, string serviceSid)
    {
        _serviceSid = serviceSid;
        _requestLimit = new List<Tuple<string, DateTime>>();
        TwilioClient.Init(accountSid, authToken);
    }

    public async Task<bool> Verification(string to, string limitKey)
    {
        if (HasReachedLimit(limitKey)) return false;
        _requestLimit.Add(Tuple.Create(limitKey, DateTime.UtcNow));
        return await VerificationWithoutLimit(to);
    }

    public async Task<bool> VerificationWithoutLimit(string to)
    {
        var options = new CreateVerificationOptions(_serviceSid, to, "sms");
        var call = await VerificationResource.CreateAsync(options);
        return !string.IsNullOrEmpty(call.Sid);
    }

    public async Task<Tuple<bool, string>> VerificationCheck(string to, string code)
    {
        var options = new CreateVerificationCheckOptions(_serviceSid)
        {
            Code = code.Trim(),
            To = to.Trim()
        };
        var call = await VerificationCheckResource.CreateAsync(options);
        return call?.Valid == true ? Tuple.Create(true, call.To) : Tuple.Create(false, string.Empty);
    }

    public bool HasReachedLimit(string limitKey)
    {
        limitKey = limitKey.Trim();
        var current = DateTime.UtcNow;
        var offset = new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, 0);
        _requestLimit.RemoveAll(x => x.Item2 < offset);
        return _requestLimit.Count(x => x.Item1 == limitKey) >= RequestPerMin;
    }
}