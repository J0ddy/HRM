using System.IO;
using System.Threading.Tasks;

namespace Services.External
{
    public interface IImageManager
    {
        public Task<string> UploadImageAsync(MemoryStream ms, string fileName);
    }
}
