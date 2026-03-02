using Bacera.Gateway.Core.Types;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

using M = Crypto;

public partial class Crypto
{
    public sealed class CreateSpec
    {
        private string _address = null!;

        public string Address
        {
            get => _address;
            set => _address = value.Trim();
        }

        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}