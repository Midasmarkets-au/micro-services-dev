using Bacera.Gateway.Core.Types;
using HashidsNet;
using Newtonsoft.Json;

namespace Bacera.Gateway;

using M = Address;

public partial class Address
{
    public CopyModel ToCopyModel() => new()
    {
        Name = Name,
        CCC = CCC,
        Phone = Phone,
        Country = Country,
        Content = Content,
    };

    public string ToCopyString() => JsonConvert.SerializeObject(ToCopyModel());
}