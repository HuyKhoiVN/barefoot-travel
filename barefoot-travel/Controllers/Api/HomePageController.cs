using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.Services;
using barefoot_travel.DTOs.Category;
using barefoot_travel.DTOs.Section;
using CategoryReorderDto = barefoot_travel.DTOs.Category.ReorderSectionDto;
using SectionReorderDto = barefoot_travel.DTOs.Section.ReorderSectionDto;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HomePageController : ControllerBase
    {
        private readonly IHomePageService _homePageService;
        private readonly IHomePageSectionService _sectionService;
        private readonly ILogger<HomePageController> _logger;

        public HomePageController(
            IHomePageService homePageService,
            IHomePageSectionService sectionService,
            ILogger<HomePageController> logger)
        {
            _homePageService = homePageService;
            _sectionService = sectionService;
            _logger = logger;
        }

        #region New Section Management Endpoints

        /// <summary>
        /// Get all homepage sections from HomePageSection table
        /// </summary>
        [HttpGet("sections")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllSections()
        {
            try
            {
                var result = await _sectionService.GetAllSectionsAsync();
                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sections");
                return BadRequest(new ApiResponse(false, "Failed to get sections"));
            }
        }

        /// <summary>
        /// Get section by ID
        /// </summary>
        [HttpGet("sections/{sectionId}")]
        public async Task<IActionResult> GetSectionById(int sectionId)
        {
            try
            {
                var result = await _sectionService.GetSectionByIdAsync(sectionId);
                if (result == null)
                    return NotFound(new ApiResponse(false, "Section not found"));

                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section {SectionId}", sectionId);
                return BadRequest(new ApiResponse(false, "Failed to get section"));
            }
        }

        /// <summary>
        /// Create a new homepage section
        /// </summary>
        [HttpPost("sections")]
        public async Task<IActionResult> CreateSection([FromBody] ConfigureHomePageSectionDto dto)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                var result = await _sectionService.CreateSectionAsync(dto, username);
                return CreatedAtAction(nameof(GetSectionById), new { sectionId = result.Id }, 
                    new ApiResponse(true, "Section created successfully", result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating section");
                return StatusCode(500, new ApiResponse(false, "Failed to create section"));
            }
        }

        /// <summary>
        /// Update a homepage section
        /// </summary>
        [HttpPut("sections/{sectionId}")]
        public async Task<IActionResult> UpdateSection(int sectionId, [FromBody] ConfigureHomePageSectionDto dto)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                var result = await _sectionService.UpdateSectionAsync(sectionId, dto, username);
                return Ok(new ApiResponse(true, "Section updated successfully", result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating section {SectionId}", sectionId);
                return StatusCode(500, new ApiResponse(false, "Failed to update section"));
            }
        }

        /// <summary>
        /// Delete a homepage section
        /// </summary>
        [HttpDelete("sections/{sectionId}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                var result = await _sectionService.DeleteSectionAsync(sectionId, username);
                if (!result)
                    return NotFound(new ApiResponse(false, "Section not found"));

                return Ok(new ApiResponse(true, "Section deleted successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting section {SectionId}", sectionId);
                return StatusCode(500, new ApiResponse(false, "Failed to delete section"));
            }
        }

        /// <summary>
        /// Reorder sections
        /// </summary>
        [HttpPut("sections/reorder")]
        public async Task<IActionResult> ReorderSections([FromBody] List<SectionReorderDto> sections)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _sectionService.ReorderSectionsAsync(sections, username);
                return Ok(new ApiResponse(true, "Sections reordered successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering sections");
                return BadRequest(new ApiResponse(false, "Failed to reorder sections"));
            }
        }

        /// <summary>
        /// Get tours for a section
        /// </summary>
        [HttpGet("sections/{sectionId}/tours")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSectionTours(int sectionId)
        {
            try
            {
                var result = await _sectionService.GetSectionToursAsync(sectionId);
                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tours for section {SectionId}", sectionId);
                return BadRequest(new ApiResponse(false, "Failed to get section tours"));
            }
        }

        /// <summary>
        /// Get section preview (for admin UI)
        /// </summary>
        [HttpGet("sections/{sectionId}/preview")]
        public async Task<IActionResult> GetSectionPreview(int sectionId)
        {
            try
            {
                var result = await _sectionService.GetSectionPreviewAsync(sectionId);
                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting section preview");
                return BadRequest(new ApiResponse(false, "Failed to get section preview"));
            }
        }

        /// <summary>
        /// Get available tours for section (manual mode)
        /// </summary>
        [HttpGet("sections/{sectionId}/available-tours")]
        public async Task<IActionResult> GetAvailableToursForSection(int sectionId)
        {
            try
            {
                var result = await _sectionService.GetAvailableToursForSectionAsync(sectionId);
                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tours for section");
                return BadRequest(new ApiResponse(false, "Failed to get available tours"));
            }
        }

        /// <summary>
        /// Add tours to section (manual mode)
        /// </summary>
        [HttpPost("sections/{sectionId}/tours")]
        public async Task<IActionResult> AddToursToSection(int sectionId, [FromBody] List<int> tourIds)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _sectionService.AddToursToSectionAsync(sectionId, tourIds, username);
                return Ok(new ApiResponse(true, "Tours added to section successfully", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tours to section");
                return StatusCode(500, new ApiResponse(false, "Failed to add tours"));
            }
        }

        /// <summary>
        /// Remove tour from section
        /// </summary>
        [HttpDelete("sections/{sectionId}/tours/{tourId}")]
        public async Task<IActionResult> RemoveTourFromSection(int sectionId, int tourId)
        {
            try
            {
                await _sectionService.RemoveTourFromSectionAsync(sectionId, tourId);
                return Ok(new ApiResponse(true, "Tour removed from section successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tour from section");
                return BadRequest(new ApiResponse(false, "Failed to remove tour"));
            }
        }

        /// <summary>
        /// Reorder tours in section
        /// </summary>
        [HttpPut("sections/{sectionId}/tours/order")]
        public async Task<IActionResult> ReorderSectionTours(int sectionId, [FromBody] List<int> tourIds)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _sectionService.ReorderSectionToursAsync(sectionId, tourIds, username);
                return Ok(new ApiResponse(true, "Tours reordered successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering section tours");
                return StatusCode(500, new ApiResponse(false, "Failed to reorder tours"));
            }
        }

        /// <summary>
        /// Add categories to section
        /// </summary>
        [HttpPost("sections/{sectionId}/categories")]
        public async Task<IActionResult> AddCategoriesToSection(int sectionId, [FromBody] List<int> categoryIds)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _sectionService.AddCategoriesToSectionAsync(sectionId, categoryIds, username);
                return Ok(new ApiResponse(true, "Categories added to section successfully", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding categories to section");
                return StatusCode(500, new ApiResponse(false, "Failed to add categories"));
            }
        }

        /// <summary>
        /// Remove category from section
        /// </summary>
        [HttpDelete("sections/{sectionId}/categories/{categoryId}")]
        public async Task<IActionResult> RemoveCategoryFromSection(int sectionId, int categoryId)
        {
            try
            {
                await _sectionService.RemoveCategoryFromSectionAsync(sectionId, categoryId);
                return Ok(new ApiResponse(true, "Category removed from section successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing category from section");
                return BadRequest(new ApiResponse(false, "Failed to remove category"));
            }
        }

        #endregion

        #region Old Category-based Homepage Endpoints (Keep for backward compatibility)

        /// <summary>
        /// [DEPRECATED] Get homepage sections (old implementation using Category.HomepageConfig)
        /// </summary>
        [HttpGet("sections/legacy")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHomepageSectionsLegacy()
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
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _homePageService.ConfigureCategoryHomepageAsync(categoryId, dto, username);
                return Ok(new ApiResponse(true, "Category configured for homepage", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring category for homepage");
                return StatusCode(500, new ApiResponse(false, "Failed to configure category"));
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
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _homePageService.RemoveCategoryFromHomepageAsync(categoryId, username);
                return Ok(new ApiResponse(true, "Category removed from homepage", true));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing category from homepage");
                return StatusCode(500, new ApiResponse(false, "Failed to remove category from homepage"));
            }
        }

        /// <summary>
        /// [DEPRECATED] Update section order (old implementation using Category)
        /// </summary>
        [HttpPut("sections/reorder/legacy")]
        public async Task<IActionResult> ReorderSectionsLegacy([FromBody] List<CategoryReorderDto> sections)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _homePageService.ReorderSectionsAsync(sections, username);
                return Ok(new ApiResponse(true, "Sections reordered successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering sections (legacy)");
                return StatusCode(500, new ApiResponse(false, "Failed to reorder sections"));
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

        /// <summary>
        /// Get selected tours for a category
        /// </summary>
        [HttpGet("category/{categoryId}/selected-tours")]
        public async Task<IActionResult> GetSelectedTours(int categoryId)
        {
            try
            {
                var result = await _homePageService.GetSelectedToursAsync(categoryId);
                return Ok(new ApiResponse(true, "Success", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting selected tours");
                return BadRequest(new ApiResponse(false, "Failed to get selected tours"));
            }
        }

        #endregion

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
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _homePageService.ConfigureCategoryForWaysToTravelAsync(categoryId, dto, username);
                return Ok(new ApiResponse(true, "Category configured for Ways to Travel", true));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring category for Ways to Travel");
                return StatusCode(500, new ApiResponse(false, "Failed to configure category"));
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
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _homePageService.RemoveCategoryFromWaysToTravelAsync(categoryId, username);
                return Ok(new ApiResponse(true, "Category removed from Ways to Travel", true));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing category from Ways to Travel");
                return StatusCode(500, new ApiResponse(false, "Failed to remove category from Ways to Travel"));
            }
        }

        /// <summary>
        /// Reorder Ways to Travel categories
        /// </summary>
        [HttpPut("ways-to-travel/reorder")]
        public async Task<IActionResult> ReorderWaysToTravelCategories([FromBody] List<CategoryReorderDto> orders)
        {
            try
            {
                var username = GetUserIdFromClaims.GetUsername(this.User);
                await _homePageService.ReorderWaysToTravelCategoriesAsync(orders, username);
                return Ok(new ApiResponse(true, "Categories reordered successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering Ways to Travel categories");
                return StatusCode(500, new ApiResponse(false, "Failed to reorder categories"));
            }
        }

        #endregion
    }
}
