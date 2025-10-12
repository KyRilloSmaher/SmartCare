using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Domain.Enums;
using SmartCare.Domain.Helpers;



namespace SmartCare.InfraStructure.ExternalServices

{
    public class ImageUploaderService : IImageUploaderService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _cloudinarySettings;

        public ImageUploaderService(CloudinarySettings cloudinarySettings)
        {
            this._cloudinarySettings = cloudinarySettings ?? throw new ArgumentNullException(nameof(cloudinarySettings));

            if (_cloudinarySettings == null)
            {
                throw new ArgumentNullException(nameof(_cloudinarySettings),
                    "Cloudinary settings not found in configuration");
            }

            var account = new Account(
                _cloudinarySettings.CloudName,
            _cloudinarySettings.ApiKey,
                _cloudinarySettings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = Path.GetFileNameWithoutExtension(fileName),
                Overwrite = true,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentNullException(nameof(publicId));

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile imageFile, ImageFolder imageFolder)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("Image file is required");

            var folderName = GetFolderNameForImageType(imageFolder);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

            using var stream = imageFile.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                PublicId = $"{folderName}/{Path.GetFileNameWithoutExtension(fileName)}",
                Overwrite = true,
                Folder = folderName,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
            }

            return uploadResult;
        }

        public async Task<bool> DeleteImageByUrlAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentNullException(nameof(imageUrl));

            var publicId = ExtractPublicIdFromUrl(imageUrl);
            return await DeleteImageAsync(publicId);
        }

        public async Task<IEnumerable<ImageUploadResult>> UploadMultipleImagesAsync(
            IEnumerable<IFormFile> imageFiles, ImageFolder imageType)
        {
            if (imageFiles == null || !imageFiles.Any())
                throw new ArgumentException("At least one image file is required");

            var uploadTasks = imageFiles.Select(file => UploadImageAsync(file, imageType));
            return await Task.WhenAll(uploadTasks);
        }

        public async Task<string> GetImageUrlAsync(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                throw new ArgumentNullException(nameof(imageName));

            var result = await _cloudinary.GetResourceAsync(imageName);
            return result.SecureUrl;
        }

        private string GetFolderNameForImageType(ImageFolder imageType)
        {
            return imageType switch
            {
                ImageFolder.UserProfiles => "UserProfilesImages",
                ImageFolder.ProductImages => "ProductImages",
                ImageFolder.CategoryImages => "CategoryImages",
                ImageFolder.BrandLogos => "BrandLogos",
                _ => "others"
            };
        }

        public string ExtractPublicIdFromUrl(string url)
        {
            try
            {

                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');


                var filename = segments[^1];


                var publicId = Path.GetFileNameWithoutExtension(filename);

                // If the image is in a folder, combine segments
                if (segments.Length > 5)
                {
                    var folderPath = string.Join("/", segments.Skip(5).Take(segments.Length - 6));
                    return string.IsNullOrEmpty(folderPath) ? publicId : $"{folderPath}/{publicId}";
                }

                return publicId;
            }
            catch
            {
                return null;
            }
        }
    }
}
