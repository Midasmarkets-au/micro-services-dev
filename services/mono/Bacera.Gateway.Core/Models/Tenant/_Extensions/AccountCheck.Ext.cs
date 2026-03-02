using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = AccountCheck;

public partial class AccountCheck
{
    public List<long> GetAccountNumbers() => Utils.JsonDeserializeObjectWithDefault<List<long>>(AccountNumberContent);
}