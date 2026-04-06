using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartCare.Domain.Constants
{
    public static class Constants
    {
        public const string EMAIL_REGEX = @"^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,6}$";
        public const string USERNAME_REGEX = @"^[a-zA-Z0-9._]{3,20}$";
        public const string PASS_REGEX = @"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=!])(?=\S+$).{12,}$";
        public const string EGYPT_PHONE_REGEX = @"^(?:\+20|0020|0)?1[0|1|2|5]\d{8}$";
        public const long MaxImgSize = 5 * 1024 * 1024;
        public static readonly List<string> AllowedImageExtensions = new List<string>() { ".png", ".jpeg", ".jpg" };
        private static readonly List<string> _allowedExtensions = new List<string>() { ".mp3", ".wav", ".m4a" };
        private const long MaxaudioFileSize = 10 * 1024 * 1024; // 10 MB


        public enum StringType
        {
            EMAIL,
            USERNAME,
            PASSWORD,
            LETTERS_ONLY,
            PHONE_NO,
            LETTERS_NUMS_UNDERSCORE
        }

        public static bool IsValid(StringType type, string checkString)
        {
            string pattern = type switch
            {
                StringType.EMAIL => EMAIL_REGEX,
                StringType.USERNAME => USERNAME_REGEX,
                StringType.PASSWORD => PASS_REGEX,
                StringType.PHONE_NO => EGYPT_PHONE_REGEX,
                _ => throw new ArgumentException($"Unknown type: {type}")
            };

            return Regex.IsMatch(checkString, pattern);
        }
        public static bool BeAValidImage(IFormFile file)
        {
            if (file == null) return true;

            var ext = Path.GetExtension(file.FileName).ToLower();
            return AllowedImageExtensions.Contains(ext) && file.Length <= MaxImgSize;
        }
        public static bool IsValidAudioFile(IFormFile file)
        {
            if (file == null)
                return false;

            if (file.Length == 0)
                return false;

            if (file.Length > MaxaudioFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName)?.ToLower();
            if (!_allowedExtensions.Contains(extension))
                return false;

            if (string.IsNullOrEmpty(file.ContentType) || !file.ContentType.StartsWith("audio/"))
                return false;

            return true;
        }
   
        public static List<string> GetPasswordErrors(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(password)) return errors;
            if (password.Contains(" "))
                errors.Add("Password cannot contain spaces.");
            if (password.Length < 12)
                errors.Add("Password must be at least 12 characters long.");

            if (!Regex.IsMatch(password, "[A-Z]"))
                errors.Add("Password must contain an uppercase letter.");

            if (!Regex.IsMatch(password, "[a-z]"))
                errors.Add("Password must contain a lowercase letter.");

            if (!Regex.IsMatch(password, "[0-9]"))
                errors.Add("Password must contain a digit.");

            if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                errors.Add("Password must contain a special character.");

            return errors;
        }
    }
}
