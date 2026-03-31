using Ai_Fund.Configuration;
using Ai_Fund.Data.Interfaces;
using Ai_Fund.Models;
using System.Data;
using System.Data.SqlClient;

namespace Ai_Fund.Data.Repositories;

public class MutualFundRepository : IMutualFundRepository
{
    private readonly string _connectionString;

    public MutualFundRepository(IConfiguration configuration)
    {
        _connectionString = AppConfiguration.GetRequiredConnectionString(configuration);
    }

    public async Task<List<(int Id, string Question, string Answer, string Embedding)>> GetAllKnowledgeAsync()
    {
        var result = new List<(int, string, string, string)>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Id, Question, Answer, Embedding FROM MutualFundKnowledge WHERE IsActive = 1", conn))
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

    public async Task<List<ChatHistory>> GetChatHistoryAsync(string userId, int count = 5)
    {
        var result = new List<ChatHistory>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = @"SELECT TOP (@Count) Id, UserId, Role, Message, CreatedDate 
                         FROM ChatHistory 
                         WHERE UserId = @UserId 
                         ORDER BY CreatedDate DESC";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Count", count);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new ChatHistory
                        {
                            Id = reader.GetInt32(0),
                            UserId = reader.GetString(1),
                            Role = reader.GetString(2),
                            Message = reader.GetString(3),
                            CreatedDate = reader.GetDateTime(4)
                        });
                    }
                }
            }
        }

        result.Reverse(); // Return in chronological order
        return result;
    }

    public async Task SaveChatHistoryAsync(ChatHistory chatHistory)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = @"INSERT INTO ChatHistory (UserId, Role, Message, CreatedDate) 
                         VALUES (@UserId, @Role, @Message, @CreatedDate)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", chatHistory.UserId);
                cmd.Parameters.AddWithValue("@Role", chatHistory.Role);
                cmd.Parameters.AddWithValue("@Message", chatHistory.Message);
                cmd.Parameters.AddWithValue("@CreatedDate", chatHistory.CreatedDate);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task SaveAiLogAsync(AiLog aiLog)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = @"INSERT INTO AiLog (UserId, Query, Response, ConfidenceScore, Intent, Source, CreatedDate) 
                         VALUES (@UserId, @Query, @Response, @ConfidenceScore, @Intent, @Source, @CreatedDate)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", aiLog.UserId);
                cmd.Parameters.AddWithValue("@Query", aiLog.Query);
                cmd.Parameters.AddWithValue("@Response", aiLog.Response);
                cmd.Parameters.AddWithValue("@ConfidenceScore", aiLog.ConfidenceScore);
                cmd.Parameters.AddWithValue("@Intent", aiLog.Intent ?? string.Empty);
                cmd.Parameters.AddWithValue("@Source", aiLog.Source ?? string.Empty);
                cmd.Parameters.AddWithValue("@CreatedDate", aiLog.CreatedDate);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeactivateKnowledgeAsync(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE MutualFundKnowledge SET IsActive = 0 WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task ActivateKnowledgeAsync(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE MutualFundKnowledge SET IsActive = 1 WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task UpdateKnowledgeVersionAsync(int id, int version)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE MutualFundKnowledge SET Version = @Version WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Version", version);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<Models.KnowledgeGap?> GetKnowledgeGapByQuestionAsync(string question)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = "SELECT Id, Question, DetectedIntent, ConfidenceScore, OccurrenceCount, LastAsked, Status, SuggestedAnswer, CreatedAt FROM KnowledgeGaps WHERE Question = @Question";
            
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Question", question);
                await conn.OpenAsync();
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Models.KnowledgeGap
                        {
                            Id = reader.GetInt32(0),
                            Question = reader.GetString(1),
                            DetectedIntent = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            ConfidenceScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                            OccurrenceCount = reader.GetInt32(4),
                            LastAsked = reader.GetDateTime(5),
                            Status = reader.GetString(6),
                            SuggestedAnswer = reader.IsDBNull(7) ? null : reader.GetString(7),
                            CreatedAt = reader.GetDateTime(8)
                        };
                    }
                }
            }
        }
        return null;
    }

    public async Task SaveKnowledgeGapAsync(Models.KnowledgeGap gap)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = @"INSERT INTO KnowledgeGaps (Question, DetectedIntent, ConfidenceScore, OccurrenceCount, LastAsked, Status, CreatedAt) 
                         VALUES (@Question, @DetectedIntent, @ConfidenceScore, @OccurrenceCount, @LastAsked, @Status, @CreatedAt)";
            
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Question", gap.Question);
                cmd.Parameters.AddWithValue("@DetectedIntent", gap.DetectedIntent);
                cmd.Parameters.AddWithValue("@ConfidenceScore", gap.ConfidenceScore);
                cmd.Parameters.AddWithValue("@OccurrenceCount", gap.OccurrenceCount);
                cmd.Parameters.AddWithValue("@LastAsked", gap.LastAsked);
                cmd.Parameters.AddWithValue("@Status", gap.Status);
                cmd.Parameters.AddWithValue("@CreatedAt", gap.CreatedAt);
                
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task UpdateKnowledgeGapAsync(Models.KnowledgeGap gap)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = @"UPDATE KnowledgeGaps SET OccurrenceCount = @OccurrenceCount, LastAsked = @LastAsked, Status = @Status, SuggestedAnswer = @SuggestedAnswer WHERE Id = @Id";
            
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", gap.Id);
                cmd.Parameters.AddWithValue("@OccurrenceCount", gap.OccurrenceCount);
                cmd.Parameters.AddWithValue("@LastAsked", gap.LastAsked);
                cmd.Parameters.AddWithValue("@Status", gap.Status);
                cmd.Parameters.AddWithValue("@SuggestedAnswer", (object?)gap.SuggestedAnswer ?? DBNull.Value);
                
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<List<Models.KnowledgeGap>> GetTopKnowledgeGapsAsync(int count)
    {
        var result = new List<Models.KnowledgeGap>();
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = $@"SELECT TOP {count} Id, Question, DetectedIntent, ConfidenceScore, OccurrenceCount, LastAsked, Status, SuggestedAnswer, CreatedAt 
                          FROM KnowledgeGaps 
                          WHERE Status != 'Resolved' 
                          ORDER BY OccurrenceCount DESC, LastAsked DESC";
            
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new Models.KnowledgeGap
                        {
                            Id = reader.GetInt32(0),
                            Question = reader.GetString(1),
                            DetectedIntent = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            ConfidenceScore = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                            OccurrenceCount = reader.GetInt32(4),
                            LastAsked = reader.GetDateTime(5),
                            Status = reader.GetString(6),
                            SuggestedAnswer = reader.IsDBNull(7) ? null : reader.GetString(7),
                            CreatedAt = reader.GetDateTime(8)
                        });
                    }
                }
            }
        }
        return result;
    }

    public async Task AddKnowledgeFromGapAsync(string question, string answer)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = @"INSERT INTO MutualFundKnowledge (Question, Answer, Version, IsActive) 
                         VALUES (@Question, @Answer, 1, 1)";
            
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Question", question);
                cmd.Parameters.AddWithValue("@Answer", answer);
                
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<int> GetAiLogCountAsync()
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM AiLog", conn))
        {
            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }

    public async Task<double> GetAverageConfidenceAsync()
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(AVG(ConfidenceScore), 0) FROM AiLog", conn))
        {
            await conn.OpenAsync();
            return Convert.ToDouble(await cmd.ExecuteScalarAsync());
        }
    }

    public async Task<int> GetActiveUserCountAsync(int days = 7)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            @"SELECT COUNT(DISTINCT UserId)
              FROM AiLog
              WHERE CreatedDate >= DATEADD(DAY, -@Days, GETUTCDATE())", conn))
        {
            cmd.Parameters.AddWithValue("@Days", days);
            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }

    public async Task<int> GetUnansweredKnowledgeGapCountAsync()
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT COUNT(*) FROM KnowledgeGaps WHERE Status <> 'Resolved'", conn))
        {
            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }

    public async Task<List<(DateTime Date, int Count)>> GetDailyQueryCountsAsync(int days = 7)
    {
        var result = new List<(DateTime Date, int Count)>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            @"SELECT CAST(CreatedDate AS DATE) AS QueryDate, COUNT(*) AS QueryCount
              FROM AiLog
              WHERE CreatedDate >= DATEADD(DAY, -@Days + 1, GETUTCDATE())
              GROUP BY CAST(CreatedDate AS DATE)
              ORDER BY QueryDate", conn))
        {
            cmd.Parameters.AddWithValue("@Days", days);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add((reader.GetDateTime(0), reader.GetInt32(1)));
            }
        }

        return result;
    }

    public async Task<List<(DateTime Date, double Value)>> GetDailyConfidenceTrendAsync(int days = 7)
    {
        var result = new List<(DateTime Date, double Value)>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            @"SELECT CAST(CreatedDate AS DATE) AS QueryDate, ISNULL(AVG(ConfidenceScore), 0) AS AvgConfidence
              FROM AiLog
              WHERE CreatedDate >= DATEADD(DAY, -@Days + 1, GETUTCDATE())
              GROUP BY CAST(CreatedDate AS DATE)
              ORDER BY QueryDate", conn))
        {
            cmd.Parameters.AddWithValue("@Days", days);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add((reader.GetDateTime(0), Convert.ToDouble(reader.GetValue(1))));
            }
        }

        return result;
    }

    public async Task<List<(string Category, int Count)>> GetIntentCategoryUsageAsync(int top = 10)
    {
        var result = new List<(string Category, int Count)>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            @"SELECT TOP (@Top)
                    CASE
                        WHEN Intent IS NULL OR LTRIM(RTRIM(Intent)) = '' THEN 'Unknown'
                        ELSE Intent
                    END AS Category,
                    COUNT(*) AS UsageCount
              FROM AiLog
              GROUP BY CASE
                        WHEN Intent IS NULL OR LTRIM(RTRIM(Intent)) = '' THEN 'Unknown'
                        ELSE Intent
                       END
              ORDER BY UsageCount DESC, Category ASC", conn))
        {
            cmd.Parameters.AddWithValue("@Top", top);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add((reader.GetString(0), reader.GetInt32(1)));
            }
        }

        return result;
    }

    public async Task<List<(string Query, int Count, double AvgConfidence)>> GetTrendingQueriesAsync(int top = 10)
    {
        var result = new List<(string Query, int Count, double AvgConfidence)>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            @"SELECT TOP (@Top) Query, COUNT(*) AS QueryCount, ISNULL(AVG(ConfidenceScore), 0) AS AvgConfidence
              FROM AiLog
              WHERE Query IS NOT NULL AND LTRIM(RTRIM(Query)) <> ''
              GROUP BY Query
              ORDER BY QueryCount DESC, AvgConfidence DESC", conn))
        {
            cmd.Parameters.AddWithValue("@Top", top);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add((
                    reader.GetString(0),
                    reader.GetInt32(1),
                    Convert.ToDouble(reader.GetValue(2))
                ));
            }
        }

        return result;
    }

    public async Task<List<Models.KnowledgeGap>> GetKnowledgeGapsAsync(bool includeResolved = true, int top = 100)
    {
        var result = new List<Models.KnowledgeGap>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = includeResolved
                ? @"SELECT TOP (@Top) Id, Question, DetectedIntent, ConfidenceScore, OccurrenceCount, LastAsked, Status, SuggestedAnswer, CreatedAt
                    FROM KnowledgeGaps
                    ORDER BY OccurrenceCount DESC, LastAsked DESC"
                : @"SELECT TOP (@Top) Id, Question, DetectedIntent, ConfidenceScore, OccurrenceCount, LastAsked, Status, SuggestedAnswer, CreatedAt
                    FROM KnowledgeGaps
                    WHERE Status <> 'Resolved'
                    ORDER BY OccurrenceCount DESC, LastAsked DESC";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Top", top);
                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new Models.KnowledgeGap
                    {
                        Id = reader.GetInt32(0),
                        Question = reader.GetString(1),
                        DetectedIntent = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        ConfidenceScore = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3)),
                        OccurrenceCount = reader.GetInt32(4),
                        LastAsked = reader.GetDateTime(5),
                        Status = reader.GetString(6),
                        SuggestedAnswer = reader.IsDBNull(7) ? null : reader.GetString(7),
                        CreatedAt = reader.GetDateTime(8)
                    });
                }
            }
        }

        return result;
    }

    public async Task<Models.KnowledgeGap?> GetKnowledgeGapByIdAsync(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(
            @"SELECT Id, Question, DetectedIntent, ConfidenceScore, OccurrenceCount, LastAsked, Status, SuggestedAnswer, CreatedAt
              FROM KnowledgeGaps
              WHERE Id = @Id", conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Models.KnowledgeGap
                {
                    Id = reader.GetInt32(0),
                    Question = reader.GetString(1),
                    DetectedIntent = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ConfidenceScore = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3)),
                    OccurrenceCount = reader.GetInt32(4),
                    LastAsked = reader.GetDateTime(5),
                    Status = reader.GetString(6),
                    SuggestedAnswer = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CreatedAt = reader.GetDateTime(8)
                };
            }
        }

        return null;
    }
}
