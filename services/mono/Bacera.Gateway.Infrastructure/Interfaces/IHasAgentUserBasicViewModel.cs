using Bacera.Gateway.Agent;
using Bacera.Gateway.ViewModels.Parent;

namespace Bacera.Gateway.Interfaces;

public interface IHasAgentUserBasicViewModel
{
    UserBasicForParentViewModel User { get; set; }
}