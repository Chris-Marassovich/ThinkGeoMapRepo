using GeoJSON.Net.Geometry;

namespace ThinkGeoMapRepo.Models
{
    public class TeamMemberDeviceModel
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// A geographic coordinate reference based on the GeoJSON format.
        /// </summary>
        /// <see cref="https://github.com/GeoJSON-Net/GeoJSON.Net"/>
        public IGeometryObject Geometry { get; set; }

        public TeamMemberDeviceModel()
        {
        }

        public override string ToString()
        {
            return $"Device Id: {DeviceId}, Name: {Name}";
        }
    }
}
