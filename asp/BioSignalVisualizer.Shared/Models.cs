namespace BioSignalVisualizer.Shared;

public record CatalogEntry(string Date, string Participant, IEnumerable<string> Metrics);

public record ParticipantEntry(string Date, string Participant);

public record MetricSeries(string Participant, string Metric, IEnumerable<DataPoint> Points, IEnumerable<ActivityWindow>? Activity = null);

public record DataPoint(DateTime Timestamp, double Value);

public record ActivityWindow(string Participant, DateTime Start, DateTime End, string Classification, string Color);

public record AnnotationEntry(
    string Date,
    string Participant,
    string Metric,
    DateTime Timestamp,
    string Note,
    bool IsRange = false,
    DateTime? EndTimestamp = null);

public record PdfExportRequest(
    string Date,
    IEnumerable<string> Participants,
    IEnumerable<MetricSeries> Series,
    IEnumerable<AnnotationEntry> Annotations,
    IEnumerable<ActivityWindow>? Activity);
