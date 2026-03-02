using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using FluentValidation;
using Newtonsoft.Json;

namespace Bacera.Gateway;

partial class Lead
{
    public sealed class AutoAssignInfo
    {
        public long AutoAssignAccountUid { get; set; }
        public bool Enabled { get; set; }
        public string ToJson() => JsonConvert.SerializeObject(this);
        public static AutoAssignInfo FromJson(string json) => JsonConvert.DeserializeObject<AutoAssignInfo>(json)!;
    }
}