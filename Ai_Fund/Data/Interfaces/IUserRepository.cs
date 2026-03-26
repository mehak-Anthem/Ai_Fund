using Ai_Fund.Models;

namespace Ai_Fund.Data.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int userId);

    Task CreateAsync(User user);
}
