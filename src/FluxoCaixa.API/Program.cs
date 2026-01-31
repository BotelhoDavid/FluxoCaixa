using FluxoCaixa.Application.Interfaces;
using FluxoCaixa.Application.Services;
using FluxoCaixa.Domain.Interfaces.Repositories;
using FluxoCaixa.Domain.Interfaces.Services;
using FluxoCaixa.Domain.Interfaces.UoW;
using FluxoCaixa.Domain.Models;
using FluxoCaixa.Infra.CrossCutting.ExceptionHandler;
using FluxoCaixa.Infra.Data.Context;
using FluxoCaixa.Infra.Data.Repositories;
using FluxoCaixa.Infra.Data.Services;
using FluxoCaixa.Infra.Data.Uow;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using System.Net;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var connectionString = FluxoCaixaContext.GetConnectionStringFromEnvironment();

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

builder.Services.AddDbContext<FluxoCaixaContext>(options =>
    options.UseSqlServer(connectionString));

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

//Dependency Injection
builder.Services.AddHttpClient();

// Domain Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Application Services
builder.Services.AddScoped<IAutenticacaoAppService, AutenticacaoAppService>();
builder.Services.AddScoped<ILancamentoAppService, LancamentoAppService>();

// Repositories
builder.Services.AddScoped<ILancamentoRepository, LancamentoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add services to the container.
builder.Services.AddControllers(options =>
                {
                    options.ModelBinderProviders.Insert(0, new FluxoCaixa.API.Binders.DateTimeModelBinderProvider());
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(
                        new FluxoCaixa.API.Converters.DateTimeJsonConverter());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                }); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseInlineDefinitionsForEnums();
    options.EnableAnnotations();

    // Configura leitura de comentários XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    var applicationXmlFile = "FluxoCaixa.Application.xml";
    var applicationXmlPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, applicationXmlFile);
    if (System.IO.File.Exists(applicationXmlPath))
    {
        options.IncludeXmlComments(applicationXmlPath);
    }

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FluxoCaixa API",
        Version = "v1",
        Description = "API para gerenciamento de fluxo de caixa"
    });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// Mapster Configuration
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(Program).Assembly, typeof(AutenticacaoAppService).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"] ?? "ChaveMestraSuperSecretaUtilizadaParaTeste123!");
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            throw new ApiException(message: "Você precisa estar autenticado para acessar este recurso.", 
                                   statusCode: HttpStatusCode.Unauthorized);
        }
    };
});


builder.Services.AddAuthorization();

// Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}.json";
    });

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("FluxoCaixa API Reference");
        options.WithTheme(ScalarTheme.Mars);
        options.WithSidebar(true);
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.WithOpenApiRoutePattern("/openapi/v1.json");
    });
}

// Apply migrations automatically
await ApplyMigrationsAsync(app);

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<FluxoCaixaContext>();

    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Tentativa {Attempt}/{MaxRetries}: Aplicando migrations...", attempt, maxRetries);

            await db.Database.MigrateAsync();

            logger.LogInformation("Migrations aplicadas com sucesso.");
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Erro ao conectar no banco (Tentativa {Attempt}/{MaxRetries}).",
                attempt, maxRetries);

            if (attempt == maxRetries)
            {
                logger.LogCritical(ex,
                    "Não foi possível aplicar migrations após {MaxRetries} tentativas.",
                    maxRetries);
                throw;
            }

            await Task.Delay(delay);
        }
    }
}

app.Run();
