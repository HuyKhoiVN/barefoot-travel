using System.Security.Claims;

namespace barefoot_travel.Common
{
    public static class GetUserIdFromClaims
    {
        public static Guid GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định User ID.");
            }

            return userId;
        }
    }
}