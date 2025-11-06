using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using MarketPal.Web.Models;
using MarketPal.Web.Services;

namespace MarketPal.Web.Pages
{
    public class MarketTrackerModel : PageModel
    {
        private readonly StockService _stockService;

        public MarketTrackerModel(StockService stockService)
        {
            _stockService = stockService;
        }

        public string Symbol { get; set; } = "DEMO";
        public decimal? LatestClose { get; set; }
        public decimal? MA50 { get; set; }
        public decimal? MA200 { get; set; }
        public string SignalLabel { get; set; } = "—";
        public string SignalClass { get; set; } = "signal-neutral";

    // Additional indicators
    public decimal? VolatilityPercent { get; set; }
    public decimal? MA50SlopePercent { get; set; }
    public decimal? RecentLow { get; set; }
    public decimal? RecentHigh { get; set; }
    public string RecommendationText { get; set; } = "—";
    public string RecommendationClass { get; set; } = "text-muted";

        // Series for the chart
        public List<string> Labels { get; set; } = new();
        public List<decimal> Prices { get; set; } = new();
        public List<decimal?> MA50Series { get; set; } = new();
        public List<decimal?> MA200Series { get; set; } = new();

        public async System.Threading.Tasks.Task OnGetAsync(string? symbol)
        {
            Symbol = string.IsNullOrWhiteSpace(symbol) ? "DEMO" : symbol.ToUpperInvariant();
            var quotes = await _stockService.GetDemoQuotesAsync(Symbol, days: 260);

            LatestClose = quotes.OrderByDescending(q => q.Date).FirstOrDefault()?.Close;
            MA50 = _stockService.ComputeMovingAverage(quotes, 50);
            MA200 = _stockService.ComputeMovingAverage(quotes, 200);

            if (MA50.HasValue && MA200.HasValue)
            {
                if (MA50 > MA200)
                {
                    SignalLabel = "BUY";
                    SignalClass = "signal-buy";
                }
                else if (MA50 < MA200)
                {
                    SignalLabel = "HOLD";
                    SignalClass = "signal-hold";
                }
                else
                {
                    SignalLabel = "NEUTRAL";
                    SignalClass = "signal-neutral";
                }
            }

            Labels = quotes.Select(q => q.Date.ToString("MM-dd")).ToList();
            Prices = quotes.Select(q => q.Close).ToList();
            MA50Series = _stockService.ComputeRollingMA(quotes, 50);
            MA200Series = _stockService.ComputeRollingMA(quotes, 200);

            // Volatility (14-day std dev of daily returns, annualized)
            var closes = quotes.OrderBy(q => q.Date).Select(q => q.Close).ToList();
            if (closes.Count >= 15)
            {
                var lastReturns = new List<decimal>();
                for (int i = closes.Count - 14; i < closes.Count; i++)
                {
                    var prev = closes[i - 1];
                    var cur = closes[i];
                    lastReturns.Add((cur - prev) / prev);
                }
                // sample std dev
                var avg = lastReturns.Average();
                var sumSq = lastReturns.Select(r => (double)(r - (decimal)avg)).Sum(d => d * d);
                var std = (decimal)Math.Sqrt(sumSq / (lastReturns.Count - 1));
                // annualize approx
                VolatilityPercent = Math.Round(std * (decimal)Math.Sqrt(252) * 100, 2);
            }

            // MA50 slope (compare current MA50 to MA50 10 days ago)
            if (MA50Series != null && MA50Series.Count >= 11)
            {
                var lastIdx = MA50Series.Count - 1;
                var current = MA50Series[lastIdx];
                var earlier = MA50Series[Math.Max(0, lastIdx - 10)];
                if (current.HasValue && earlier.HasValue && earlier != 0)
                {
                    MA50SlopePercent = Math.Round((current.Value - earlier.Value) / earlier.Value * 100, 2);
                }
            }

            // Recent support/resistance (30-day min/max)
            if (closes.Count >= 30)
            {
                var recent = closes.Skip(closes.Count - 30).Take(30).ToList();
                RecentLow = recent.Min();
                RecentHigh = recent.Max();
            }

            // Recommendation heuristics (educational only)
            if (MA50.HasValue && MA200.HasValue && MA50SlopePercent.HasValue && VolatilityPercent.HasValue)
            {
                if (MA50 > MA200 && MA50SlopePercent > 0 && VolatilityPercent < 40)
                {
                    RecommendationText = "Trend appears strong — MA50 above MA200 with positive slope and moderate volatility. Consider studying position sizing and risk management before taking action.";
                    RecommendationClass = "text-success";
                }
                else if (MA50 < MA200 && MA50SlopePercent < 0)
                {
                    RecommendationText = "Downtrend — MA50 below MA200 and slope negative. This is typically a sign to avoid initiating long positions; consider watching for trend reversal signals.";
                    RecommendationClass = "text-warning";
                }
                else
                {
                    RecommendationText = "Neutral or mixed signals — combine this with other indicators (volume, momentum) and fundamental research before acting.";
                    RecommendationClass = "text-muted";
                }
            }
        }
    }
}
