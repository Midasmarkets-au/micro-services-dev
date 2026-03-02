namespace Bacera.Gateway;

partial class RebateDirectSchemaItem
{
    public class CreateSpec
    {
        public string SymbolCode { get; set; } = null!;
        public decimal Rate { get; set; }
        public decimal Pips { get; set; }
        public decimal Commission { get; set; }

        public decimal Total => Rate + Pips + Commission;

        public static CreateSpec Build(long rate, long pip, long commission, string symbolCode)
            => new()
            {
                Rate = rate,
                Pips = pip,
                Commission = commission,
                SymbolCode = symbolCode
            };
    }

    public class CheckAmountSpec
    {
        public long Pips { get; set; }
        public long Commission { get; set; }
        public long Rate { get; set; }
        public string SymbolCode { get; set; } = null!;
        public long TradeServiceId { get; set; }
        public int Volume { get; set; }
        public CurrencyTypes SourceCurrencyId { get; set; }

        public RebateDirectSchemaItem ToRebateDirectSchemaItem() => new()
        {
            Rate = Rate,
            Pips = Pips,
            Commission = Commission,
            SymbolCode = SymbolCode
        };
    }

    public class UpdateSpec
    {
        public decimal Rate { get; set; }
        public decimal Pips { get; set; }
        public decimal Commission { get; set; }
    }
}