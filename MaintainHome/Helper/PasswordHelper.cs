using System.Security.Cryptography;
using System.Text;

namespace MaintainHome.Helper
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as necessary
                throw new Exception($"An error occurred while hashing the password: {ex.Message}");
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                string hashedInputPassword = HashPassword(password);
                return hashedInputPassword == hashedPassword;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as necessary
                throw new Exception($"An error occurred while verifying the password: {ex.Message}");
            }
        }
    }
}
