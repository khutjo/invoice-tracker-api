using System.Reflection;
using System.Text;

namespace InvoiceTracker.Business.Infrastructure.Persistence.ReferenceData;

/// <summary>
/// Builds the INSERT statements that seed postal_codes and banks from the embedded .tsv files
/// (sources: scripts/PostalCodes.txt from SAPO and scripts/BranchCodes.txt).
/// Called from the AddReferenceData migration so a fresh database gets the data with the tables.
/// </summary>
public static class ReferenceDataSeed
{
    // SQL Server allows at most 1000 rows in one VALUES list
    private const int BatchSize = 1000;

    public static string PostalCodesSql()
    {
        return InsertSql("postal_codes", ["code", "suburb", "town", "kind"], ReadRows("postal-codes.tsv"));
    }

    public static string BanksSql()
    {
        return InsertSql("banks", ["name", "universal_branch_code"], ReadRows("banks.tsv"));
    }

    private static string InsertSql(string table, string[] columns, IReadOnlyList<string[]> rows)
    {
        var sql = new StringBuilder();

        foreach (var batch in rows.Chunk(BatchSize))
        {
            sql.Append("INSERT INTO ").Append(table).Append(" (").Append(string.Join(", ", columns)).AppendLine(") VALUES");
            sql.AppendJoin(",\n", batch.Select(row => "(" + string.Join(", ", row.Select(Literal)) + ")"));
            sql.AppendLine(";");
        }

        return sql.ToString();
    }

    private static string Literal(string value)
    {
        return "N'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";
    }

    /// <summary>Tab-separated, first line is the header.</summary>
    private static List<string[]> ReadRows(string fileName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ReferenceData.{fileName}")
            ?? throw new InvalidOperationException($"Embedded reference data '{fileName}' was not found.");
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var rows = new List<string[]>();
        reader.ReadLine();
        while (reader.ReadLine() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                rows.Add(line.Split('\t'));
            }
        }

        return rows;
    }
}
