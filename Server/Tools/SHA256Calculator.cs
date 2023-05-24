using System.Security.Cryptography;
using System.Text;

namespace Server.Tools;

public static class SHA256Calculator
{
    public static byte[] Calculate(string str)
    {
        SHA256 sha256 = SHA256.Create();
        UTF8Encoding objUtf8 = new();
        byte[] hashValue = sha256.ComputeHash(objUtf8.GetBytes(str));

        return hashValue;
    }
}
