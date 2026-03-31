using Ai_Fund.Configuration;
using Ai_Fund.Data.Interfaces;
using Ai_Fund.Data.Repositories;
using Ai_Fund.Services;
using Ai_Fund.Services.Embedding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register services
builder.Services.AddScoped<IMutualFundRepository, MutualFundRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMutualFundService, MutualFundService>();
builder.Services.AddSingleton<IEmbeddingService, GeminiEmbeddingService>();
builder.Services.AddSingleton<ILLMService, GroqLLMService>();
builder.Services.AddScoped<IAiOrchestratorService, AiOrchestratorService>();
builder.Services.AddScoped<IKnowledgeGapService, KnowledgeGapService>();
builder.Services.AddSingleton<IQdrantService, QdrantService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddSingleton<IContextManager, ContextManager>();
builder.Services.AddScoped<IRewriteService, RewriteService>();
builder.Services.AddSingleton<IPersonalityService, PersonalityService>();
builder.Services.AddSingleton<IExpansionService, ExpansionService>();
builder.Services.AddSingleton<IComparisonService, ComparisonService>();
builder.Services.AddScoped<IStructuredAnswerService, StructuredAnswerService>();
builder.Services.AddScoped<ISmartGuidanceService, SmartGuidanceService>();
builder.Services.AddHttpClient<IMfApiService, MfApiService>();
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddHttpClient<IMarketNewsService, MultiSourceMarketNewsService>();
builder.Services.AddScoped<IMarketService, MarketService>();


// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = AppConfiguration.GetValue(
    builder.Configuration,
    "Jwt:Key",
    "Jwt__Key",
    "JWT_KEY")
    ?? "vK5p8S9zX2y4m1N7q3R6t0W4e1Q8i0O2p5A7s3D9f1G0h2J4k6L8";
var jwtIssuer = AppConfiguration.GetValue(
    builder.Configuration,
    "Jwt:Issuer",
    "Jwt__Issuer",
    "JWT_ISSUER")
    ?? "AiFundIssuer";
var jwtAudience = AppConfiguration.GetValue(
    builder.Configuration,
    "Jwt:Audience",
    "Jwt__Audience",
    "JWT_AUDIENCE")
    ?? "AiFundAudience";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Fund API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Enable CORS early to handle all requests
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Fund API v1");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", (IConfiguration configuration) => Results.Ok(new
{
    status = "ok",
    environment = app.Environment.EnvironmentName,
    config = new
    {
        databaseConfigured = AppConfiguration.HasConfiguredValue(
            configuration,
            "ConnectionStrings:DefaultConnection",
            "DefaultConnection",
            "ConnectionStrings__DefaultConnection"),
        jwtConfigured = AppConfiguration.HasConfiguredValue(
            configuration,
            "Jwt:Key",
            "Jwt__Key",
            "JWT_KEY"),
        qdrantConfigured = AppConfiguration.HasConfiguredValue(
            configuration,
            "Qdrant:Host",
            "Qdrant__Host",
            "QDRANT_HOST"),
        geminiConfigured = AppConfiguration.HasConfiguredValue(
            configuration,
            "Gemini:ApiKey",
            "Gemini__ApiKey",
            "GEMINI_API_KEY"),
        groqConfigured = AppConfiguration.HasConfiguredValue(
            configuration,
            "Groq:ApiKey",
            "Groq__ApiKey",
            "GROQ_API_KEY"),
        marketAuxConfigured = AppConfiguration.HasConfiguredValue(
            configuration,
            "MarketAux:ApiKey",
            "MarketAux__ApiKey",
            "MARKETAUX_API_KEY")
    }
}));

app.MapControllers();

app.Run();
