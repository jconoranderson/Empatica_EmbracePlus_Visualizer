# BioSignalVisualizer (Blazor)

This folder contains an ASP.NET Core 8 hosted Blazor application that mirrors the Streamlit Empatica visualizer.

## Projects

- **BioSignalVisualizer.Server** – ASP.NET Core Web API that exposes catalog, metric, annotations, and PDF export endpoints, and serves the Blazor app.
- **BioSignalVisualizer.Client** – Blazor WebAssembly UI (MudBlazor + Chart.js) for data exploration.
- **BioSignalVisualizer.Shared** – Shared DTOs used by both client and server.

## Key Features

- Automatic discovery of Empatica export folders (`VisualizerSettings:BaseDataPath`).
- Chart rendering via Chart.js with adjustable height slider.
- Persistent annotations stored in `annotations_store.json` next to the data.
- Single-page PDF export using QuestPDF.

## Prerequisites

- .NET 8 SDK
- Empatica exports available in the path configured via `appsettings.json` (`VisualizerSettings:BaseDataPath`).

## Getting Started

```bash
cd asp

# Restore and build
 dotnet restore
 dotnet build

# Run the hosted app
 dotnet run --project BioSignalVisualizer.Server
```

Browse to `https://localhost:7188` (or the HTTP endpoint shown in the console).

## NuGet Packages

- `CsvHelper` – CSV parsing for per-minute aggregates.
- `QuestPDF` – PDF generation on the server.
- `ChartJs.Blazor`, `MudBlazor`, `Blazored.LocalStorage` – Client-side UI/visuals and state persistence.

## Notes

- The client requests metrics dynamically; large datasets may require backend paging.
- PDF export currently includes the first 25 rows per metric for brevity.
- Modify `BaseDataPath` in `BioSignalVisualizer.Server/appsettings.json` to point at your Empatica export root.
