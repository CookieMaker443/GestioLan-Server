using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using GestioLan.API.Models;
using Serilog;
using System;
using System.Net;                                       // IPAddress
using System.Text;                                      // Per Encoding
using System.Threading.RateLimiting;                    // Per RateLimitPartition, FixedWindowRateLimiterOptions, QueueProcessingOrder
using Microsoft.AspNetCore.HttpOverrides;               // ForwardedHeaders, ForwardedHeadersOptions
using Microsoft.AspNetCore.RateLimiting;                // Per AddRateLimiter, UseRateLimiter
using Microsoft.AspNetCore.Authentication.JwtBearer;    // Per JwtBearerDefaults
using Microsoft.IdentityModel.Tokens;                   // Per TokenValidationParameters e SymmetricSecurityKey
using GestioLan.API.Utils.JWT;                          // Per la classe JWT 
using GestioLan.API.Utils.Helpers;                      // Per ICurrentUserService e CurrentUserService e middleware
using GestioLan.API.Services.Categories;                // Per ICategoryService e CategoryService
using GestioLan.API.Services.Images;                    // Per IImageService e ImageService
using GestioLan.API.Services.Users;                     // Per IUserService e UserService
using GestioLan.API.Services.Items;                     // Per IItemService e ItemService
using GestioLan.API.Services.QuantityUnits;             // Per IQuantityUnitService e QuantityUnitService
using GestioLan.API.Services.Metadata;
using System.Reflection;                                // per l'assembly
using Plugins.Shared;                                   //per IMetadataProvider
using GestioLan.API.Utils.Options;                      // Per RateLimitingOptions e ReverseProxyOptions



var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAZIONE SERILOG ---
string logsFolder = builder.Configuration["Storage:LogsPath"];
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()          // <-- NECESSARIO per User e Service, oper passare info ai logger con LogContext.PushProperty(...)
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:yyyy-MM-dd HH:mm:ss}][{Level:u4}][{User}][{Service}][{Action}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(logsFolder, 
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}][{Level:u4}][{User}][{Service}][{Action}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day, // Crea un file nuovo ogni giorno
        retainedFileCountLimit: 7) // Tiene solo gli ultimi 7 giorni
    .WriteTo.Seq(builder.Configuration["ConnectionStrings:seq"] ?? "http://seq:5341")   // <-- Seq in Docker
    .CreateLogger();

builder.Host.UseSerilog(); // Dice all'API di usare Serilog


builder.Services.AddEndpointsApiExplorer(); // qui si aggiunge la configurazione per Swagger/OpenAPI
builder.Services.AddSwaggerGen();           // qui si aggiunge la configurazione per Swagger/OpenAPI
builder.Services.AddControllers();          // qui si aggiunge la configurazione per i controller


// --- Caricamento dei plugin dalla cartella ---
// Recuperiamo il percorso configurato in appsettings.json. 
// Se per caso manca, usiamo come fallback la cartella locale "plugins"
string pluginsFolder = builder.Configuration["Storage:PluginsPath"] 
                      ?? Path.Combine(AppContext.BaseDirectory, "plugins");
// Logghiamo il percorso che l'API sta scansionando (utile in Docker per fare debug dei volumi)
Log.Information("[Plugin Loader] Cartella di scansione configurata: {Path}", pluginsFolder);
if (Directory.Exists(pluginsFolder))
{   
    // Carica Plugins.Shared PRIMA del loop 
    // Caricandola nel Default context adesso, quando i plugin la richiedono
    // il runtime la trova già e non la ricarica una seconda volta.
    string sharedDll = Path.Combine(pluginsFolder, "Plugins.Shared", "Plugins.Shared.dll");
    if (File.Exists(sharedDll))
    {
        Assembly.LoadFrom(sharedDll);
        Log.Information("[Plugin Loader] Plugins.Shared caricata: {Path}", sharedDll);
    }
    else
    {
        Log.Warning("[Plugin Loader] Plugins.Shared.dll non trovata in: {Path}", sharedDll);
    }


    // Cerca tutti i file .dll nella cartella configurata
    // Loop sui plugin, esclusa la cartella Shared
    string sharedFolder = Path.Combine(pluginsFolder, "Plugins.Shared");
    var dllFiles = Directory
        .GetFiles(pluginsFolder, "*.dll", SearchOption.AllDirectories)
        .Where(dll =>
        {
            string folder = Path.GetDirectoryName(dll)!;
            // Salta tutto ciò che è dentro Plugins.Shared/
            return !folder.Equals(sharedFolder, StringComparison.OrdinalIgnoreCase);
        });;

    foreach (var dllPath in dllFiles)
    {
        try
        {
            // Carica la DLL in memoria tramite Riflessione
            Assembly assembly = Assembly.LoadFrom(dllPath);

            // Filtra solo le classi concrete che implementano l'interfaccia IMetadataProvider
            var providerTypes = assembly.GetTypes()
                .Where(t => typeof(IMetadataProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in providerTypes)
            {
                // Registrazione dinamica nel motore di Dependency Injection
                builder.Services.AddTransient(typeof(IMetadataProvider), type);
                Log.Information("[Plugin Loader] Caricato con successo: {PluginName} dalla DLL {DllName}", type.Name, Path.GetFileName(dllPath));
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Plugin Loader] Errore critico durante il caricamento della DLL: {DllPath}", dllPath);
        }
    }
}
else
{
    // Se la cartella specificata nell'appsettings non esiste (es. il volume Docker non è montato correttamente)
    // la creiamo al volo per evitare eccezioni di DirectoryNotFound successive
    Log.Warning("[Plugin Loader] La cartella dei plugin non esiste. Tentativo di creazione a: {Path}", pluginsFolder);
    try
    {
        Directory.CreateDirectory(pluginsFolder);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "[Plugin Loader] Impossibile creare la cartella dei plugin configurata.");
    }
}


