using BioSignalVisualizer.Client;
using BioSignalVisualizer.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
    Timeout = TimeSpan.FromMinutes(30)
});
builder.Services.AddScoped<CatalogClient>();
builder.Services.AddScoped<MetricsClient>();
builder.Services.AddScoped<AnnotationsClient>();
builder.Services.AddScoped<PdfClient>();
builder.Services.AddScoped<ActivityClient>();
builder.Services.AddScoped<SyncClient>();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
});
builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
