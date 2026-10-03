using Ecom.core.Exceptions;
using Ecom.core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Ecom.infrastructure.Services
{
    public class ImageManagementService : IImageManagementService
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        private const string ImagesFolder = "Images";

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly string _webRoot;

        public ImageManagementService(IWebHostEnvironment environment)
        {
            _webRoot = Path.GetFullPath(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"));
        }

        public async Task<List<string>> AddImageAsync(IFormFileCollection files, string folder)
        {
            var uploads = files.Where(f => f.Length > 0).ToList();
            foreach (var file in uploads)
            {
                Validate(file);
            }

            var safeFolder = ToSafeFolderName(folder);
            var directory = Path.Combine(_webRoot, ImagesFolder, safeFolder);
            Directory.CreateDirectory(directory);

            var savedPaths = new List<string>();
            try
            {
                foreach (var file in uploads)
                {
                    var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                    savedPaths.Add($"{ImagesFolder}/{safeFolder}/{fileName}");

                    await using var stream = new FileStream(Path.Combine(directory, fileName), FileMode.CreateNew);
                    await file.CopyToAsync(stream);
                }
            }
            catch
            {
                foreach (var path in savedPaths)
                {
                    DeleteImage(path);
                }
                throw;
            }
            return savedPaths;
        }

        public void DeleteImage(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
                return;

            var fullPath = Path.GetFullPath(Path.Combine(_webRoot, src.TrimStart('/', '\\')));
            if (!fullPath.StartsWith(_webRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                return;

            try
            {
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                var directory = Path.GetDirectoryName(fullPath);
                if (directory is not null && Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                    Directory.Delete(directory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Cleanup is best effort: a locked file must not fail an otherwise successful request.
            }
        }

        private static void Validate(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException($"File '{Path.GetFileName(file.FileName)}' is not an allowed image type ({string.Join(", ", AllowedExtensions)}).");

            if (file.Length > MaxFileSizeBytes)
                throw new BadRequestException($"File '{Path.GetFileName(file.FileName)}' exceeds the {MaxFileSizeBytes / 1024 / 1024} MB limit.");
        }

        private static string ToSafeFolderName(string name)
        {
            var safe = Regex.Replace(name ?? string.Empty, "[^a-zA-Z0-9_-]+", "-").Trim('-');
            if (safe.Length > 50)
                safe = safe[..50];
            return safe.Length == 0 ? "product" : safe;
        }
    }
}
