using Hangfire;

namespace Bacera.Gateway.Web.Services;

public class EmptyHangfireWrapper : IHangfireWrapper
{
    public IBackgroundJobClient BackgroundJobClient => new BackgroundJobClient(JobStorage.Current);
}