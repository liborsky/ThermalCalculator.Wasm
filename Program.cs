using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ThermalCalculator.Wasm;
using ThermalCalculator.Wasm.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Radzen services
builder.Services.AddRadzenComponents();

// Add application services
builder.Services.AddScoped<MaterialService>();
builder.Services.AddScoped<WallAssemblyService>();
builder.Services.AddScoped<PdfExportService>();
builder.Services.AddScoped<WallTemplateService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<InsulationOptimizationService>();

await builder.Build().RunAsync();
