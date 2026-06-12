# Bio-Signal Visualizer

A modern, highly interactive ASP.NET Blazor application designed to visualize, annotate, and export high-resolution biometric and activity data (such as data from Empatica devices). 

## 🌟 Key Features

* **Dynamic Data Rendering**: Built using the powerful `Blazor-ApexCharts` library for smooth, interactive, and beautifully customized line and scatter plots.
* **Modern UI/UX**: Designed with `MudBlazor`, featuring a custom premium aesthetic, dark mode inspired glassmorphism effects, and highly responsive components.
* **Custom Metric Selection**: Effortlessly switch between "Default" standard metrics (EDA, Actigraphy, Pulse Rate, Temperature), "All" metrics, or any custom combination of available signals.
* **Drag-and-Drop Reordering**: Intuitively drag and drop charts to reorder them on the screen in real-time, helping you stack related signals side-by-side for perfect correlation analysis.
* **Contextual Annotations**: Click anywhere on a chart to drop a point or range-based note. Annotations visually overlay across your selected metrics so you never lose context.
* **Activity Classification Overlays**: Toggle translucent background shading on the charts to see exactly when participants transitioned between classified activity states (e.g., "Walking", "Resting").
* **One-Click PDF Export**: Easily export your currently visible (and ordered!) charts, annotations, and activity overlays directly into a neatly formatted, landscape A4 PDF report for sharing and presentations.

## 🚀 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later.

### Running the App
1. Navigate to the `asp` directory inside this repository.
2. Build and run the server using the .NET CLI:
   ```bash
   cd asp/BioSignalVisualizer.Server
   dotnet build
   dotnet run
   ```
3. Open your browser and navigate to `http://localhost:5077/Tools/Empatica_Visualizer`.

*(Note: Ensure your `data` folder contains the required CSV files in the correct directory structure, as defined in your `appsettings.json` BaseDataPath).*

## 🛠️ Tech Stack
* **Framework**: ASP.NET Core Blazor (WebAssembly/Server hybrid architecture)
* **Component Library**: [MudBlazor](https://mudblazor.com/)
* **Charting**: [Blazor-ApexCharts](https://github.com/mikes-gh/blazor-apexcharts) (wrapping ApexCharts.js)
* **PDF Generation**: [QuestPDF](https://www.questpdf.com/)

## 📝 License
This project is open-sourced under the **MIT License**. Feel free to use, modify, and distribute it!
