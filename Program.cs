using Ai_Fund.Data.Interfaces;
using Ai_Fund.Data.Repositories;
using Ai_Fund.Services;
using Ai_Fund.Services.Embedding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register services
builder.Services.AddScoped<IMutualFundRepository, MutualFundRepository>();
builder.Services.AddScoped<IMutualFundService, MutualFundService>();
builder.Services.AddSingleton<IEmbeddingService, NomicEmbeddingService>();
builder.Services.AddSingleton<ILLMService, OllamaLLMService>();
builder.Services.AddScoped<IAiOrchestratorService, AiOrchestratorService>();
builder.Services.AddScoped<IKnowledgeGapService, KnowledgeGapService>();
builder.Services.AddSingleton<IQdrantService, QdrantService>();
builder.Services.AddScoped<ISyncService, SyncService>();

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
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
