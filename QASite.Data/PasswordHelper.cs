using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QASite.Data
{
   public class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool PasswordMatch(string userInput, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(userInput, passwordHash);
        }
    }
}
