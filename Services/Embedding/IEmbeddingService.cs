namespace Ai_Fund.Services.Embedding;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
}
