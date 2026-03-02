using Hangfire;

namespace Bacera.Gateway.Web.Services;

public class HangfireWrapper : IHangfireWrapper
{
    public IBackgroundJobClient BackgroundJobClient { get; }

    public HangfireWrapper(IBackgroundJobClient backgroundJobClient)
    {
        BackgroundJobClient = backgroundJobClient;
    }
}