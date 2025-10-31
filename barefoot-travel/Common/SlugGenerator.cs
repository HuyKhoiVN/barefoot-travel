using System.Text;
using System.Text.RegularExpressions;

namespace barefoot_travel.Common
{
    /// <summary>
    /// Utility class for generating URL-friendly slugs from Vietnamese text
    /// </summary>
    public static class SlugGenerator
    {
        /// <summary>
        /// Generate slug from Vietnamese text
        /// </summary>
        /// <param name="text">Input text (Vietnamese or English)</param>
        /// <returns>URL-friendly slug</returns>
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Convert to lowercase
            string slug = text.ToLowerInvariant();

            // Replace Vietnamese characters
            slug = RemoveVietnameseTones(slug);

            // Replace common separators with hyphen first
            // This handles: "Ha Long - Cat Ba", "Part 1 / Part 2", "A & B", etc.
            slug = slug.Replace(" - ", "-");
            slug = slug.Replace(" / ", "-");
            slug = slug.Replace(" & ", "-");
            slug = slug.Replace(" + ", "-");
            slug = slug.Replace("–", "-");  // En dash
            slug = slug.Replace("—", "-");  // Em dash
            slug = slug.Replace("/", "-");
            slug = slug.Replace("&", "-");
            slug = slug.Replace("+", "-");

            // Remove all invalid characters (keep only a-z, 0-9, spaces, hyphens)
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Replace multiple spaces/hyphens with single hyphen
            slug = Regex.Replace(slug, @"[\s-]+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            return slug;
        }

        /// <summary>
        /// Remove Vietnamese tones and diacritics
        /// </summary>
        private static string RemoveVietnameseTones(string text)
        {
            var replacements = new Dictionary<string, string>
            {
                // Lowercase
                { "à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ", "a" },
                { "è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ", "e" },
                { "ì|í|ị|ỉ|ĩ", "i" },
                { "ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ", "o" },
                { "ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ", "u" },
                { "ỳ|ý|ỵ|ỷ|ỹ", "y" },
                { "đ", "d" },
                
                // Uppercase
                { "À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ", "a" },
                { "È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ", "e" },
                { "Ì|Í|Ị|Ỉ|Ĩ", "i" },
                { "Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ", "o" },
                { "Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ", "u" },
                { "Ỳ|Ý|Ỵ|Ỷ|Ỹ", "y" },
                { "Đ", "d" }
            };

            foreach (var replacement in replacements)
            {
                text = Regex.Replace(text, replacement.Key, replacement.Value);
            }

            return text;
        }

        /// <summary>
        /// Ensure slug is unique by appending suffix
        /// </summary>
        /// <param name="baseSlug">Base slug</param>
        /// <param name="existingSlugs">List of existing slugs</param>
        /// <returns>Unique slug</returns>
        public static string EnsureUnique(string baseSlug, IEnumerable<string> existingSlugs)
        {
            var slug = baseSlug;
            var counter = 1;

            while (existingSlugs.Contains(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        /// <summary>
        /// Validate slug format
        /// </summary>
        /// <param name="slug">Slug to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // Only lowercase letters, numbers, and hyphens
            var regex = new Regex(@"^[a-z0-9-]+$");
            return regex.IsMatch(slug) && 
                   slug.Length <= 300 && 
                   !slug.StartsWith("-") && 
                   !slug.EndsWith("-") &&
                   !slug.Contains("--");
        }
    }
}

