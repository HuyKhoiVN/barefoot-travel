using barefoot_travel.Common;
using barefoot_travel.DTOs.Category;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeaturedDailyToursController : ControllerBase
    {
        private readonly IFeaturedDailyToursService _service;
        private readonly ILogger<FeaturedDailyToursController> _logger;

        public FeaturedDailyToursController(IFeaturedDailyToursService service, ILogger<FeaturedDailyToursController> logger)
        {
            _service = service;
            _logger = logger;
        }

        #region Featured Tours

        [HttpGet("featured-tours")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeaturedTours()
        {
            try
            {
                var result = await _service.GetFeaturedToursAsync();
                return Ok(new ApiResponse(true, "Featured tours retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured tours");
                return BadRequest(new ApiResponse(false, "Failed to get featured tours"));
            }
        }

        [HttpPut("featured-tours/{tourId}")]
        public async Task<IActionResult> ConfigureFeaturedTour(int tourId, [FromBody] ConfigureFeaturedTourDto dto)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _service.ConfigureFeaturedTourAsync(tourId, dto, userId);
                return Ok(new ApiResponse(true, "Featured tour configured successfully", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring featured tour");
                return BadRequest(new ApiResponse(false, "Failed to configure featured tour"));
            }
        }

        [HttpPatch("featured-tours/{id}/update")]
        public async Task<IActionResult> UpdateFeaturedTour(int id, [FromBody] ConfigureFeaturedTourDto dto)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _service.UpdateFeaturedTourAsync(id, dto, userId);
                return Ok(new ApiResponse(true, "Featured tour updated successfully", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating featured tour");
                return BadRequest(new ApiResponse(false, "Failed to update featured tour"));
            }
        }

        [HttpDelete("featured-tours/{id}")]
        public async Task<IActionResult> RemoveFeaturedTour(int id)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _service.RemoveFeaturedTourAsync(id, userId);
                return Ok(new ApiResponse(true, "Featured tour removed successfully", true));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing featured tour");
                return BadRequest(new ApiResponse(false, "Failed to remove featured tour"));
            }
        }

        [HttpPut("featured-tours/reorder")]
        public async Task<IActionResult> ReorderFeaturedTours([FromBody] List<ReorderTourDto> orders)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _service.ReorderFeaturedToursAsync(orders, userId);
                return Ok(new ApiResponse(true, "Featured tours reordered successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering featured tours");
                return BadRequest(new ApiResponse(false, "Failed to reorder featured tours"));
            }
        }

        #endregion

        #region Daily Tours

        [HttpGet("daily-tours")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDailyTours()
        {
            try
            {
                var result = await _service.GetDailyToursAsync();
                return Ok(new ApiResponse(true, "Daily tours retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily tours");
                return BadRequest(new ApiResponse(false, "Failed to get daily tours"));
            }
        }

        [HttpPut("daily-tours/{categoryId}")]
        public async Task<IActionResult> ConfigureDailyTour(int categoryId, [FromBody] ConfigureDailyTourDto dto)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _service.ConfigureDailyTourAsync(categoryId, dto, userId);
                return Ok(new ApiResponse(true, "Daily tour configured successfully", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring daily tour");
                return BadRequest(new ApiResponse(false, "Failed to configure daily tour"));
            }
        }

        [HttpDelete("daily-tours/{categoryId}")]
        public async Task<IActionResult> RemoveDailyTour(int categoryId)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _service.RemoveDailyTourAsync(categoryId, userId);
                return Ok(new ApiResponse(true, "Daily tour removed successfully", true));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing daily tour");
                return BadRequest(new ApiResponse(false, "Failed to remove daily tour"));
            }
        }

        [HttpPut("daily-tours/reorder")]
        public async Task<IActionResult> ReorderDailyTours([FromBody] List<ReorderTourDto> orders)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _service.ReorderDailyToursAsync(orders, userId);
                return Ok(new ApiResponse(true, "Daily tours reordered successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering daily tours");
                return BadRequest(new ApiResponse(false, "Failed to reorder daily tours"));
            }
        }

        #endregion
    }
}
