using AnjUx.Client;
using AnjUx.Client.Shared;
using AnjUx.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("AnjUx.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AnjUx.ServerAPI"));

// Registramos todos os services via reflection
var baseServiceType = typeof(BaseService);
List<Type> serviceTypes = Assembly.GetAssembly(baseServiceType)!.GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseServiceType.IsAssignableFrom(t)).ToList();
foreach (var serviceType in serviceTypes)
    builder.Services.AddScoped(serviceType);

Assembly assembly = Assembly.GetExecutingAssembly();
CoreRoutes.Instance.Initialize(assembly, "AnjUx.Client.Pages");

CoreRegistor.Register(builder.Services);

await builder.Build().RunAsync();
