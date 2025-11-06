# MarketPal — Starter ASP.NET Core Razor Pages site

This workspace contains a minimal, nicely styled Razor Pages site under `MarketPal.Web`.

## Market Tracker (educational)

A small educational "Market Tracker" page has been added to demonstrate simple stock analysis visualizations:

- Latest price (demo data)
- 50-day and 200-day moving averages (MA50, MA200)
- Color-coded BUY / HOLD signal based on MA50 vs MA200
- A small price + moving averages chart using Chart.js (via CDN)

This is for educational purposes only and is not financial advice.

## Prerequisites

- .NET SDK 8.0 or later (install from https://dotnet.microsoft.com)

## Run locally

```bash
cd MarketPal.Web
dotnet restore
dotnet run
```

Then open https://localhost:5001 or the URL shown in the console. The Market Tracker page is available at:

```
https://localhost:5001/MarketTracker
```

## Tech / Implementation notes

- The tracker uses demo (synthetic) price data by default and computes simple moving averages.
- Charting is done via Chart.js included from a CDN and a small JS helper at `wwwroot/js/marketTrackerChart.js`.
- The server-side model and service live under `Pages/`, `Models/`, and `Services/` inside the `MarketPal.Web` project.

## Disclaimer

This tool is for educational visualization only. It does not provide financial advice or trading signals. Always perform your own research before making financial decisions.
