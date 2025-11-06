using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPal.Web.Models;

namespace MarketPal.Web.Services
{
    /// <summary>
    /// Provides demo stock data and utility methods for moving averages.
    /// This service intentionally uses synthetic/demo data by default.
    /// </summary>
    public class StockService
    {
        private readonly Random _rng = new(DateTime.Now.Millisecond);

        public Task<List<StockQuote>> GetDemoQuotesAsync(string symbol, int days = 250)
        {
            // Generate synthetic daily close prices for `days` days.
            var quotes = new List<StockQuote>(days);

            // Start price depends on symbol so repeated runs vary slightly by symbol
            decimal basePrice = symbol?.GetHashCode() ?? 100;
            basePrice = Math.Abs(basePrice % 500) + 20;

            DateTime today = DateTime.UtcNow.Date;

            decimal price = basePrice;
            for (int i = 0; i < days; i++)
            {
                // Walk the price with small random changes
                var changePct = (decimal)(_rng.NextDouble() * 0.02 - 0.01); // -1% .. +1%
                price = Math.Max(1, price + price * changePct);
                var dt = today.AddDays(- (days - 1 - i));
                quotes.Add(new StockQuote { Date = dt, Close = Math.Round(price, 2) });
            }

            return Task.FromResult(quotes);
        }

        public decimal? ComputeMovingAverage(IEnumerable<StockQuote> quotes, int period)
        {
            if (quotes == null) return null;
            var list = quotes.OrderBy(q => q.Date).Select(q => q.Close).ToList();
            if (list.Count < period) return null;
            var last = list.Skip(Math.Max(0, list.Count - period)).Take(period);
            return Math.Round(last.Average(), 2);
        }

        /// <summary>
        /// Compute MA for each day aligned with dates; returns null for days with insufficient history.
        /// </summary>
        public List<decimal?> ComputeRollingMA(IEnumerable<StockQuote> quotes, int period)
        {
            var ordered = quotes.OrderBy(q => q.Date).Select(q => q.Close).ToList();
            var result = new List<decimal?>(ordered.Count);
            for (int i = 0; i < ordered.Count; i++)
            {
                if (i + 1 < period)
                {
                    result.Add(null);
                    continue;
                }
                var window = ordered.Skip(i + 1 - period).Take(period);
                result.Add(Math.Round(window.Average(), 2));
            }
            return result;
        }
    }
}
