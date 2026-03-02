using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

public class ShopService
{
    private readonly TenantDbContext _tenantCtx;

    public ShopService(TenantDbContext tenantCtx)
    {
        _tenantCtx = tenantCtx;
    }

    public async Task<Result<List<Product.TenantResponseModel>, Product.Criteria>> ProductTenantQueryAsync(
        Product.Criteria criteria)
    {
        var items = await _tenantCtx.Products
            .PagedFilterBy(criteria)
            .ToTenantResponseModel()
            .ToListAsync();

        return Result<List<Product.TenantResponseModel>, Product.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Order.TenantResponseModel>, Order.Criteria>> OrderTenantQueryAsync(
        Order.Criteria criteria)
    {
        var items = await _tenantCtx.Orders
            .PagedFilterBy(criteria)
            .ToTenantResponseModels()
            .ToListAsync();

        return Result<List<Order.TenantResponseModel>, Order.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Order.ResponseModel>, Order.Criteria>> OrderQueryAsync(
        Order.Criteria criteria)
    {
        var items = await _tenantCtx.Orders
            .PagedFilterBy(criteria)
            .ToResponseModels()
            .ToListAsync();

        return Result<List<Order.ResponseModel>, Order.Criteria>.Of(items, criteria);
    }

    public async Task<Result<List<Product.ResponseModel>, Product.Criteria>> ProductQueryAsync(
        Product.Criteria criteria)
    {
        var items = await _tenantCtx.Products
            .PagedFilterBy(criteria)
            .ToResponseModel()
            .ToListAsync();

        return Result<List<Product.ResponseModel>, Product.Criteria>.Of(items, criteria);
    }
}