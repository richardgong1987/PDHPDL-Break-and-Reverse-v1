using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace cAlgo.Robots;

public class PdhpdlTradeCsvLogger
{
    private const string FileName = "pdhpdl-trades.csv";

    private readonly string _filePath;

    public PdhpdlTradeCsvLogger()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _filePath = Path.Combine(documentsPath, FileName);

        EnsureFileExists();
    }

    public string FilePath => _filePath;

    public void Append(PdhpdlTradeCsvRecord record)
    {
        if (record == null)
            return;

        int nextId = GetNextId();

        string line = string.Join(
            ",",
            Escape(nextId.ToString(CultureInfo.InvariantCulture)),
            Escape(record.Side),
            Escape(record.KeyLevel),
            Escape(record.Signal),
            Escape(record.CloseEntryResult),
            Escape(record.Pullback25Result),
            Escape(record.Pullback382Result),
            Escape(record.Pullback50Result),
            Escape(record.Comment),
            Escape(record.Symbol),
            Escape(record.TimeFrame),
            Escape(record.EntryTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
            Escape(record.EntryPrice.ToString(CultureInfo.InvariantCulture)),
            Escape(record.StopPrice.ToString(CultureInfo.InvariantCulture)),
            Escape(record.Tp1Price.ToString(CultureInfo.InvariantCulture)),
            Escape(record.Tp2Price.ToString(CultureInfo.InvariantCulture)),
            Escape(record.RiskPrice.ToString(CultureInfo.InvariantCulture)),
            Escape(record.VolumeInUnits.ToString(CultureInfo.InvariantCulture))
        );

        File.AppendAllText(_filePath, line + Environment.NewLine, Encoding.UTF8);
    }

    private void EnsureFileExists()
    {
        if (File.Exists(_filePath))
            return;

        string header = string.Join(
            ",",
            "编号",
            "多空",
            "关键位",
            "信号",
            "收线入场",
            "回撤25入场",
            "回撤38.2入场",
            "回撤50入场",
            "备注",
            "Symbol",
            "TimeFrame",
            "EntryTime",
            "EntryPrice",
            "StopPrice",
            "TP1Price",
            "TP2Price",
            "RiskPrice",
            "VolumeInUnits"
        );

        File.WriteAllText(_filePath, header + Environment.NewLine, Encoding.UTF8);
    }

    private int GetNextId()
    {
        if (!File.Exists(_filePath))
            return 1;

        int lineCount = 0;

        foreach (string line in File.ReadLines(_filePath))
        {
            if (!string.IsNullOrWhiteSpace(line))
                lineCount++;
        }

        return Math.Max(1, lineCount);
    }

    private static string Escape(string value)
    {
        if (value == null)
            return string.Empty;

        bool mustQuote =
            value.Contains(",") ||
            value.Contains("\"") ||
            value.Contains("\n") ||
            value.Contains("\r");

        if (!mustQuote)
            return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}