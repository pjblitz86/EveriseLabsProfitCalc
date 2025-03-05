using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveriseLabsProfitCalc
{
    class Trades
    {
        public int TradeId { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string Client { get; set; }
        public string Security { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Fee { get; set; }
    }
}
