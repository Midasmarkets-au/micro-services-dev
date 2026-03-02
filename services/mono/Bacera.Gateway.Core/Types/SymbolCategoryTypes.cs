namespace Bacera.Gateway
{
    public enum SymbolCategoryTypes : int
    {
        Forex = 1,
        PreciousMetal = 2,
        Commodity = 3,
        Index = 4,
        Stock = 6,
        DForex = 7,
        Crypto = 8,
    }

    public static class SymbolCategoryTypesExtension
    {
        public static string GetName(this SymbolCategoryTypes type) =>
            type switch
            {
                SymbolCategoryTypes.Forex => "Forex",
                SymbolCategoryTypes.PreciousMetal => "Precious Metal",
                SymbolCategoryTypes.Commodity => "Commodity",
                SymbolCategoryTypes.Index => "Index",
                SymbolCategoryTypes.Stock => "Stock",
                SymbolCategoryTypes.DForex => "D.Forex",
                SymbolCategoryTypes.Crypto => "Crypto",
                _ => "Unknown"
            };
    }
}
