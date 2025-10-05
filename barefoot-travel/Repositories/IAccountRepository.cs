using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetUserByUsernameAsync(string username);
        Task<Account?> GetByIdAsync(int id);
        Task<List<Account>> GetAllAsync();
        Task<Account> CreateAsync(Account account);
        Task<Account> UpdateAsync(Account account);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
