using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BookStore.Validation
{
    public static class ImageUploadValidator
    {
        public const int MaxFileSizeMb = 5;
        public const long MaxFileSizeBytes = MaxFileSizeMb * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

        /// <summary>
        /// Validates an optional image upload. Returns Success when file is null or empty.
        /// </summary>
        public static ValidationResult? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return ValidationResult.Success;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            {
                return new ValidationResult(
                    "Only JPG, JPEG, PNG, and WEBP images are allowed.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return new ValidationResult(
                    $"Image size must not exceed {MaxFileSizeMb} MB.");
            }

            if (!HasValidImageSignature(file))
            {
                return new ValidationResult(
                    "The uploaded file is not a valid image or its content does not match the file type.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Defense-in-depth guard for BookService before writing to disk.
        /// </summary>
        public static void EnsureValid(IFormFile file)
        {
            var result = Validate(file);

            if (result != null && result != ValidationResult.Success)
            {
                throw new InvalidOperationException(result.ErrorMessage);
            }
        }

        private static bool HasValidImageSignature(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            Span<byte> header = stackalloc byte[12];
            var bytesRead = stream.Read(header);

            if (bytesRead < 3)
            {
                return false;
            }

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return true;
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (bytesRead >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                return true;
            }

            // WebP: "RIFF" .... "WEBP"
            if (bytesRead >= 12 &&
                header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            {
                return true;
            }

            return false;
        }
    }
}