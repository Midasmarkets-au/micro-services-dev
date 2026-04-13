using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace Bacera.Gateway.Web.Areas.Tenant.Helpers;

public sealed class BlacklistUploadResult
{
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int SkippedEmptyOrInvalid { get; set; }
    public int SkippedDuplicate { get; set; }
    public int SkippedParseError { get; set; }
    public List<string> Errors { get; } = new();
}

public sealed class BlacklistExcelColumnMap
{
    public int? NameCol { get; set; }
    public int? PhoneCol { get; set; }
    public int? IdNumberCol { get; set; }
    public int? EmailCol { get; set; }
    public int? IpCol { get; set; }
    public int? ReasonCol { get; set; }
}

/// <summary>Reads ChipPay-style blacklist workbooks (headers bilingual).</summary>
public static class BlacklistExcelImportHelper
{
    private const int MaxNoteLength = 255;
    /// <summary>Max sample rows kept in <see cref="BlacklistUploadResult.Errors"/> for client review.</summary>
    private const int MaxErrors = 200;

    private static readonly Regex Ipv4Regex = new(
        @"\b(?:(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\b",
        RegexOptions.Compiled);

    /// <summary>Caller must keep <paramref name="workbook"/> alive while using the worksheet.</summary>
    public static bool TryGetWorksheet(XLWorkbook workbook, string? sheetName, int sheetIndex, out IXLWorksheet worksheet,
        out string error)
    {
        error = "";
        if (!string.IsNullOrWhiteSpace(sheetName))
        {
            var sn = sheetName.Trim();
            var byName = workbook.Worksheets
                .FirstOrDefault(w => string.Equals(w.Name, sn, StringComparison.OrdinalIgnoreCase));
            if (byName == null)
            {
                error = "__SHEET_NOT_FOUND__";
                worksheet = null!;
                return false;
            }

            worksheet = byName;
            return true;
        }

        if (sheetIndex < 0 || sheetIndex >= workbook.Worksheets.Count)
        {
            error = "__SHEET_INDEX_INVALID__";
            worksheet = null!;
            return false;
        }

        worksheet = workbook.Worksheet(sheetIndex + 1);
        return true;
    }

    public static int FindHeaderRow(IXLWorksheet ws)
    {
        var range = ws.RangeUsed();
        if (range == null) return 0;
        var firstRow = range.FirstRow().RowNumber();
        var lastRow = Math.Min(range.LastRow().RowNumber(), firstRow + 30);
        for (var r = firstRow; r <= lastRow; r++)
        {
            var nonEmpty = 0;
            foreach (var cell in ws.Row(r).CellsUsed())
            {
                if (!string.IsNullOrWhiteSpace(CellText(cell))) nonEmpty++;
            }

            if (nonEmpty >= 3) return r;
        }

        return firstRow;
    }

