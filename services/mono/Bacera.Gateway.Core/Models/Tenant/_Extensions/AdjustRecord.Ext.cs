using Bacera.Gateway.Core.Types;

namespace Bacera.Gateway;

public partial class AdjustRecord
{
    public static AdjustRecord ReadFromCsvLine(string line, AdjustTypes type)
    {
        var fields = line.Split(',');
        var comment = (Enum.GetName(typeof(AdjustTypes), type) ?? "").ToLower() + " " + fields[2];
        // uppercase the first letter of the comment
        comment = char.ToUpper(comment[0]) + comment[1..];

        return new AdjustRecord
        {
            AccountNumber = long.Parse(fields[0]),
            Amount = (long)Math.Round(decimal.Parse(fields[1]) * 100 * 10000, 0),
            Comment = comment,
        };
    }
}