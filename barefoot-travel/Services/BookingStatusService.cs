using barefoot_travel.Common;
using barefoot_travel.DTOs.BookingStatus;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services
{
    public class BookingStatusService : IBookingStatusService
    {
        private readonly IBookingStatusRepository _bookingStatusRepository;

        public BookingStatusService(IBookingStatusRepository bookingStatusRepository)
        {
            _bookingStatusRepository = bookingStatusRepository;
        }

        public async Task<ApiResponse> GetBookingStatusByIdAsync(int id)
        {
            try
            {
                var bookingStatus = await _bookingStatusRepository.GetByIdAsync(id);
                if (bookingStatus == null)
                {
                    return new ApiResponse(false, "Booking status not found");
                }

                var bookingStatusDto = MapToBookingStatusDto(bookingStatus);
                return new ApiResponse(true, "Success", bookingStatusDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetAllBookingStatusesAsync()
        {
            try
            {
                var bookingStatuses = await _bookingStatusRepository.GetAllAsync();
                var bookingStatusDtos = MapToBookingStatusDtoList(bookingStatuses);
                return new ApiResponse(true, "Success", bookingStatusDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse> CreateBookingStatusAsync(CreateBookingStatusEntityDto dto)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(dto.StatusName))
                {
                    return new ApiResponse(false, "Status name is required");
                }

                // Check if name already exists
                var exists = await _bookingStatusRepository.ExistsByNameAsync(dto.StatusName);
                if (exists)
                {
                    return new ApiResponse(false, "Status name already exists");
                }

                var bookingStatus = MapToBookingStatus(dto);
                var createdBookingStatus = await _bookingStatusRepository.CreateAsync(bookingStatus);
                var bookingStatusDto = MapToBookingStatusDto(createdBookingStatus);

                return new ApiResponse(true, "Booking status created successfully", bookingStatusDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateBookingStatusAsync(int id, UpdateBookingStatusEntityDto dto)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(dto.StatusName))
                {
                    return new ApiResponse(false, "Status name is required");
                }

                var existingBookingStatus = await _bookingStatusRepository.GetByIdAsync(id);
                if (existingBookingStatus == null)
                {
                    return new ApiResponse(false, "Booking status not found");
                }

                // Check if name already exists (excluding current record)
                var exists = await _bookingStatusRepository.ExistsByNameAsync(dto.StatusName, id);
                if (exists)
                {
                    return new ApiResponse(false, "Status name already exists");
                }

                MapToBookingStatusForUpdate(existingBookingStatus, dto);
                var updatedBookingStatus = await _bookingStatusRepository.UpdateAsync(existingBookingStatus);
                var bookingStatusDto = MapToBookingStatusDto(updatedBookingStatus);

                return new ApiResponse(true, "Booking status updated successfully", bookingStatusDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteBookingStatusAsync(int id)
        {
            try
            {
                var exists = await _bookingStatusRepository.ExistsAsync(id);
                if (!exists)
                {
                    return new ApiResponse(false, "Booking status not found");
                }

                var success = await _bookingStatusRepository.DeleteAsync(id);
                if (success)
                {
                    return new ApiResponse(true, "Booking status deleted successfully");
                }
                else
                {
                    return new ApiResponse(false, "Failed to delete booking status");
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        // Manual mapping methods
        private BookingStatusEntityDto MapToBookingStatusDto(BookingStatus bookingStatus)
        {
            if (bookingStatus == null) return null;

            return new BookingStatusEntityDto
            {
                Id = bookingStatus.Id,
                StatusName = bookingStatus.StatusName,
                CreatedTime = bookingStatus.CreatedTime,
                UpdatedTime = bookingStatus.UpdatedTime,
                UpdatedBy = bookingStatus.UpdatedBy,
                Active = bookingStatus.Active
            };
        }

        private List<BookingStatusEntityDto> MapToBookingStatusDtoList(List<BookingStatus> bookingStatuses)
        {
            if (bookingStatuses == null || !bookingStatuses.Any()) return new List<BookingStatusEntityDto>();

            return bookingStatuses.Select(MapToBookingStatusDto).Where(dto => dto != null).ToList();
        }

        private BookingStatus MapToBookingStatus(CreateBookingStatusEntityDto dto)
        {
            if (dto == null) return null;

            return new BookingStatus
            {
                StatusName = dto.StatusName,
                Active = true,
                CreatedTime = DateTime.UtcNow
            };
        }

        private void MapToBookingStatusForUpdate(BookingStatus existingBookingStatus, UpdateBookingStatusEntityDto dto)
        {
            if (existingBookingStatus == null || dto == null) return;

            existingBookingStatus.StatusName = dto.StatusName;
            existingBookingStatus.UpdatedTime = DateTime.UtcNow;
        }
    }
}
