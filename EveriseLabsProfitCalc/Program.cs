using EveriseLabsProfitCalc;
using System.Diagnostics;
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
Dictionary<string, decimal> profitLoss = CalculateProfitLoss(trades, client, targetDate);

Console.WriteLine($"Profit/Loss for {client} on {targetDate:yyyy-MM-dd}");
Console.WriteLine("-------------------------------------");
foreach (var entry in profitLoss)
{
    Console.WriteLine($"{entry.Key}: {entry.Value:C}");
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

static Dictionary<string, decimal> CalculateProfitLoss(List<Trades> trades, string client, DateTime targetDate)
{
    var filteredTrades = trades.Where(t => t.Client == client && t.Date <= targetDate)
                               .OrderBy(t => t.Date)
                               .ToList();

    var securityHoldings = new Dictionary<string, Queue<(int Amount, decimal Cost)>>();
    var profitLoss = new Dictionary<string, decimal>();

    foreach (var trade in filteredTrades)
    {
        if (!profitLoss.ContainsKey(trade.Security))
            profitLoss[trade.Security] = 0m;

        if (trade.Type == "BUY")
        {
            decimal cost = (trade.Price * trade.Amount) + trade.Fee;
            if (!securityHoldings.ContainsKey(trade.Security))
                securityHoldings[trade.Security] = new Queue<(int, decimal)>();
            securityHoldings[trade.Security].Enqueue((trade.Amount, cost / trade.Amount));
        }
        else if (trade.Type == "SELL")
        {
            int amountToSell = trade.Amount;
            decimal revenue = (trade.Price * trade.Amount) - trade.Fee;
            decimal costOfSold = 0m;

            while (amountToSell > 0 && securityHoldings.ContainsKey(trade.Security) && securityHoldings[trade.Security].Count > 0)
            {
                var (availableAmount, unitCost) = securityHoldings[trade.Security].Dequeue();
                int usedAmount = Math.Min(availableAmount, amountToSell);
                costOfSold += usedAmount * unitCost;
                amountToSell -= usedAmount;
                if (availableAmount > usedAmount)
                    securityHoldings[trade.Security].Enqueue((availableAmount - usedAmount, unitCost));
            }

            profitLoss[trade.Security] += revenue - costOfSold;
        }
    }

    return profitLoss;
}