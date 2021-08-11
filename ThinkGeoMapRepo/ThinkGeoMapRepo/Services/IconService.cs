using System.IO;

namespace ThinkGeoMapRepo.Services
{
    public class IconService
    {
        public Stream GetIconEmbeddedResource(string resourceName)
        {
            Stream sourceStream = this.GetType().Assembly.GetManifestResourceStream(resourceName);

            return sourceStream;
        }
    }
}
