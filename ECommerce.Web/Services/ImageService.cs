using Microsoft.AspNetCore.Http;

namespace ECommerce.Web.Services
{
    public class ImageService
    {
        private readonly string _rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        public ImageService()
        {
            Directory.CreateDirectory(_rootPath);
        }

    
        public async Task<string?> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;

            string ext = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedExtensions.Contains(ext))
                throw new Exception("Invalid file type.");

            if (file.Length > 2 * 1024 * 1024)
                throw new Exception("File too large (max 2MB).");

            string folderPath = Path.Combine(_rootPath, folder);
            Directory.CreateDirectory(folderPath);

            string safeName = Path.GetFileNameWithoutExtension(file.FileName)
                                    .Replace(" ", "-")
                                    .Replace(".", "-")
                                    .Replace("/", "-");

            string fileName = $"{Guid.NewGuid()}_{safeName}{ext}";

            string filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

       
        public void Delete(string? fileName, string folder)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default.png")
                return;

            string folderPath = Path.Combine(_rootPath, folder);
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        
        public async Task<string?> ReplaceAsync(string? oldFile, IFormFile? newFile, string folder)
        {
            if (newFile == null)
                return oldFile;

            Delete(oldFile, folder);

            return await UploadAsync(newFile, folder);
        }
    }
}