    public static bool IsIgnoredHeader(string normalized)
    {
        if (string.IsNullOrEmpty(normalized)) return true;
        return normalized.Contains("回報日期", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("reported date", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("区码", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("區碼", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("dialing number", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("dialing", StringComparison.OrdinalIgnoreCase);
    }

    public static BlacklistExcelColumnMap ResolveColumns(IXLWorksheet ws, int headerRow)
    {
        var map = new BlacklistExcelColumnMap();
        var claimed = new HashSet<int>();

        foreach (var cell in ws.Row(headerRow).CellsUsed())
        {
            var col = cell.Address.ColumnNumber;
            if (claimed.Contains(col)) continue;
            var raw = CellText(cell).Trim();
            if (string.IsNullOrEmpty(raw)) continue;

            var norm = NormalizeHeader(raw);
            if (IsIgnoredHeader(norm)) continue;

            var role = ClassifyHeader(norm);
            if (role == null) continue;

            switch (role.Value)
            {
                case ColRole.Name when map.NameCol == null:
                    map.NameCol = col;
                    claimed.Add(col);
                    break;
                case ColRole.Phone when map.PhoneCol == null:
                    map.PhoneCol = col;
                    claimed.Add(col);
                    break;
                case ColRole.IdNumber when map.IdNumberCol == null:
                    map.IdNumberCol = col;
                    claimed.Add(col);
                    break;
                case ColRole.Email when map.EmailCol == null:
                    map.EmailCol = col;
                    claimed.Add(col);
                    break;
                case ColRole.Ip when map.IpCol == null:
                    map.IpCol = col;
                    claimed.Add(col);
                    break;
                case ColRole.Reason when map.ReasonCol == null:
                    map.ReasonCol = col;
                    claimed.Add(col);
                    break;
            }
        }

        return map;
    }

    public static string TruncateNote(string note)
    {
        if (string.IsNullOrEmpty(note)) return "";
        if (note.Length <= MaxNoteLength) return note;
        return note[..(MaxNoteLength - 1)] + "…";
    }

    public static string TruncateField(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLen ? value : value[..maxLen];
    }

    public static string NormalizePhone(string phone) =>
        string.IsNullOrEmpty(phone) ? "" : new string(phone.Where(c => !char.IsWhiteSpace(c)).ToArray());

    public static bool TryNormalizeEmailKey(string email, out string key)
    {
        key = "";
        if (string.IsNullOrWhiteSpace(email)) return false;
        key = email.Trim().ToLowerInvariant();
        return true;
    }

    public static bool TryCanonicalIp(string raw, out string canonical)
    {
        canonical = "";
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var trimmed = raw.Trim();
        if (!IPAddress.TryParse(trimmed, out var ip)) return false;

        if (ip.AddressFamily == AddressFamily.InterNetworkV6 && trimmed.Contains('%'))
        {
            canonical = ip.ToString().ToLowerInvariant();
            return true;
        }

        canonical = ip.ToString();
        return true;
    }

    /// <summary>Extract IPv4 and IPv6 from messy spreadsheet text.</summary>
    public static IReadOnlyList<string> ExtractIps(string cellText)
    {
        if (string.IsNullOrWhiteSpace(cellText)) return Array.Empty<string>();

        var sb = new StringBuilder(cellText.Trim());
        sb.Replace('／', '/');
        sb.Replace('，', ',');
        sb.Replace('、', ',');
        sb.Replace('：', ':');
        sb.Replace('；', ';');
        sb.Replace('（', ' ');
        sb.Replace('）', ' ');
        sb.Replace('(', ' ');
        sb.Replace(')', ' ');
        var normalized = sb.ToString();

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var results = new List<string>();

        foreach (Match m in Ipv4Regex.Matches(normalized))
        {
            if (TryCanonicalIp(m.Value, out var c) && seen.Add(c))
                results.Add(c);
        }

        static string[] SplitSeps(string s) =>
            s.Split(new[] { '/', ',', ';', '\n', '\r', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in SplitSeps(normalized))
        {
            var p = part.Trim();
            if (p.Length == 0) continue;
            if (TryCanonicalIp(p, out var c) && seen.Add(c))
                results.Add(c);
        }

        return results;
    }

    public static void AddError(BlacklistUploadResult result, string message)
    {
        if (result.Errors.Count >= MaxErrors) return;
        result.Errors.Add(message);
    }

    /// <summary>Safe one-line preview of a cell for upload error messages.</summary>
    public static string FormatCellForError(string? raw, int maxChars = 200)
    {
        if (raw == null || string.IsNullOrWhiteSpace(raw)) return "(empty)";
        var t = Regex.Replace(raw.Trim(), @"[\r\n]+", " ");
        if (t.Length == 0) return "(empty)";
        if (t.Length > maxChars) return t[..maxChars] + "…";
        return t;
    }

    private static string NormalizeHeader(string header)
    {
        var t = header.Trim().ToLowerInvariant();
        var split = t.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', split);
    }

    private enum ColRole
    {
        Name,
        Phone,
        IdNumber,
        Email,
        Ip,
        Reason
    }

    private static ColRole? ClassifyHeader(string h)
    {
        if (h.Contains("邮箱", StringComparison.OrdinalIgnoreCase) || h.Contains("电邮", StringComparison.OrdinalIgnoreCase)
                                                                      || h.Contains("email", StringComparison.OrdinalIgnoreCase)
                                                                      || h.Contains("e-mail", StringComparison.OrdinalIgnoreCase))
            return ColRole.Email;

        if (h.Contains("身份证", StringComparison.OrdinalIgnoreCase) || h.Contains("身分證", StringComparison.OrdinalIgnoreCase)
                                                                    || h.Contains("证件", StringComparison.OrdinalIgnoreCase)
                                                                    || h.Contains("id number", StringComparison.OrdinalIgnoreCase)
                                                                    || h.EndsWith(" id", StringComparison.OrdinalIgnoreCase))
            return ColRole.IdNumber;

        var phoneish = h.Contains("手机", StringComparison.OrdinalIgnoreCase)
                       || h.Contains("mobile", StringComparison.OrdinalIgnoreCase)
                       || (h.Contains("电话", StringComparison.OrdinalIgnoreCase)
                           && !h.Contains("区", StringComparison.OrdinalIgnoreCase)
                           && !h.Contains("區", StringComparison.OrdinalIgnoreCase));
        if (phoneish)
            return ColRole.Phone;

        if (h.Contains("姓名", StringComparison.OrdinalIgnoreCase) || h.Equals("name", StringComparison.OrdinalIgnoreCase)
                                                                   || (h.Contains("name", StringComparison.OrdinalIgnoreCase) && !h.Contains("user name", StringComparison.OrdinalIgnoreCase)
                                                                                                                                 && !h.Contains("username", StringComparison.OrdinalIgnoreCase)))
            return ColRole.Name;

        if (h.Contains("ip", StringComparison.OrdinalIgnoreCase) || h.Contains("ip地址", StringComparison.OrdinalIgnoreCase))
            return ColRole.Ip;

        if (h.Contains("原因", StringComparison.OrdinalIgnoreCase) || h.Contains("reason", StringComparison.OrdinalIgnoreCase)
                                                                    || h.Contains("blocked", StringComparison.OrdinalIgnoreCase)
                                                                    || h.Contains("黑名單", StringComparison.OrdinalIgnoreCase)
                                                                    || h.Contains("黑名单", StringComparison.OrdinalIgnoreCase))
            return ColRole.Reason;

        return null;
    }

    public static string CellText(IXLCell cell)
    {
        if (cell.IsEmpty()) return "";
        return cell.GetFormattedString().Trim();
    }
}
