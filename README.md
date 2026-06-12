# Empatica Digital Biomarker Viewer

This repository now includes a lightweight Streamlit application that scans the date-based Empatica exports stored in this folder and visualises the per-minute CSV metrics.

## Setup

1. (Recommended) Create and activate a virtual environment:
   ```bash
   python3 -m venv .venv
   source .venv/bin/activate
   ```
2. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

## Usage

Run the Streamlit app from the repository root:

```bash
streamlit run visualizer/app.py
```

The sidebar lets you:
- Refresh the catalog (useful after new data is synchronised).
- Pick a date folder, participant export, and metric CSV.

The interface shows a recording window at the top (first/last valid samples) and automatically surfaces the accelerometers-std, eda, pulse-rate, and temperature metrics for the selected participant/date so you always see the core signals up front. Each metric view displays interactive charts for numeric (line plot) and categorical (scatter) measurements, with translucent bands and a right-aligned legend (per chart) that mirror the activity-classification stream so state changes are immediately visible. Use the annotation panel beneath the recording window to drop per-timestamp phase-line notes that appear on every chart; manage them in one place. Switching dates or participants clears annotations automatically so notes stay scoped to the current dataset. Data discovery is cached for 60 seconds so the interface can stay responsive while still detecting new files shortly after they arrive.

Use the `Download charts PDF` button near the top-right of the page to export the currently visible charts (including default metrics and any selected extra metric) to a single-page PDF for sharing. The app uses Plotly’s Kaleido engine, `pypdf`, and `Pillow`; if the button is disabled, install Kaleido with `pip install --force-reinstall kaleido==0.2.1` (and ensure Pillow is available) before restarting the app.

## Raw Avro inspection

To inspect the raw `.avro` device exports interactively, open `notebooks/avro_exploration.ipynb` in Jupyter. It previews the schema and loads a sample into a DataFrame so you can discover which fields are available.

To stream the entire Avro file into per-metric CSV tables, use:

```bash
python scripts/extract_biometrics.py \
  --input 2025-10-01/TESTSUBJECT-3YK9K1J1QX/raw_data/v6/1-1-TESTSUBJECT_1759347781.avro \
  --output tables/raw_metrics
```

The script uses `fastavro` + `pandas` to flatten the records, automatically detects the first timestamp column, and writes one CSV per numeric measurement (axes, heart rate, temperature, etc.).
