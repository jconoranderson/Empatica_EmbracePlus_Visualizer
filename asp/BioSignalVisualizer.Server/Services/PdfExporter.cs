using BioSignalVisualizer.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace BioSignalVisualizer.Server.Services;

public sealed class PdfExporter
{
    public byte[] CreateSummaryPdf(PdfExportRequest request)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(12);
                page.Size(PageSizes.A4.Landscape());
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("Empatica Digital Biomarker Summary").FontSize(18).SemiBold();
                        column.Item().Text($"Participant: {request.Participant}  |  Date: {request.Date}");
                    });
                });

                page.Content().Column(column =>
                {
                    var allAnnotations = request.Annotations?.ToList() ?? new List<AnnotationEntry>();
                    var activity = request.Activity?.ToList() ?? new List<ActivityWindow>();

                    var legendItems = activity
                        .GroupBy(w => w.Classification, StringComparer.OrdinalIgnoreCase)
                        .Select(g => new LegendItem(g.Key, g.First().Color ?? "#cfd0d1"))
                        .OrderBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (legendItems.Any())
                    {
                        column.Item().PaddingBottom(4).AlignRight().Row(legendRow =>
                        {
                            foreach (var item in legendItems)
                            {
                                legendRow.AutoItem().PaddingLeft(12).Row(inner =>
                                {
                                    inner.AutoItem().Width(12).Height(12).Background(item.Color);
                                    inner.Spacing(4);
                                    inner.AutoItem().Text(item.Label).FontSize(9);
                                });
                            }
                        });
                    }

                    foreach (var metric in request.Series)
                    {
                        column.Item().PaddingBottom(4).Element(
                            ComposeMetric(metric, allAnnotations, activity));
                    }

                    if (allAnnotations.Any())
                    {
                        column.Item().PageBreak();
                        column.Item().PaddingTop(10).Element(ComposeAnnotations(allAnnotations));
                    }
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static Action<IContainer> ComposeMetric(
        MetricSeries series,
        IReadOnlyCollection<AnnotationEntry> annotations,
        IReadOnlyCollection<ActivityWindow> activityWindows) => container =>
    {
        var points = series.Points.OrderBy(p => p.Timestamp).ToList();
        var relevantAnnotations = annotations
            .Where(a =>
                string.Equals(a.Metric, "Global", StringComparison.OrdinalIgnoreCase)
                || string.Equals(a.Metric, series.Metric, StringComparison.OrdinalIgnoreCase))
            .OrderBy(a => a.Timestamp)
            .ToList();

        container.Border(1).Padding(2).Column(col =>
        {
            col.Item().Text(FormatMetricTitle(series.Metric)).SemiBold();

            if (!points.Any())
            {
                col.Item().Text("No data available for this metric.").FontColor(Colors.Grey.Medium);
                return;
            }

            var chartBytes = RenderChartImage(series, relevantAnnotations, activityWindows);
            if (chartBytes.Length > 0)
            {
                col.Item().Image(chartBytes);
            }
        });
    };

    private static Action<IContainer> ComposeAnnotations(IEnumerable<AnnotationEntry> annotations) => container =>
    {
        container.Border(1).Padding(8).Column(col =>
        {
            col.Item().Text("Annotations").SemiBold();
            foreach (var note in annotations)
            {
                col.Item().Text($"[{note.Timestamp:u}] {note.Metric}: {note.Note}");
            }
        });
    };

    private static byte[] RenderChartImage(
        MetricSeries series,
        IReadOnlyList<AnnotationEntry> annotations,
        IReadOnlyCollection<ActivityWindow> activityWindows)
    {
        const int width = 1600;
        const int height = 200;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        DrawChart(canvas, width, height, series, annotations, activityWindows);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }

    private static void DrawChart(
        SKCanvas canvas,
        float width,
        float height,
        MetricSeries series,
        IReadOnlyList<AnnotationEntry> annotations,
        IReadOnlyCollection<ActivityWindow> activityWindows)
    {
        var margin = 24f;
        var left = margin;
        var right = width - margin;
        var top = margin;
        var bottom = height - margin;
        var chartWidth = right - left;
        var chartHeight = bottom - top;

        if (chartWidth <= 0 || chartHeight <= 0)
        {
            return;
        }

        var points = series.Points.OrderBy(p => p.Timestamp).ToList();
        if (!points.Any()) return;

        var minValue = points.Min(p => p.Value);
        var maxValue = points.Max(p => p.Value);
        if (Math.Abs(maxValue - minValue) < 0.0001)
        {
            var adjustment = Math.Abs(maxValue) < 0.001 ? 1 : Math.Abs(maxValue) * 0.05;
            maxValue += adjustment;
            minValue -= adjustment;
        }

        var minTime = points.First().Timestamp;
        var maxTime = points.Last().Timestamp;
        var totalSeconds = (maxTime - minTime).TotalSeconds;
        if (totalSeconds <= 0)
        {
            totalSeconds = 1;
        }

        double ValueToY(double value)
            => bottom - chartHeight * ((value - minValue) / (maxValue - minValue));

        double TimeToX(DateTime timestamp)
        {
            var clamped = timestamp < minTime ? minTime : timestamp > maxTime ? maxTime : timestamp;
            var seconds = (clamped - minTime).TotalSeconds;
            return left + chartWidth * (seconds / totalSeconds);
        }

        using var axisPaint = new SKPaint
        {
            Color = SKColors.Gray,
            StrokeWidth = 1,
            IsStroke = true,
            IsAntialias = true
        };

        canvas.DrawLine(left, bottom, right, bottom, axisPaint);
        canvas.DrawLine(left, top, left, bottom, axisPaint);

        using var gridPaint = new SKPaint
        {
            Color = new SKColor(220, 220, 220),
            StrokeWidth = 1,
            IsStroke = true
        };

        var gridSteps = 4;
        for (var i = 1; i < gridSteps; i++)
        {
            var y = (float)(top + chartHeight * (i / (float)gridSteps));
            canvas.DrawLine(left, y, right, y, gridPaint);
        }

        foreach (var window in activityWindows)
        {
            if (window.End <= minTime || window.Start >= maxTime)
            {
                continue;
            }

            var startX = (float)TimeToX(window.Start);
            var endX = (float)TimeToX(window.End);
            var rectLeft = Math.Min(startX, endX);
            var rectWidth = Math.Abs(endX - startX);
            using var regionPaint = new SKPaint
            {
                Color = ToColor(window.Color, 60),
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(rectLeft, top, rectWidth, chartHeight, regionPaint);
        }

        using var linePaint = new SKPaint
        {
            Color = SKColor.Parse(GetMetricColor(series.Metric)),
            StrokeWidth = 2,
            IsStroke = true,
            IsAntialias = true
        };

        var path = new SKPath();
        path.MoveTo((float)TimeToX(points[0].Timestamp), (float)ValueToY(points[0].Value));
        foreach (var point in points.Skip(1))
        {
            path.LineTo((float)TimeToX(point.Timestamp), (float)ValueToY(point.Value));
        }
        canvas.DrawPath(path, linePaint);

        using var annotationTextPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 10,
            IsAntialias = true
        };

        var greyShades = new[] { SKColor.Parse("#1f2937"), SKColor.Parse("#374151"), SKColor.Parse("#4b5563"), SKColor.Parse("#6b7280"), SKColor.Parse("#9ca3af") };
        var uniquePointNotes = annotations
            .Where(a => !a.IsRange)
            .Select(a => a.Note)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var note in annotations)
        {
            if (note.IsRange && note.EndTimestamp.HasValue)
            {
                var startX = (float)TimeToX(note.Timestamp);
                var endX = (float)TimeToX(note.EndTimestamp.Value);
                var rectLeft = Math.Min(startX, endX);
                var rectWidth = Math.Max(1, Math.Abs(endX - startX));
                
                using var regionPaint = new SKPaint
                {
                    Color = SKColor.Parse("#374151").WithAlpha(51),
                    Style = SKPaintStyle.Fill
                };
                canvas.DrawRect(rectLeft, top, rectWidth, chartHeight, regionPaint);
                
                var label = note.Note;
                var textWidth = annotationTextPaint.MeasureText(label);
                var padding = 4f;
                var boxWidth = textWidth + padding * 2;
                var boxHeight = annotationTextPaint.TextSize + padding * 2;
                var boxX = rectLeft + (rectWidth / 2) - (boxWidth / 2);
                boxX = Math.Clamp(boxX, left, right - boxWidth);
                var boxY = top + 4;
                
                using var annotationBackground = new SKPaint { Color = SKColor.Parse("#374151"), Style = SKPaintStyle.Fill, IsAntialias = true };
                canvas.DrawRect(boxX, boxY, boxWidth, boxHeight, annotationBackground);
                canvas.DrawText(label, boxX + padding, boxY + boxHeight - padding, annotationTextPaint);
            }
            else
            {
                int noteIndex = uniquePointNotes.FindIndex(n => string.Equals(n, note.Note, StringComparison.OrdinalIgnoreCase));
                int pointIndex = Math.Max(0, noteIndex) % 5;
                var pointColor = greyShades[pointIndex];
                
                using var annotationPaint = new SKPaint
                {
                    Color = pointColor,
                    StrokeWidth = 1,
                    IsStroke = true,
                    IsAntialias = true,
                    PathEffect = SKPathEffect.CreateDash(new[] { 4f, 4f }, 0)
                };
                
                var x = (float)TimeToX(note.Timestamp);
                canvas.DrawLine(x, top, x, bottom, annotationPaint);

                var label = note.Note;
                var textWidth = annotationTextPaint.MeasureText(label);
                var padding = 4f;
                var boxWidth = textWidth + padding * 2;
                var boxHeight = annotationTextPaint.TextSize + padding * 2;
                var boxX = Math.Clamp(x - boxWidth / 2, left, right - boxWidth);
                var boxY = top + (pointIndex * 26) + 4; 

                using var annotationBackground = new SKPaint { Color = pointColor, Style = SKPaintStyle.Fill, IsAntialias = true };
                canvas.DrawRect(boxX, boxY, boxWidth, boxHeight, annotationBackground);
                canvas.DrawText(label, boxX + padding, boxY + boxHeight - padding, annotationTextPaint);
            }
        }

        using var axisTextPaint = new SKPaint
        {
            Color = SKColors.Gray,
            TextSize = 9,
            IsAntialias = true
        };

        var maxLabel = maxValue.ToString("F2");
        var minLabel = minValue.ToString("F2");
        canvas.DrawText(maxLabel, left, top - 4, axisTextPaint);
        canvas.DrawText(minLabel, left, bottom + axisTextPaint.TextSize + 2, axisTextPaint);

        var timeLabels = BuildTimeTicks(minTime, maxTime, left, right, axisTextPaint);
        foreach (var tick in timeLabels)
        {
            canvas.DrawText(tick.Label, tick.PositionX, bottom + axisTextPaint.TextSize * 2, axisTextPaint);
        }
    }

    private static IReadOnlyList<(float PositionX, string Label)> BuildTimeTicks(
        DateTime minTime,
        DateTime maxTime,
        float left,
        float right,
        SKPaint paint)
    {
        var ticks = new List<(float, string)>();
        var totalSeconds = Math.Max(1, (maxTime - minTime).TotalSeconds);
        var segments = 4;
        for (var i = 0; i <= segments; i++)
        {
            var ratio = i / (double)segments;
            var timestamp = minTime.AddSeconds(totalSeconds * ratio);
            var label = timestamp.ToString("HH:mm");
            var x = left + (float)((right - left) * ratio);
            var width = paint.MeasureText(label);

            if (ticks.Any(existing => Math.Abs(existing.Item1 - x) < width + 6))
            {
                continue;
            }

            ticks.Add((x - width / 2, label));
        }

        return ticks;
    }

    private static SKColor ToColor(string? hex, byte alpha = 255)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return new SKColor(207, 208, 209, alpha);
        }

        if (!hex.StartsWith('#'))
        {
            hex = $"#{hex}";
        }

        if (SKColor.TryParse(hex, out var color))
        {
            return color.WithAlpha(alpha);
        }

        return new SKColor(207, 208, 209, alpha);
    }

    private static string FormatMetricTitle(string rawName)
    {
        var last = rawName.LastIndexOf('_');
        var suffix = last >= 0 && last < rawName.Length - 1 ? rawName[(last + 1)..] : rawName;
        suffix = suffix.ToUpperInvariant();
        if (suffix == "EDA") return "ELECTRODERMAL ACTIVITY";
        
        var dashIndex = suffix.IndexOf('-');
        if (dashIndex > 0)
        {
            suffix = suffix[..dashIndex];
        }
        
        return suffix;
    }

    private static string GetMetricColor(string metricName)
    {
        if (string.IsNullOrWhiteSpace(metricName)) return "#1f2937";

        var last = metricName.LastIndexOf('_');
        var suffix = last >= 0 && last < metricName.Length - 1 ? metricName[(last + 1)..] : metricName;
        suffix = suffix.ToUpperInvariant();

        return suffix switch
        {
            "EDA" => "#166534",
            "ACCELEROMETERS-STD" => "#1e3a8a",
            "ACTIGRAPHY-COUNTS" => "#1e3a8a",
            "PULSE-RATE" => "#991b1b",
            "TEMPERATURE" => "#ea580c",
            "PRV" => "#4c1d95",
            "RESPIRATORY-RATE" => "#374151",
            _ => "#1f2937"
        };
    }

    private sealed record LegendItem(string Label, string Color);
}
