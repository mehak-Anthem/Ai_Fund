using Ai_Fund.Data.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace Ai_Fund.Data.Repositories;

public class MutualFundRepository : IMutualFundRepository
{
    private readonly string _connectionString;

    public MutualFundRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<List<(int Id, string Question, string Answer, string Embedding)>> GetAllKnowledgeAsync()
    {
        var result = new List<(int, string, string, string)>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Id, Question, Answer, Embedding FROM MutualFundKnowledge", conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add((
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                        ));
                    }
                }
            }
        }

        return result;
    }

    public async Task UpdateEmbeddingAsync(int id, string embedding)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE MutualFundKnowledge SET Embedding = @Embedding WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Embedding", embedding);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
