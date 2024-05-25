using AnjUx.Migrator;
using AnjUx.Server;
using AnjUx.Server.Controllers;
using AnjUx.Server.Middlewares;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new PadraoRoteamento());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
    builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
builder.Services.AddRazorPages();

var app = builder.Build();


app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("CorsPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

Config.Instance.Initialize(app.Configuration);

new Migrator(Config.Instance.ConnectionString!, Config.Instance.DbName!).Executar();

app.Run();
