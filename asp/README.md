# Bio-Signal Visualizer | Empatica Embrace Data Analysis

An advanced, open-source **ASP.NET Core 8 hosted Blazor WebAssembly** application designed for visualizing, analyzing, and annotating digital biomarker data exported from **Empatica Embrace** wearables. This tool provides a powerful, interactive alternative to standard scripts, allowing researchers and health tech professionals to effortlessly explore physiological data (EDA, Actigraphy, Pulse Rate, Temperature).

## Key Features

- **Automatic Data Discovery**: Seamlessly parses Empatica Embrace export folders (`VisualizerSettings:BaseDataPath`).
- **Interactive Multi-User Comparison**: Side-by-side synchronized timeline visualizations using modern charting (ApexCharts/Chart.js).
- **Digital Biomarker Analysis**: Visualize Electrodermal Activity (EDA), Actigraphy, and more.
- **Persistent Annotations**: Add and manage point/range event annotations stored locally alongside data.
- **High-Quality PDF Exports**: Generate single-page PDF reports via QuestPDF for clinical or research sharing.

## Architecture & Projects

- **BioSignalVisualizer.Server** – ASP.NET Core Web API serving the Blazor app and handling backend file/catalog operations.
- **BioSignalVisualizer.Client** – Blazor WebAssembly UI utilizing MudBlazor and ApexCharts for high-performance interactive visual analytics.
- **BioSignalVisualizer.Shared** – Shared data models and DTOs bridging the client-server gap.


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
