namespace Ai_Fund.Services;

public interface IQdrantService
{
    Task InitializeCollectionAsync();
    Task UpsertAsync(int id, float[] vector, string content, Dictionary<string, object>? metadata = null);
    Task<List<QdrantSearchResult>> SearchAsync(float[] queryVector, int limit = 3);
    Task<bool> CollectionExistsAsync();
    Task DeleteCollectionAsync();
}

public class QdrantSearchResult
{
    public int Id { get; set; }
    public double Score { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}
