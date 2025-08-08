using System;
using System.Security.Cryptography;
using System.Text;

namespace InventoryERP.Infrastructure.Utils;

public static class PasswordHasher
{
    // 生成随机盐值
    public static string GenerateSalt()
    {
        byte[] saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }
    
    // 使用HMACSHA512算法生成密码哈希
    public static string HashPassword(string password, string salt)
    {
        using (var hmac = new HMACSHA512(Convert.FromBase64String(salt)))
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = hmac.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    // 验证密码
    public static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        var computedHash = HashPassword(password, storedSalt);
        return computedHash == storedHash;
    }
}