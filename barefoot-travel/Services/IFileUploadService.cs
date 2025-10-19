using barefoot_travel.Common;
using System.Security.Cryptography;
using System.Text;

namespace barefoot_travel.Services
{
    public interface IFileUploadService
    {
        Task<ApiResponse> UploadImageAsync(IFormFile file, string folder = "uploads");
        Task<ApiResponse> DeleteImageAsync(string fileName, string folder = "uploads");
        Task<ApiResponse> GetImagesAsync(string folder = "uploads");
        Task<ApiResponse> AnalyzeUnusedImagesAsync(string htmlContent, string folder = "uploads");
        Task<ApiResponse> CleanupUnusedImagesAsync(string htmlContent, string folder = "uploads");
        Task<ApiResponse> CleanupUnusedDescriptionImagesAsync(string htmlContent, string folder = "uploads");
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<ApiResponse> UploadImageAsync(IFormFile file, string folder = "uploads")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new ApiResponse(false, "No file provided");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new ApiResponse(false, "Invalid file type. Only JPG, JPEG, PNG, GIF, and WebP files are allowed");
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return new ApiResponse(false, "File size too large. Maximum size is 10MB");
                }

                // Generate unique filename
                var fileName = GenerateUniqueFileName(file.FileName);
                
                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, folder);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Save file
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Generate URL
                var fileUrl = $"/{folder}/{fileName}";

                _logger.LogInformation("File uploaded successfully: {FileName}", fileName);

                return new ApiResponse(true, "File uploaded successfully", new
                {
                    fileName = fileName,
                    originalName = file.FileName,
                    fileUrl = fileUrl,
                    fileSize = file.Length,
                    uploadedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return new ApiResponse(false, "An error occurred while uploading the file");
            }
        }

        public async Task<ApiResponse> DeleteImageAsync(string fileName, string folder = "uploads")
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return new ApiResponse(false, "File name is required");
                }

                var filePath = Path.Combine(_environment.WebRootPath, folder, fileName);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return new ApiResponse(false, "File not found");
                }

                System.IO.File.Delete(filePath);

                _logger.LogInformation("File deleted successfully: {FileName}", fileName);

                return new ApiResponse(true, "File deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
                return new ApiResponse(false, "An error occurred while deleting the file");
            }
        }

        public async Task<ApiResponse> GetImagesAsync(string folder = "uploads")
        {
            try
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, folder);
                
                if (!Directory.Exists(uploadPath))
                {
                    return new ApiResponse(true, "No images found", new List<object>());
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var imageFiles = Directory.GetFiles(uploadPath)
                    .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .Select(file => new
                    {
                        fileName = Path.GetFileName(file),
                        fileUrl = $"/{folder}/{Path.GetFileName(file)}",
                        fileSize = new FileInfo(file).Length,
                        createdTime = File.GetCreationTime(file),
                        modifiedTime = File.GetLastWriteTime(file)
                    })
                    .OrderByDescending(f => f.modifiedTime)
                    .ToList();

                return new ApiResponse(true, "Images retrieved successfully", imageFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving images");
                return new ApiResponse(false, "An error occurred while retrieving images");
            }
        }

        public async Task<ApiResponse> AnalyzeUnusedImagesAsync(string htmlContent, string folder = "uploads")
        {
            try
            {
                if (string.IsNullOrEmpty(htmlContent))
                {
                    return new ApiResponse(true, "No HTML content provided", new List<object>());
                }

                var uploadPath = Path.Combine(_environment.WebRootPath, folder);
                
                if (!Directory.Exists(uploadPath))
                {
                    return new ApiResponse(true, "No images found", new List<object>());
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var allImageFiles = Directory.GetFiles(uploadPath)
                    .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .Select(file => Path.GetFileName(file))
                    .ToList();

                var usedImages = new HashSet<string>();
                
                // Extract image URLs from HTML content
                var imgTagPattern = @"<img[^>]+src=[""']([^""']+)[""'][^>]*>";
                var matches = System.Text.RegularExpressions.Regex.Matches(htmlContent, imgTagPattern);
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var src = match.Groups[1].Value;
                    var fileName = Path.GetFileName(src);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        usedImages.Add(fileName);
                    }
                }

                // Find unused images
                var unusedImages = allImageFiles
                    .Where(fileName => !usedImages.Contains(fileName))
                    .Select(fileName => new
                    {
                        fileName = fileName,
                        fileUrl = $"/{folder}/{fileName}",
                        filePath = Path.Combine(uploadPath, fileName),
                        fileSize = new FileInfo(Path.Combine(uploadPath, fileName)).Length,
                        createdTime = File.GetCreationTime(Path.Combine(uploadPath, fileName))
                    })
                    .ToList();

                return new ApiResponse(true, "Analysis completed successfully", new
                {
                    totalImages = allImageFiles.Count,
                    usedImages = usedImages.Count,
                    unusedImages = unusedImages.Count,
                    unusedImageList = unusedImages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing unused images");
                return new ApiResponse(false, "An error occurred while analyzing images");
            }
        }

        public async Task<ApiResponse> CleanupUnusedImagesAsync(string htmlContent, string folder = "uploads")
        {
            try
            {
                if (string.IsNullOrEmpty(htmlContent))
                {
                    return new ApiResponse(true, "No HTML content provided", new { deletedCount = 0 });
                }

                var uploadPath = Path.Combine(_environment.WebRootPath, folder);
                
                if (!Directory.Exists(uploadPath))
                {
                    return new ApiResponse(true, "No images found", new { deletedCount = 0 });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var allImageFiles = Directory.GetFiles(uploadPath)
                    .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .Select(file => Path.GetFileName(file))
                    .ToList();

                var usedImages = new HashSet<string>();
                
                // Extract image URLs from HTML content
                var imgTagPattern = @"<img[^>]+src=[""']([^""']+)[""'][^>]*>";
                var matches = System.Text.RegularExpressions.Regex.Matches(htmlContent, imgTagPattern);
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var src = match.Groups[1].Value;
                    var fileName = Path.GetFileName(src);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        usedImages.Add(fileName);
                    }
                }

                // Delete unused images
                var deletedCount = 0;
                var deletedFiles = new List<string>();
                
                foreach (var fileName in allImageFiles)
                {
                    if (!usedImages.Contains(fileName))
                    {
                        var filePath = Path.Combine(uploadPath, fileName);
                        try
                        {
                            System.IO.File.Delete(filePath);
                            deletedCount++;
                            deletedFiles.Add(fileName);
                            _logger.LogInformation("Deleted unused image: {FileName}", fileName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete image: {FileName}", fileName);
                        }
                    }
                }

                return new ApiResponse(true, "Cleanup completed successfully", new
                {
                    deletedCount = deletedCount,
                    deletedFiles = deletedFiles,
                    totalImages = allImageFiles.Count,
                    remainingImages = allImageFiles.Count - deletedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up unused images");
                return new ApiResponse(false, "An error occurred while cleaning up images");
            }
        }

        public async Task<ApiResponse> CleanupUnusedDescriptionImagesAsync(string htmlContent, string folder = "uploads")
        {
            try
            {
                if (string.IsNullOrEmpty(htmlContent))
                {
                    return new ApiResponse(true, "No HTML content provided", new { deletedCount = 0 });
                }

                var uploadPath = Path.Combine(_environment.WebRootPath, folder);
                
                if (!Directory.Exists(uploadPath))
                {
                    return new ApiResponse(true, "No images found", new { deletedCount = 0 });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var allImageFiles = Directory.GetFiles(uploadPath)
                    .Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    .Select(file => Path.GetFileName(file))
                    .ToList();

                var usedImages = new HashSet<string>();
                
                // Extract image URLs from HTML content (only from description)
                var imgTagPattern = @"<img[^>]+src=[""']([^""']+)[""'][^>]*>";
                var matches = System.Text.RegularExpressions.Regex.Matches(htmlContent, imgTagPattern);
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var src = match.Groups[1].Value;
                    var fileName = Path.GetFileName(src);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        usedImages.Add(fileName);
                    }
                }

                // Only delete images that are not used in description AND are older than 1 hour
                var deletedCount = 0;
                var deletedFiles = new List<string>();
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                
                foreach (var fileName in allImageFiles)
                {
                    if (!usedImages.Contains(fileName))
                    {
                        var filePath = Path.Combine(uploadPath, fileName);
                        var fileInfo = new FileInfo(filePath);
                        
                        // Only delete if file is older than 1 hour to avoid deleting recently uploaded tour images
                        if (fileInfo.CreationTimeUtc < oneHourAgo)
                        {
                            try
                            {
                                System.IO.File.Delete(filePath);
                                deletedCount++;
                                deletedFiles.Add(fileName);
                                _logger.LogInformation("Deleted unused description image: {FileName}", fileName);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to delete image: {FileName}", fileName);
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Skipping recent file (might be tour image): {FileName}", fileName);
                        }
                    }
                }

                return new ApiResponse(true, "Description cleanup completed successfully", new
                {
                    deletedCount = deletedCount,
                    deletedFiles = deletedFiles,
                    totalImages = allImageFiles.Count,
                    remainingImages = allImageFiles.Count - deletedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up unused description images");
                return new ApiResponse(false, "An error occurred while cleaning up description images");
            }
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            
            // Generate hash from original name and timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var hash = ComputeHash(nameWithoutExtension + timestamp);
            
            return $"{hash}{extension}";
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();
            }
        }
    }
}
