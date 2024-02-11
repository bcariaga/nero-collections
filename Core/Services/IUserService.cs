using Nero.Core.Entities;

namespace Nero.Core.Services
{
    public interface IUserService
    {
        Task<User?> GetBy(string userId);
    }
}