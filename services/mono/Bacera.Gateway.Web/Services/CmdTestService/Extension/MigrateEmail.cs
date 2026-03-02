using Bacera.Gateway.Context;
using Bacera.Gateway.Web.BackgroundJobs.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Services;

public partial class CmdTestService
{
    private async Task TestMigrateEmailTemplate()
    {
        var names = new[]
        {
            "VerificationCode",
        };
        var pool = _serviceProvider.GetRequiredService<MyDbContextPool>();

        await using var bviCtx = pool.CreateTenantDbContext(10000);

        var items = await bviCtx.Topics
            .Where(x => names.Contains(x.Title))
            .Include(x => x.TopicContents)
            .ToListAsync();

        var tasks = new List<TenantDbContext>
        {
            pool.CreateTenantDbContext(1),
            pool.CreateTenantDbContext(10004),
            GetCtx("portal_au"),
            GetCtx("portal_mn"),
            GetCtx("portal_bvi"),
            GetCtx("portal_vn")
        }.Select(async ctx =>
        {
            await MigrateEmailTemplateHelper(ctx, items);
            await ctx.DisposeAsync();
        });

        await Task.WhenAll(tasks);
    }

    private static async Task MigrateEmailTemplateHelper(TenantDbContext ctx, List<Topic> items)
    {
        foreach (var item in items)
        {
            var existing = await ctx.Topics
                .Where(x => x.Title == item.Title)
                .Include(x => x.TopicContents)
                .ToListAsync();
            ctx.TopicContents.RemoveRange(existing.SelectMany(x => x.TopicContents));
            ctx.Topics.RemoveRange(existing);

            item.Id = 0;
            foreach (var iit in item.TopicContents)
            {
                iit.Id = 0;
                iit.TopicId = 0;
            }

            ctx.Topics.Add(item);
            await ctx.SaveChangesAsync();
        }
    }

    private static TenantDbContext GetCtx(string db)
    {
        var connectionStr =
            $"Database={db}";
        var builder = new DbContextOptionsBuilder<TenantDbContext>();
        builder.UseNpgsql(connectionStr);
        return new TenantDbContext(builder.Options);
    }
}