using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace ECommerce.Web.Services
{
    public class ImageProcessingService
    {
        private readonly string Watermark = "E-Commerce";

        public async Task<(string FileName, string ThumbName)> ProcessImageAsync(
            IFormFile file, string folder)
        {
            string root = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/{folder}");
            Directory.CreateDirectory(root);

            string ext = Path.GetExtension(file.FileName).ToLower();
            string fileName = Guid.NewGuid() + ext;
            string filePath = Path.Combine(root, fileName);

            string thumbName = "thumb_" + fileName;
            string thumbPath = Path.Combine(root, thumbName);

            using var image = await Image.LoadAsync(file.OpenReadStream());

            Font font = SystemFonts.CreateFont("Tahoma", 28);
            var textColor = Color.White;

            image.Mutate(x =>
            {
                x.DrawText(Watermark, font, textColor,
                    new PointF(10, image.Height - 50));
            });

            var encoder = new JpegEncoder { Quality = 75 };
            await image.SaveAsync(filePath, encoder);

            using var thumb = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(400, 400),
                Mode = ResizeMode.Crop
            }));

            await thumb.SaveAsync(thumbPath, encoder);

            return (fileName, thumbName);
        }

        public void DeleteImage(string? fileName, string folder)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            string root = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/{folder}");

            string original = Path.Combine(root, fileName);
            if (File.Exists(original)) File.Delete(original);

            string thumb = Path.Combine(root, "thumb_" + fileName);
            if (File.Exists(thumb)) File.Delete(thumb);
        }
    }
}
