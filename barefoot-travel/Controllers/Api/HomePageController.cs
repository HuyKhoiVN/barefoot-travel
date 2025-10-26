using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.Services;
using barefoot_travel.DTOs.Category;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HomePageController : ControllerBase
    {
        private readonly IHomePageService _homePageService;
        private readonly ILogger<HomePageController> _logger;

        public HomePageController(
            IHomePageService homePageService,
            ILogger<HomePageController> logger)
        {
            _homePageService = homePageService;
            _logger = logger;
        }

        /// <summary>
        /// Get all homepage sections with tours
        /// </summary>
        [HttpGet("sections")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHomepageSections()
        {
            try
            {
                var result = await _homePageService.GetHomepageSectionsAsync();
                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting homepage sections");
                return BadRequest(new ApiResponse(false, "Failed to get homepage sections"));
            }
        }

        /// <summary>
        /// Configure a category for homepage display
        /// </summary>
        [HttpPut("category/{categoryId}/homepage")]
        public async Task<IActionResult> ConfigureCategoryHomepage(int categoryId, [FromBody] ConfigureHomepageDto dto)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _homePageService.ConfigureCategoryHomepageAsync(categoryId, dto, userId);
                return Ok(new ApiResponse(true, "Category configured for homepage", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring category for homepage");
                return BadRequest(new ApiResponse(false, "Failed to configure category"));
            }
        }

        /// <summary>
        /// Remove category from homepage
        /// </summary>
        [HttpDelete("category/{categoryId}/homepage")]
        public async Task<IActionResult> RemoveFromHomepage(int categoryId)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _homePageService.RemoveCategoryFromHomepageAsync(categoryId, userId);
                return Ok(new ApiResponse(true, "Category removed from homepage", true));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing category from homepage");
                return BadRequest(new ApiResponse(false, "Failed to remove category from homepage"));
            }
        }

        /// <summary>
        /// Update section order
        /// </summary>
        [HttpPut("sections/reorder")]
        public async Task<IActionResult> ReorderSections([FromBody] List<ReorderSectionDto> sections)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _homePageService.ReorderSectionsAsync(sections, userId);
                return Ok(new ApiResponse(true, "Sections reordered successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering sections");
                return BadRequest(new ApiResponse(false, "Failed to reorder sections"));
            }
        }

        /// <summary>
        /// Get category homepage configuration
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetCategoryHomepageConfig(int categoryId)
        {
            try
            {
                var result = await _homePageService.GetCategoryHomepageConfigAsync(categoryId);
                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category homepage config");
                return BadRequest(new ApiResponse(false, "Failed to get category config"));
            }
        }

        #region Ways to Travel Endpoints

        /// <summary>
        /// Get all Ways to Travel categories
        /// </summary>
        [HttpGet("ways-to-travel")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWaysToTravelCategories()
        {
            try
            {
                var result = await _homePageService.GetWaysToTravelCategoriesAsync();
                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Ways to Travel categories");
                return BadRequest(new ApiResponse(false, "Failed to get Ways to Travel categories"));
            }
        }

        /// <summary>
        /// Configure category for Ways to Travel
        /// </summary>
        [HttpPut("category/{categoryId}/ways-to-travel")]
        public async Task<IActionResult> ConfigureCategoryForWaysToTravel(int categoryId, [FromBody] ConfigureWaysToTravelDto dto)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _homePageService.ConfigureCategoryForWaysToTravelAsync(categoryId, dto, userId);
                return Ok(new ApiResponse(true, "Category configured for Ways to Travel", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring category for Ways to Travel");
                return BadRequest(new ApiResponse(false, "Failed to configure category"));
            }
        }

        /// <summary>
        /// Remove category from Ways to Travel
        /// </summary>
        [HttpDelete("category/{categoryId}/ways-to-travel")]
        public async Task<IActionResult> RemoveFromWaysToTravel(int categoryId)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _homePageService.RemoveCategoryFromWaysToTravelAsync(categoryId, userId);
                return Ok(new ApiResponse(true, "Category removed from Ways to Travel", true));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing category from Ways to Travel");
                return BadRequest(new ApiResponse(false, "Failed to remove category from Ways to Travel"));
            }
        }

        /// <summary>
        /// Reorder Ways to Travel categories
        /// </summary>
        [HttpPut("ways-to-travel/reorder")]
        public async Task<IActionResult> ReorderWaysToTravelCategories([FromBody] List<ReorderSectionDto> orders)
        {
            try
            {
                var userId = Common.GetUserIdFromClaims.GetUserId(this.User).ToString();
                await _homePageService.ReorderWaysToTravelCategoriesAsync(orders, userId);
                return Ok(new ApiResponse(true, "Categories reordered successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering Ways to Travel categories");
                return BadRequest(new ApiResponse(false, "Failed to reorder categories"));
            }
        }

        #endregion
    }
}
