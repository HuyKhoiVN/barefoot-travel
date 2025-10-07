using System.Security.Claims;

namespace barefoot_travel.Common
{
    public static class GetUserIdFromClaims
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định User ID.");
            }

            return userId;
        }

        public static string GetUsername(ClaimsPrincipal user)
        {
            var usernameClaim = user.FindFirst("username")?.Value;

            if (string.IsNullOrEmpty(usernameClaim))
            {
                throw new UnauthorizedAccessException("Không thể xác định Username.");
            }

            return usernameClaim;
        }
    }
}