// --- Registrazone dei servizi essenziali per i controller --- 
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IMetadataService, MetadataService>();
builder.Services.AddScoped<IQuantityUnitService, QuantityUnitService>();

// --- servizi necessari per loggare bene con le info degli utenti
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// --- Connection String per il db ---
var connectionString = builder.Configuration.GetConnectionString("GestioLANConnection");


builder.Services.AddDbContext<GestioLanContext>(options =>
    options.UseMySql(connectionString,
        new MariaDbServerVersion(new Version(10, 11, 13))));


// --- sezione che gestisce il JWT ---
var jwtSecret = builder.Configuration["JwtSettings:Key"];
if (string.IsNullOrEmpty(jwtSecret))
{
    //Console.WriteLine("WARNING: JWT_SECRET is not set. Please set it in the environment variables or appsettings.json.");
    throw new InvalidOperationException("FATAL: JWT_SECRET is not set!");
}
else
{
    Log.Information("JWT_SECRET is set.");
}

var keyBytes = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddScoped<JWT>(); // Registra la classe JWT come servizio, così può essere iniettata nei controller


// --- qui si aggiunge la poliza dell'adminonly ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("isAdmin", "true"));
});


// --- qui si aggiunge la configurazione per il rate limiting ---
var rl = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingOptions>()
         ?? new RateLimitingOptions();
var window = TimeSpan.FromMinutes(rl.WindowMinutes);    // <-- finestra di tempo per il rate limiting

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Definisce la politica di rate limiting per le richieste generali (Default)
    options.AddPolicy("Default", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() 
                ?? "unknown",   // <-- Se non riesce a ottenere l'IP, usa "unknown" come chiave di partizione
            factory: _ => new FixedWindowRateLimiterOptions     // <-- Configura il rate limiting
            {
                PermitLimit = rl.Default,
                Window = window,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("AI", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name   // <-- Usa il nome dell'utente come chiave di partizione per le richieste AI
                ?? context.Connection.RemoteIpAddress?.ToString()   // <-- Se non riesce a ottenere il nome dell'utente, usa l'IP
                ?? "unknown",   // <-- Se non riesce a ottenere l'IP, usa "unknown" come chiave di partizione
            factory: _ => new FixedWindowRateLimiterOptions     // <-- Configura il rate limiting
            {
                PermitLimit = rl.AI,
                Window = window,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});


// --- qui si aggiunge la configurazione per il reverse proxy ---
var proxy = builder.Configuration.GetSection("ReverseProxy").Get<ReverseProxyOptions>() // inserisce la sezione ReverseProxy del file di configurazione in un oggetto ReverseProxyOptions
            ?? new ReverseProxyOptions();   // se non esiste la sezione, crea un oggetto vuoto con valori di default

if (proxy.Enabled)
{
    // registra il middleware per i reverse proxy e salva le impostazioni di proxy in ForwardedHeadersOptions
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        // Configura quali intestazioni proxy devono essere considerate (legge gli headerX-Forwarded-For e X-Forwarded-Proto)
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

        // di default sono già presenti localhost: li svuotiamo per dichiarare i nostri
        o.KnownNetworks.Clear();
        o.KnownProxies.Clear();

        // Aggiunge gli IP dei proxy conosciuti e le reti conosciute dalle impostazioni di configurazione 
        foreach (var ip in proxy.KnownProxies)
            o.KnownProxies.Add(IPAddress.Parse(ip));

        foreach (var net in proxy.KnownNetworks)
        {
            var parts = net.Split('/');
            o.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(
                IPAddress.Parse(parts[0]), int.Parse(parts[1])));
        }
    });
}


// --- qui costruisce l'app con le configurazioni di prima e la esegue ---
var app = builder.Build();

// --- se il reverse proxy è abilitato, usa il middleware per leggere gli header X-Forwarded-For e X-Forwarded-Proto
if (proxy.Enabled)
    app.UseForwardedHeaders();

Log.Information($"Proxy enabled: {proxy.Enabled}");

// --- Configure the HTTP request pipeline. per swagger in velopement ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();  
app.UseStaticFiles();       // Fondamentale per far sì che l'API possa leggere/scrivere file 
app.UseRouting();
app.UseAuthentication();    // legge il JWT e popola il campo user
app.UseRateLimiter();       // applica il rate limiting
app.UseAuthorization();     // valida i permessi
app.UseMiddleware<LogEnricherMiddleware>();             // esegue il middleware
app.MapControllers().RequireRateLimiting("Default");    // esegue il controller e l'action, e assegna Default come politica di rate limiting

Log.Information("C# API is running...");
// Log.Information($"Using connection string: {connectionString}");
Log.Information($"Environment: {builder.Environment.EnvironmentName}");



// --- MIGRAZIONE AUTOMATICA DEL DATABASE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GestioLanContext>();

        // Questo comando applica le migrazioni pendenti. 
        // Se il DB non esiste, lo crea.
        if (context.Database.GetPendingMigrations().Any())
            {
            Log.Information("Applicazione migrazioni in corso...");
            context.Database.Migrate();
            Log.Information("Database aggiornato con successo!");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        Log.Error(ex, "Si è verificato un errore durante la migrazione del database.");
    }
}



app.Run();