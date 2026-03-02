using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Bacera.Gateway.Auth.Security;

/// <summary>
/// ASP.NET Identity V3 password hash verification and hashing.
///
/// Binary format (header byte 0x01):
///   [0]           = 0x01  (format marker)
///   [1..4]        = PRF   (big-endian uint32: 1=HMACSHA256, 2=HMACSHA512)
///   [5..8]        = iteration count (big-endian uint32)
///   [9..12]       = salt length (big-endian uint32)
///   [13..13+slen] = salt bytes
///   [13+slen..]   = derived sub-key bytes
/// </summary>
public static class PasswordVerifier
{
    public static bool VerifyHashedPasswordV3(string hashedPasswordBase64, string password)
    {
        byte[] hashedPassword;
        try
        {
            hashedPassword = Convert.FromBase64String(hashedPasswordBase64);
        }
        catch
        {
            return false;
        }

        if (hashedPassword.Length == 0 || hashedPassword[0] != 0x01)
            return false;
        if (hashedPassword.Length < 13)
            return false;

        var prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
        var iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
        var saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

        if (saltLength < 16)
            return false;
        if (hashedPassword.Length < 13 + saltLength)
            return false;

        var salt = new byte[saltLength];
        Buffer.BlockCopy(hashedPassword, 13, salt, 0, saltLength);

        var subKeyLength = hashedPassword.Length - 13 - saltLength;
        if (subKeyLength < 16)
            return false;

        var expectedSubKey = new byte[subKeyLength];
        Buffer.BlockCopy(hashedPassword, 13 + saltLength, expectedSubKey, 0, subKeyLength);

        var actualSubKey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subKeyLength);
        return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
    }

    public static string HashPasswordV3(string password)
    {
        var bytes = HashPasswordV3Internal(
            password,
            prf: KeyDerivationPrf.HMACSHA512,
            iterCount: 10000,
            saltSize: 16,
            numBytesRequested: 32);
        return Convert.ToBase64String(bytes);
    }

    private static byte[] HashPasswordV3Internal(string password, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
    {
        var salt = RandomNumberGenerator.GetBytes(saltSize);
        var subKey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

        var output = new byte[13 + saltSize + numBytesRequested];
        output[0] = 0x01;
        WriteNetworkByteOrder(output, 1, (uint)prf);
        WriteNetworkByteOrder(output, 5, (uint)iterCount);
        WriteNetworkByteOrder(output, 9, (uint)saltSize);
        Buffer.BlockCopy(salt, 0, output, 13, saltSize);
        Buffer.BlockCopy(subKey, 0, output, 13 + saltSize, numBytesRequested);
        return output;
    }

    private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        => ((uint)buffer[offset] << 24)
           | ((uint)buffer[offset + 1] << 16)
           | ((uint)buffer[offset + 2] << 8)
           | buffer[offset + 3];

    private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }
}
