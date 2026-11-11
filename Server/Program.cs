using Acderator.Server;
using MagicOnion.Server;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue("PORT", 3000), listen =>
    {
        listen.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

builder.Services.Configure<ServerOptions>(builder.Configuration.GetSection("Acderator"));
builder.Services.AddSingleton(new MySqlDataSource(
    builder.Configuration["DATABASE_URL"] ?? throw new InvalidOperationException("DATABASE_URL must be set.")));
builder.Services.AddSingleton<GameRepository>();
builder.Services.AddSingleton<MasterDataStore>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddMagicOnion();
builder.Services.AddHealthChecks();

var app = builder.Build();
await app.Services.GetRequiredService<MasterDataStore>().SynchronizeAsync();
app.MapHealthChecks("/healthz");
app.MapMagicOnionService();
app.Run();

public partial class Program;
