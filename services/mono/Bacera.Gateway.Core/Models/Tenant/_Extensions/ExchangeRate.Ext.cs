using System.ComponentModel.DataAnnotations;

namespace Bacera.Gateway;

partial class ExchangeRate
{
    public ExchangeRate()
    {
        Name = string.Empty;
    }

    public bool IsEmpty() => Id == 0;

    public static ExchangeRate Build(CurrencyTypes from, CurrencyTypes to, decimal buyingRate, decimal sellingRate = 0m,
        decimal adjustRate = 0m, string name = "")
        => new()
        {
            Name = name,
            FromCurrencyId = (int)from,
            ToCurrencyId = (int)to,
            BuyingRate = buyingRate,
            SellingRate = sellingRate,
            AdjustRate = adjustRate,
            UpdatedOn = DateTime.UtcNow,
        };

    public sealed class ResponseModel
    {
        public bool IsEmpty() => Id == 0;
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public decimal BuyingRate { get; set; }
        public decimal SellingRate { get; set; }
        public decimal AdjustRate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string FromCurrencyName { get; set; } = string.Empty;
        public string ToCurrencyName { get; set; } = string.Empty;
        public string FromCurrencyCode { get; set; } = string.Empty;
        public string ToCurrencyCode { get; set; } = string.Empty;
    }

    public sealed class BasicViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int FromCurrencyId { get; set; }
        public int ToCurrencyId { get; set; }
        public decimal Rate { get; set; }
        public string FromCurrencyCode { get; set; } = string.Empty;
        public string ToCurrencyCode { get; set; } = string.Empty;
    }

    public sealed class UpdateSpec
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public decimal BuyingRate { get; set; }
        public decimal SellingRate { get; set; }
        public decimal AdjustRate { get; set; }
    }

    public sealed class CreateSpec
    {
        [Required] public string Name { get; set; } = null!;
        [Required] public CurrencyTypes FromCurrencyId { get; set; }
        [Required] public CurrencyTypes ToCurrencyId { get; set; }
        [Required] public decimal BuyingRate { get; set; }
        [Required] public decimal SellingRate { get; set; }
        [Required] public decimal AdjustRate { get; set; }
    }
}

public static class ExchangeRateExtensions
{
    public static IQueryable<ExchangeRate.ResponseModel> ToClientResponseModels(
        this IQueryable<ExchangeRate> query)
        => query.Select(x => new ExchangeRate.ResponseModel
        {
            Id = x.Id,
            Name = x.Name,
            FromCurrencyId = x.FromCurrencyId,
            ToCurrencyId = x.ToCurrencyId,
            BuyingRate = x.BuyingRate,
            SellingRate = x.SellingRate,
            AdjustRate = x.AdjustRate,
            CreatedOn = x.CreatedOn,
            UpdatedOn = x.UpdatedOn,
            FromCurrencyName = x.FromCurrency.Name,
            ToCurrencyName = x.ToCurrency.Name,
            FromCurrencyCode = x.FromCurrency.Code,
            ToCurrencyCode = x.ToCurrency.Code,
        });

    public static IQueryable<ExchangeRate.BasicViewModel> ToBasicViewModel(
        this IQueryable<ExchangeRate> query)
        => query.Select(x => new ExchangeRate.BasicViewModel()
        {
            Name = x.Name,
            FromCurrencyId = x.FromCurrencyId,
            ToCurrencyId = x.ToCurrencyId,
            Rate = x.BuyingRate,
            FromCurrencyCode = x.FromCurrency.Code,
            ToCurrencyCode = x.ToCurrency.Code,
        });
}