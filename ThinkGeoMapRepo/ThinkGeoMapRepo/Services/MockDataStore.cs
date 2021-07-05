using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThinkGeoMapRepo.Models;

namespace ThinkGeoMapRepo.Services
{
    public class MockDataStore : IDataStore<Item>
    {
        readonly List<Item> items;
        readonly List<LocationModel> locations;
        readonly List<TeamMemberDeviceModel> teamMemberDevices;

        public MockDataStore()
        {
            items = new List<Item>()
            {
                new Item { Id = Guid.NewGuid().ToString(), Text = "First item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Second item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Third item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Fourth item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Fifth item", Description="This is an item description." },
                new Item { Id = Guid.NewGuid().ToString(), Text = "Sixth item", Description="This is an item description." }
            };

            locations = new List<LocationModel>()
            {
                new LocationModel()
                {
                    LocationId = "Office",
                    Description = "An Office in ACT",
                    Name = "Office",
                    Geometry = new Point(new Position(-35.2771027, 149.128207))
                },
                new LocationModel()
                {
                    LocationId = "Office2",
                    Description = "2nd Office in ACT",
                    Name = "Office2",
                    Geometry = new Point(new Position(-35.2765078, 149.1287285))
                },
                new LocationModel()
                {
                    LocationId = "lunchPlace",
                    Description = "A place to have lunch",
                    Name = "Happy's Chinese Restaurant",
                    Geometry = new Point(new Position(-35.2782583, 149.1318264))
                }
            };

            teamMemberDevices = new List<TeamMemberDeviceModel>()
            {
                new TeamMemberDeviceModel()
                {
                    DeviceId = "TeamMember1",
                    Description = "Jim having lunch",
                    Name = "Jim's Device",
                    Geometry = new Point(new Position(-35.2782500, 149.1318264))
                },
                new TeamMemberDeviceModel()
                {
                    DeviceId = "TeamMemberMoving1",
                    Description = "Patrol Car looping office",
                    Name = "Patrol Car Device",
                    Geometry = new Point(new Position(-35.2765078, 149.1287285))
                },
                new TeamMemberDeviceModel()
                {
                    DeviceId = "TeamMemberMoving2",
                    Description = "Northbourne Cruising",
                    Name = "Northbourne Cruising Device",
                    Geometry = new Point(new Position(-35.2771684, 149.1324447))
                },
            };
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var oldItem = items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }

        public async Task<List<LocationModel>> GetLocations()
        {
            return await Task.FromResult(locations);
        }

        public async Task<List<TeamMemberDeviceModel>> GetTeamMemberDevices()
        {
            return await Task.FromResult(teamMemberDevices);
        }
    }
}