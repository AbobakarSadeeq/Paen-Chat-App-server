using Business_Core.IServices;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Presentation.AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public class ImageCloudStorageService : IImageCloudStorageService
    {
        private Cloudinary _cloudinary;
        public ImageCloudStorageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }
        public async Task<string> StoringImageToCloudAsync(IFormFile photoFile)
        {
            string profilePhoneUrl = "";
            if (photoFile != null)
            {
                // store into cloudinary that image
                var uploadResult = new ImageUploadResult();

                using (var stream = photoFile.OpenReadStream())
                {
                    var uploadparams = new ImageUploadParams
                    {
                        File = new FileDescription(photoFile.Name, stream)
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadparams);
                }
                profilePhoneUrl = uploadResult.Url.ToString();
            }
            return profilePhoneUrl;
        }
    }
}
