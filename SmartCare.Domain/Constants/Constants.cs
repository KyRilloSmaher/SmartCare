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
    }
}
