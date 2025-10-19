using gameProject.Models;

namespace gameProject.Repositories.Interfaces
{
    public interface IUserRepository
    {
        //Get operations
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByAuthUuidAsync(string authUuid);
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllAsync();

        //Create Operation
        Task<User> CreateAsync(User user);
        //Update Operation
        Task<User> UpdateAsync(User user);
        //Delete Operation
        Task<bool> DeleteAsync(int id);

        //Existence Checks
        Task<bool> ExistsByAuthUuidAsync(string authUuid);
        Task<bool> ExistsByUsernameAsync(string username);
        
    }
}