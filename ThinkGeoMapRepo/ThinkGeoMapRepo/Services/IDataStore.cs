using System.Collections.Generic;
using System.Threading.Tasks;
using ThinkGeoMapRepo.Models;

namespace ThinkGeoMapRepo.Services
{
    public interface IDataStore<T>
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(string id);
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);

        Task<List<LocationModel>> GetLocations();
        Task<List<TeamMemberDeviceModel>> GetTeamMemberDevices();
    }
}
