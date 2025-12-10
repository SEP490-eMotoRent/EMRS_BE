using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Helper;

public static class Generator
{

    public static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
    public static string BookingCodeGenerate()
    {
        // Ví dụ: BK20251102-9F3C7A
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper(); // 6 ký tự ngẫu nhiên
        return $"BK{datePart}-{randomPart}";
    }
    public static string PublicIdGenerate(int length = 6)
    {
        string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        int AlphabetLength = Alphabet.Length;

        if (length <= 0)
            throw new ArgumentException("Length must be greater than zero.", nameof(length));

        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        var sb = new StringBuilder(length);
        foreach (var b in bytes)
        {
            sb.Append(Alphabet[b % AlphabetLength]);
        }

        return sb.ToString();
    }

    public static string TransactionCodeGenerate()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"TX{timestamp}{random}";
    }

}
