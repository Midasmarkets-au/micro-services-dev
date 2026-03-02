using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Infrastructure)]
[Trait(TraitTypes.Parties, TraitTypes.Value.FirstParty)]

public class SymbolTests : Startup
{
    public SymbolTests()
    {
        var task = TenantDbContext.SeedSymbolsAsync();
        task.Wait();
    }

    [Fact]
    public async Task GetSymbolTest()
    {
        var item = await TenantDbContext.Symbols
            .Include(x => x.Category)
            .Include(x => x.SymbolInfo)
            .FirstOrDefaultAsync(x => x.Id == (int)SymbolTypes.USDCAD);

        item.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetSymbolDetailsTest()
    {
        var item = await TenantDbContext.SymbolInfos
            .FirstOrDefaultAsync(x => x.Id == (int)SymbolTypes.USDCAD);
        item.ShouldNotBeNull();

        var languages = new Dictionary<string, Dictionary<string, string>>
        {
            {
                LanguageTypes.English, new Dictionary<string, string>
                {
                    { "DETAILS", "USDT Details" },
                    { "Line2", "Line2" },
                }
            },
            {
                LanguageTypes.Chinese, new Dictionary<string, string>
                {
                    { "DETAILS", "中文内容" },
                    { "Line2", "中文内容2" },
                }
            }
        };
        var details = new SymbolInfo.Detail
        {
            Languages = languages
        };

        item.Description = JsonConvert.SerializeObject(details);
        TenantDbContext.SymbolInfos.Update(item);
        await TenantDbContext.SaveChangesAsync();

        var symbol = await TenantDbContext.Symbols
            .Include(x => x.Category)
            .Include(x => x.SymbolInfo)
            .FirstOrDefaultAsync(x => x.Id == (int)SymbolTypes.USDCAD);

        item.ShouldNotBeNull();
        var model = symbol?.ToResponseModel();
        model.ShouldNotBeNull();

        model.Languages.Any(x => x.Key == LanguageTypes.English).ShouldBeTrue();
        model.Languages.Any(x => x.Key == LanguageTypes.Chinese).ShouldBeTrue();
    }

    [Fact]
    public void SymbolInfoMergeTest()
    {
        var original = new SymbolInfo.Detail
        {
            Languages = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    LanguageTypes.English, new Dictionary<string, string>
                    {
                        { "DETAILS", "ENG 1" },
                        { "Line2", "ENG 2" },
                        { "Line3", "ENG 3" },
                    }
                },
                {
                    LanguageTypes.Chinese, new Dictionary<string, string>
                    {
                        { "DETAILS", "ZH 1" },
                        { "Line2", "ZH 2" },
                        { "Line3", "ZH 3" },
                    }
                }
            }
        };
        var input = new SymbolInfo.Detail
        {
            Languages = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    LanguageTypes.English, new Dictionary<string, string>
                    {
                        { "DETAILS", "ENG A" },
                        { "Line2", "ENG B" },
                    }
                },
                {
                    LanguageTypes.Chinese, new Dictionary<string, string>
                    {
                        { "DETAILS", "ZH A" },
                        { "Line2", "ZH B" },
                        { "Line5", "ZH #" },
                    }
                }
            }
        };

        var org = original.Languages;
        var update = input.Languages;
        foreach (var item in update)
        {
            if (!org.ContainsKey(item.Key))
            {
                org[item.Key] = new Dictionary<string, string>();
            }

            foreach (var val in item.Value)
            {
                org[item.Key][val.Key] = val.Value;
            }
        }

        org.Count.ShouldBeGreaterThan(0);
        org[LanguageTypes.Chinese]["Line2"].ShouldBe("ZH B");
        org[LanguageTypes.Chinese]["Line5"].ShouldBe("ZH #");
    }
}