using barefoot_travel.Common;
using barefoot_travel.DTOs.Tour;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TourImageController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly ILogger<TourImageController> _logger;

        public TourImageController(ITourService tourService, ILogger<TourImageController> logger)
        {
            _tourService = tourService;
            _logger = logger;
        }

        /// <summary>
        /// Get all images for a specific tour
        /// </summary>
        /// <param name="tourId">Tour ID</param>
        /// <returns>List of tour images</returns>
        /// <response code="200">Images retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Tour not found</response>
        [HttpGet("tour/{tourId}")]
        public async Task<IActionResult> GetTourImages(int tourId)
        {
            try
            {
                _logger.LogInformation("Getting images for tour ID: {TourId}", tourId);
                var result = await _tourService.GetTourImagesAsync(tourId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour images for tour ID: {TourId}", tourId);
                return StatusCode(500, new ApiResponse(false, "An error occurred while retrieving tour images"));
            }
        }

        /// <summary>
        /// Upload and create a new tour image
        /// </summary>
        /// <param name="tourId">Tour ID</param>
        /// <param name="file">Image file</param>
        /// <param name="isBanner">Whether this image is a banner image</param>
        /// <returns>Created tour image</returns>
        /// <response code="200">Image uploaded and created successfully</response>
        /// <response code="400">Invalid file or tour ID</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("tour/{tourId}")]
        public async Task<IActionResult> UploadTourImage(int tourId, IFormFile file, [FromQuery] bool isBanner = false)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ApiResponse(false, "No file provided"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new ApiResponse(false, "Invalid file type. Only JPG, JPEG, PNG, GIF, and WebP files are allowed"));
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new ApiResponse(false, "File size too large. Maximum size is 10MB"));
                }

                _logger.LogInformation("Uploading image for tour ID: {TourId}, IsBanner: {IsBanner}", tourId, isBanner);

                // Create DTO for tour image creation
                var createImageDto = new CreateTourImageDto
                {
                    TourId = tourId,
                    ImageUrl = "", // Will be set by the service after file upload
                    IsBanner = isBanner
                };

                var adminUsername = GetUserIdFromClaims.GetUsername(User);
                var result = await _tourService.UploadTourImageAsync(createImageDto, file, adminUsername);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading tour image for tour ID: {TourId}", tourId);
                return StatusCode(500, new ApiResponse(false, "An error occurred while uploading the image"));
            }
        }

        /// <summary>
        /// Update tour image (set as banner or change other properties)
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <param name="dto">Image update data</param>
        /// <returns>Updated tour image</returns>
        /// <response code="200">Image updated successfully</response>
        /// <response code="400">Invalid input</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Image not found</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTourImage(int id, [FromBody] UpdateTourImageDto dto)
        {
            try
            {
                _logger.LogInformation("Updating tour image with ID: {ImageId}", id);
                var adminUsername = GetUserIdFromClaims.GetUsername(User);
                var result = await _tourService.UpdateTourImageAsync(id, dto, adminUsername);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tour image with ID: {ImageId}", id);
                return StatusCode(500, new ApiResponse(false, "An error occurred while updating the image"));
            }
        }

        /// <summary>
        /// Set image as banner (automatically unsets other banner images for the same tour)
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <returns>Success message</returns>
        /// <response code="200">Banner status updated successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Image not found</response>
        [HttpPut("{id}/set-banner")]
        public async Task<IActionResult> SetAsBanner(int id)
        {
            try
            {
                _logger.LogInformation("Setting image as banner: {ImageId}", id);
                var adminUsername = GetUserIdFromClaims.GetUsername(User);
                var result = await _tourService.SetTourImageAsBannerAsync(id, adminUsername);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting image as banner: {ImageId}", id);
                return StatusCode(500, new ApiResponse(false, "An error occurred while setting banner status"));
            }
        }

        /// <summary>
        /// Remove banner status from image
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <returns>Success message</returns>
        /// <response code="200">Banner status removed successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Image not found</response>
        [HttpPut("{id}/remove-banner")]
        public async Task<IActionResult> RemoveBanner(int id)
        {
            try
            {
                _logger.LogInformation("Removing banner status from image: {ImageId}", id);
                var adminUsername = GetUserIdFromClaims.GetUsername(User);
                var result = await _tourService.RemoveTourImageBannerAsync(id, adminUsername);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing banner status from image: {ImageId}", id);
                return StatusCode(500, new ApiResponse(false, "An error occurred while removing banner status"));
            }
        }

        /// <summary>
        /// Delete tour image
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <returns>Success message</returns>
        /// <response code="200">Image deleted successfully</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Image not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTourImage(int id)
        {
            try
            {
                _logger.LogInformation("Deleting tour image with ID: {ImageId}", id);
                var adminUsername = GetUserIdFromClaims.GetUsername(User);
                var result = await _tourService.DeleteTourImageAsync(id, adminUsername);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tour image with ID: {ImageId}", id);
                return StatusCode(500, new ApiResponse(false, "An error occurred while deleting the image"));
            }
        }
    }
}
