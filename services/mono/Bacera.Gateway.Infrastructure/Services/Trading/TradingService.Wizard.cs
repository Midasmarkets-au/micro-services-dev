using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway;

public partial class TradingService
{
    public async Task<Supplement.AccountWizard> GetAccountWizardAsync(long accountId)
    {
        var supplement = await dbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.AccountWizard)
            .Where(x => x.RowId == accountId)
            .FirstOrDefaultAsync();

        if (supplement != null) return Supplement.AccountWizard.FromJson(supplement.Data);

        supplement = Supplement.Build(SupplementTypes.AccountWizard, accountId,
            new Supplement.AccountWizard().ToJson());
        dbContext.Supplements.Add(supplement);
        await dbContext.SaveChangesAsync();

        return Supplement.AccountWizard.FromJson(supplement.Data);
    }

    public async Task<Supplement.AccountWizard> UpdateAccountWizardAsync(long accountId, Supplement.AccountWizard wizard)
    {
        var supplement = await dbContext.Supplements
            .Where(x => x.Type == (int)SupplementTypes.AccountWizard)
            .Where(x => x.RowId == accountId)
            .FirstOrDefaultAsync();

        if (supplement == null)
        {
            supplement = Supplement.Build(SupplementTypes.AccountWizard, accountId, wizard.ToJson());
            dbContext.Supplements.Add(supplement);
        }
        else
        {
            supplement.SetDataObject(wizard);
            dbContext.Supplements.Update(supplement);
        }

        await dbContext.SaveChangesAsync();
        return wizard;
    }
}