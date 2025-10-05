using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IAuthRepository
    {
        Task<Account> GetUserByEmail(string email);

        Task<bool> EmailExists(string email);

        Task<bool> PhoneNumberExists(string phoneNumber);

        Task<bool> UserCodeExistsAsync(string userCode);

        public Task AddUserRole(RolePermission userRole);

        Task<bool> AnyUserExists();

        Task AddUser(Account user);

        Task SaveChanges();
    }
}
