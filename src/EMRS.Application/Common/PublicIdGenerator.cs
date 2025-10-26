using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Common;

public static class PublicIdGenerator
{
 
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
}
