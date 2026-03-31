using Ai_Fund.Configuration;
using Ai_Fund.Data.Interfaces;
using Ai_Fund.Models;
using System.Data;
using System.Data.SqlClient;

namespace Ai_Fund.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = AppConfiguration.GetRequiredConnectionString(configuration);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var query = "SELECT UserId, Username, PasswordHash, Email, Role, CreatedAt FROM Users WHERE Username = @Username";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                UserId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Role = reader.GetString(4),
                                CreatedAt = reader.GetDateTime(5)

                            };
                        }
                    }
                }
            }
        }
        catch (SqlException ex) when (ex.Number == 208) // Invalid object name (table missing)
        {
            Console.WriteLine($"Database Table 'Users' missing: {ex.Message}");
            throw new Exception("The 'Users' table does not exist in the database. Please run the schema script.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user: {ex.Message}");
            throw;
        }
        return null;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            var query = "SELECT UserId, Username, PasswordHash, Email, Role, CreatedAt FROM Users WHERE UserId = @UserId";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", id);
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Role = reader.GetString(4),
                            CreatedAt = reader.GetDateTime(5)

                        };
                    }
                }
            }
        }
        return null;
    }

    public async Task CreateAsync(User user)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var query = "INSERT INTO Users (Username, PasswordHash, Email, Role, CreatedAt) VALUES (@Username, @PasswordHash, @Email, @Role, @CreatedAt); SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Role", user.Role);
                    cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                    await conn.OpenAsync();
                    var insertedId = await cmd.ExecuteScalarAsync();
                    if (insertedId != null && insertedId != DBNull.Value)
                    {
                        user.UserId = Convert.ToInt32(insertedId);
                    }
                }

            }
        }
        catch (SqlException ex) when (ex.Number == 208)
        {
            Console.WriteLine($"Database Table 'Users' missing: {ex.Message}");
            throw new Exception("The 'Users' table does not exist. Please run the EnterpriseSchema.sql script.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
            throw;
        }
    }
}
