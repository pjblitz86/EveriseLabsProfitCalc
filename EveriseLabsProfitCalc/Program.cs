using EveriseLabsProfitCalc;
using System.Globalization;

string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
string filePath = Path.Combine(projectRoot, "data.csv");

if (!File.Exists(filePath))
{
    Console.WriteLine("CSV file 'data.csv' not found in the project directory.");
    return;
}

Console.WriteLine($"Csv file found: {filePath}");

Console.Write("Enter Client Name: ");
string client = Console.ReadLine();

Console.Write("Enter Date (yyyy-MM-dd): ");
if (!DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime targetDate))
{
    Console.WriteLine("Invalid date format. Use yyyy-MM-dd.");
    return;
}

List<Trades> trades = ReadTradesFromCsv(filePath);
foreach (var trade in trades)
{
    Console.WriteLine($"{trade.TradeId}, {trade.Type}, {trade.Date:yyyy-MM-dd}, {trade.Client}, {trade.Security}, {trade.Amount}, {trade.Price:C}, {trade.Fee:C}");
}

static List<Trades> ReadTradesFromCsv(string filePath)
{
    var trades = new List<Trades>();
    var lines = File.ReadAllLines(filePath);
    var headers = lines[0].Split(';');

    foreach (var line in lines.Skip(1))
    {
        var values = line.Split(';');
        var trade = new Trades
        {
            TradeId = int.Parse(values[Array.IndexOf(headers, "TradeId")]),
            Type = values[Array.IndexOf(headers, "Type")],
            Date = DateTime.Parse(values[Array.IndexOf(headers, "Date")]),
            Client = values[Array.IndexOf(headers, "Client")],
            Security = values[Array.IndexOf(headers, "Security")],
            Amount = int.Parse(values[Array.IndexOf(headers, "Amount")]),
            Price = decimal.Parse(values[Array.IndexOf(headers, "Price")], CultureInfo.InvariantCulture),
            Fee = decimal.Parse(values[Array.IndexOf(headers, "Fee")], CultureInfo.InvariantCulture)
        };
        trades.Add(trade);
    }
    return trades;
}