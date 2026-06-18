using BioSignalVisualizer.Server.Services;
using BioSignalVisualizer.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<VisualizerSettings>(builder.Configuration.GetSection("VisualizerSettings"));
builder.Services.AddSingleton<DataCatalogService>();
builder.Services.AddSingleton<MetricLoader>();
builder.Services.AddSingleton<AnnotationStore>();
builder.Services.AddSingleton<PdfExporter>();
builder.Services.AddSingleton<ActivityWindowService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
    });
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

public class VisualizerSettings
{
    public string BaseDataPath { get; set; } = "./data";
}
