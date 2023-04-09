using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Services.External
{
    public class ImageManager : IImageManager
    {
        private readonly string _cloudName;
        private readonly string _apiKey;
        private readonly string _secret;

        public ImageManager(string cloudName, string apiKey, string secret)
        {
            _cloudName = cloudName;
            _apiKey = apiKey;
            _secret = secret;
        }

        public async Task<string> UploadImageAsync(MemoryStream ms, string fileName)
        {
            ms.Position = 0;
            var cloudinaryAccount = new Account(_cloudName, _apiKey, _secret);
            var cloudinary = new Cloudinary(cloudinaryAccount);

            var publicId = Guid.NewGuid().ToString() + fileName;
            var file = new FileDescription(fileName, ms);

            var uploadParams = new ImageUploadParams
            {
                File = file,
                Format = "jpg",
                PublicId = publicId,
                UseFilename = true,
            };

            uploadParams.Check();
            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            var uri = uploadResult.SecureUrl;
            return uri?.AbsoluteUri;
        }
    }
}
