using Nero.Core.Entities;

namespace Nero.Core.Services
{
    public interface ICollectionService
    {
        Task<List<Collection>> GetAll();
        Task<List<Collection>> GetBy(string userId);
    }
}