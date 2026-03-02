using Hangfire;

namespace Bacera.Gateway.Web;
public interface IHangfireWrapper
{
    IBackgroundJobClient BackgroundJobClient { get; }
}