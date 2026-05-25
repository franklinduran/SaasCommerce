using System.Text;

namespace SaasCommerce.SharedKernel;

/// <summary>
/// Lightweight CSV builder — no external dependencies.
/// Headers and rows are RFC 4180-compliant (fields with commas or quotes are double-quoted).
/// </summary>
public static class CsvBuilder
{
  public static byte[] Build(IReadOnlyCollection<string> headers, IEnumerable<IReadOnlyCollection<string?>> rows)
  {
    ArgumentNullException.ThrowIfNull(headers);
    ArgumentNullException.ThrowIfNull(rows);

    var sb = new StringBuilder();

    // Header row
    sb.AppendLine(string.Join(",", headers.Select(Escape)));

    // Data rows
    foreach (var row in rows)
    {
      sb.AppendLine(string.Join(",", row.Select(Escape)));
    }

    return Encoding.UTF8.GetBytes(sb.ToString());
  }

  private static string Escape(string? value)
  {
    if (string.IsNullOrEmpty(value))
    {
      return string.Empty;
    }

    // RFC 4180: if field contains comma, double-quote, or newline → wrap in double-quotes
    if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
    {
      return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    return value;
  }
}
