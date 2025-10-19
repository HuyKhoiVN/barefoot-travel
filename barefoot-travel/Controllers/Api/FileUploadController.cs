using barefoot_travel.Common;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(IFileUploadService fileUploadService, ILogger<FileUploadController> logger)
        {
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        /// <summary>
        /// Upload image file
        /// </summary>
        /// <param name="file">Image file to upload</param>
        /// <param name="folder">Folder to save the image (optional, default: 'uploads')</param>
        /// <returns>Uploaded file information</returns>
        /// <response code="200">File uploaded successfully</response>
        /// <response code="400">Invalid file or file type</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "uploads")
        {
            _logger.LogInformation("Image upload attempt for file: {FileName}", file?.FileName);
            var result = await _fileUploadService.UploadImageAsync(file, folder);
            if (result.Success)
            {
                _logger.LogInformation("Image uploaded successfully: {FileUrl}", result.Data);
                return Ok(result);
            }
            _logger.LogWarning("Image upload failed: {Message}", result.Message);
            return BadRequest(result);
        }

        /// <summary>
        /// Delete image file
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="folder">Folder where the file is located (optional, default: 'uploads')</param>
        /// <returns>Success message</returns>
        /// <response code="200">File deleted successfully</response>
        /// <response code="404">File not found</response>
        /// <response code="401">Unauthorized</response>
        [HttpDelete("image")]
        public async Task<IActionResult> DeleteImage([FromQuery] string fileName, [FromQuery] string folder = "uploads")
        {
            _logger.LogInformation("Image delete attempt for file: {FileName}", fileName);
            var result = await _fileUploadService.DeleteImageAsync(fileName, folder);
            if (result.Success)
            {
                _logger.LogInformation("Image deleted successfully: {FileName}", fileName);
                return Ok(result);
            }
            _logger.LogWarning("Image delete failed: {Message}", result.Message);
            return BadRequest(result);
        }

        /// <summary>
        /// Get list of uploaded images
        /// </summary>
        /// <param name="folder">Folder to list images from (optional, default: 'uploads')</param>
        /// <returns>List of uploaded images</returns>
        /// <response code="200">Images retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("images")]
        public async Task<IActionResult> GetImages([FromQuery] string folder = "uploads")
        {
            _logger.LogInformation("Getting images from folder: {Folder}", folder);
            var result = await _fileUploadService.GetImagesAsync(folder);
            return Ok(result);
        }

        /// <summary>
        /// Extract images from HTML content and return unused images
        /// </summary>
        /// <param name="request">Analysis request with HTML content and folder</param>
        /// <returns>List of unused images</returns>
        /// <response code="200">Analysis completed successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("analyze-unused-images")]
        public async Task<IActionResult> AnalyzeUnusedImages([FromBody] AnalyzeImagesRequest request)
        {
            _logger.LogInformation("Analyzing unused images in folder: {Folder}", request.Folder);
            var result = await _fileUploadService.AnalyzeUnusedImagesAsync(request.HtmlContent, request.Folder ?? "uploads");
            return Ok(result);
        }

        /// <summary>
        /// Clean up unused images
        /// </summary>
        /// <param name="request">Cleanup request with HTML content and folder</param>
        /// <returns>Cleanup results</returns>
        /// <response code="200">Cleanup completed successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("cleanup-unused-images")]
        public async Task<IActionResult> CleanupUnusedImages([FromBody] AnalyzeImagesRequest request)
        {
            _logger.LogInformation("Cleaning up unused images in folder: {Folder}", request.Folder);
            var result = await _fileUploadService.CleanupUnusedImagesAsync(request.HtmlContent, request.Folder ?? "uploads");
            return Ok(result);
        }

        /// <summary>
        /// Clean up unused description images (safer cleanup that preserves recent tour images)
        /// </summary>
        /// <param name="request">Cleanup request with HTML content and folder</param>
        /// <returns>Cleanup results</returns>
        /// <response code="200">Cleanup completed successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("cleanup-unused-description-images")]
        public async Task<IActionResult> CleanupUnusedDescriptionImages([FromBody] AnalyzeImagesRequest request)
        {
            _logger.LogInformation("Cleaning up unused description images in folder: {Folder}", request.Folder);
            var result = await _fileUploadService.CleanupUnusedDescriptionImagesAsync(request.HtmlContent, request.Folder ?? "uploads");
            return Ok(result);
        }

    }

    public class AnalyzeImagesRequest
    {
        public string HtmlContent { get; set; } = string.Empty;
        public string? Folder { get; set; } = "uploads";
    }
}
