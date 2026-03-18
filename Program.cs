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

app.UseAuthorization();

app.MapControllers();

app.Run();
