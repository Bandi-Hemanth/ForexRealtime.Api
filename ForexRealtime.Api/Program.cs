using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ForexRealtime.Api.Hubs;
using ForexRealtime.Api.Infrastructure.Persistence;
using ForexRealtime.Api.Services;

// Serilog with correlation ID
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ForexRealtime.Api")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ForexRealtime.Api")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"));

builder.Services.AddHttpContextAccessor();

var conn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=forex_realtime;Username=postgres;Password=postgres";
builder.Services.AddDbContext<ForexDbContext>(o => o.UseNpgsql(conn));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddSignalR();

builder.Services.AddSingleton<ITickCache, TickCache>();
builder.Services.AddHostedService<FinnhubIngestService>();
builder.Services.AddHostedService<BroadcastService>();
builder.Services.AddHostedService<TickPersistenceService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "ForexRealtime-SigningKey-Min32Chars!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ForexRealtime",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ForexRealtime",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/forex"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true);
}));

var app = builder.Build();

app.Use(async (context, next) =>
{
    var traceId = context.TraceIdentifier;
    if (string.IsNullOrEmpty(traceId)) context.TraceIdentifier = Guid.NewGuid().ToString("N");
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
        await next();
});

app.UseCors();
app.UseSerilogRequestLogging(options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
{
    diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
app.MapHub<ForexHub>("/hubs/forex");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ForexDbContext>();
    try { await db.Database.MigrateAsync(); }
    catch (Exception ex) { Log.Warning(ex, "Migrations not applied (e.g. DB not available). Run: dotnet ef database update"); }
}

await app.RunAsync();
