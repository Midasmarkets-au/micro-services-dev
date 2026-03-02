using System.ComponentModel.DataAnnotations;
using Bacera.Gateway.Core.Types;
using Newtonsoft.Json;

namespace Bacera.Gateway;

public partial class Event
{
    public sealed class UpdateSpec
    {
        [Required] public DateTime ApplyStartOn { get; set; }
        [Required] public DateTime ApplyEndOn { get; set; }
        [Required] public DateTime StartOn { get; set; }
        [Required] public DateTime EndOn { get; set; }
        [Required] public List<string> AccessRoles { get; set; } = [];
        [Required] public List<int> AccessSites { get; set; } = [];
        [Required] public string Key { get; set; } = "";
    }

    public sealed class UpdateLanguageSpec
    {
        [Required] public string Name { get; set; } = "";
        [Required] public string Title { get; set; } = "";
        public Dictionary<string, string> Images { get; set; } = new();
        public string? Description { get; set; }
        public string? Term { get; set; }
        public Instruction Instruction { get; set; } = new();
    }

    public sealed class CreateWithLanguageSpec
    {
        [Required] public DateTime ApplyStartOn { get; set; }
        [Required] public DateTime ApplyEndOn { get; set; }
        [Required] public DateTime StartOn { get; set; }
        [Required] public DateTime EndOn { get; set; }
        [Required] public List<string> AccessRoles { get; set; } = [];
        [Required] public List<int> AccessSites { get; set; } = [];
        [Required] public string Key { get; set; } = "";

        [Required] public string Title { get; set; } = "";
        [Required] public string Language { get; set; } = "";
        [Required] public string Name { get; set; } = "";
        public Dictionary<string, string> Images { get; set; } = new();
        public string? Description { get; set; } = "";
        public string? Term { get; set; }
        public Instruction Instruction { get; set; } = new();

        public Event ToEntity() => new()
        {
            ApplyStartOn = ApplyStartOn,
            ApplyEndOn = ApplyEndOn,
            StartOn = StartOn,
            EndOn = EndOn,
            AccessRoles = JsonConvert.SerializeObject(AccessRoles),
            AccessSites = JsonConvert.SerializeObject(AccessSites),
            Key = Key,
            Status = (short)EventStatusTypes.Draft,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
            EventLanguages = new List<EventLanguage>
            {
                new()
                {
                    Language = Language,
                    Name = Name,
                    Title = Title,
                    Description = Description,
                    Images = JsonConvert.SerializeObject(Images),
                    Instruction = JsonConvert.SerializeObject(Instruction),
                    Term = Term,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow
                }
            }
        };
    }

    public sealed class Instruction
    {
        [Required] public PointsRule PointsRule { get; set; } = new();
    }

    public sealed class PointsRule
    {
        [Required] public string All { get; set; } = "";
        [Required] public string Agent { get; set; } = "";
        [Required] public string Sales { get; set; } = "";
        [Required] public string Client { get; set; } = "";
    }
}