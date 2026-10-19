using System;
using System.Security.Cryptography;
using System.Text;

namespace SportZone.WinForms.Security
{
    public static class PasswordSecurity
    {
        public const int DefaultSaltSize = 32;
        public const int DefaultIterations = 10000;
        public const int DefaultHashByteSize = 32;

        public static byte[] GenerateSalt(int size = DefaultSaltSize)
        {
            byte[] salt = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static string GenerateSecureTemporaryPassword(int length = 14)
        {
            if (length < 8) length = 8;
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%&*?";
            string allChars = upper + lower + digits + special;

            var result = new char[length];
            byte[] randomBytes = new byte[length + 4];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            // Ensure at least one from each required set
            result[0] = upper[randomBytes[0] % upper.Length];
            result[1] = lower[randomBytes[1] % lower.Length];
            result[2] = digits[randomBytes[2] % digits.Length];
            result[3] = special[randomBytes[3] % special.Length];

            for (int i = 4; i < length; i++)
            {
                result[i] = allChars[randomBytes[i] % allChars.Length];
            }

            // Shuffle using Fisher-Yates
            for (int i = length - 1; i > 0; i--)
            {
                int j = randomBytes[i] % (i + 1);
                char temp = result[i];
                result[i] = result[j];
                result[j] = temp;
            }

            return new string(result);
        }

        public static string HashPassword(string password, byte[] salt, int iterations = DefaultIterations)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (salt == null || salt.Length == 0) throw new ArgumentException("Salt cannot be null or empty", nameof(salt));

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(DefaultHashByteSize);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(string password, byte[] salt, string storedHash, int iterations = DefaultIterations)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash) || salt == null || salt.Length == 0)
            {
                return false;
            }

            try
            {
                string computedHash = HashPassword(password, salt, iterations);
                return SlowEquals(computedHash, storedHash);
            }
            catch
            {
                return false;
            }
        }

        public static bool VerifyPassword(string password, string storedHash, byte[] salt, int iterations = DefaultIterations, string algorithm = "PBKDF2-SHA256")
        {
            return VerifyPassword(password, salt, storedHash, iterations);
        }

        private static bool SlowEquals(string a, string b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        public static string GenerateSecureOtp(int digits = 6)
        {
            if (digits < 4) digits = 4;
            if (digits > 8) digits = 8;
            int maxVal = (int)Math.Pow(10, digits);

            byte[] bytes = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            uint val = BitConverter.ToUInt32(bytes, 0) % (uint)maxVal;
            return val.ToString(new string('0', digits));
        }

        public static string HashOtp(string otp)
        {
            if (string.IsNullOrEmpty(otp)) return string.Empty;
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp.Trim()));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool VerifyOtp(string inputOtp, string storedHash)
        {
            if (string.IsNullOrEmpty(inputOtp) || string.IsNullOrEmpty(storedHash)) return false;
            string inputHash = HashOtp(inputOtp);
            return SlowEquals(inputHash, storedHash);
        }

        public static bool ValidatePasswordPolicy(string password, out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                errorMessage = "Mật khẩu phải có độ dài tối thiểu 8 ký tự.";
                return false;
            }

            bool hasLetter = false;
            bool hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsLetter(c)) hasLetter = true;
                if (char.IsDigit(c)) hasDigit = true;
            }

            if (!hasLetter || !hasDigit)
            {
                errorMessage = "Mật khẩu phải bao gồm cả chữ cái và chữ số.";
                return false;
            }

            return true;
        }
    }
}
