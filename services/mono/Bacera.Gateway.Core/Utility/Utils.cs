using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Bacera.Gateway;

public static partial class Utils
{
    public static bool IsImage(string? contentType)
        => !string.IsNullOrEmpty(contentType) &&
           contentType.StartsWith("image/", StringComparison.CurrentCultureIgnoreCase);
    public static string ToUnicode(string text)
    {
        return text.Aggregate("", (current, c) =>
        {
            if (c == ' ' || char.IsLetterOrDigit(c) && c <= 127)
            {
                return current + c; // 如果是英文字符或数字，直接保留原字符
            }

            return current + "\\u" + ((int)c).ToString("x4"); // 否则进行Unicode编码
        });
    }

    public static string HideEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        var atIndex = email.IndexOf('@');
        if (atIndex == -1)
            return email; // No '@' found, return the original email

        var firstCharacter = email[0];
        var lastCharacterBeforeAt = email[atIndex - 1];

        return $"{firstCharacter}****{lastCharacterBeforeAt}{email[atIndex..]}";
    }

    public static string HidePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return string.Empty;

        var lastFourDigits = phoneNumber[^4..];
        var maskedPart = new string('*', phoneNumber.Length - 4);

        return $"{maskedPart}{lastFourDigits}";
    }

    public static Regex PasswordHideRegex => MyRegex();

    /// <summary>
    /// Turn on and public to front-end as well.
    /// All timezone rely on it
    /// </summary>
    public static bool IsCurrentDSTLosAngeles(DateTime date)
        => TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles").IsDaylightSavingTime(date);
    // => false;

    public static bool IsWithinCloseMarketTime()
    {
        var now = DateTime.UtcNow;
        var start = now.Date
            .AddHours(IsCurrentDSTLosAngeles(now) ? 20 : 21)
            .AddMinutes(59)
            .AddSeconds(59);
        var end = start.AddMinutes(7);
        return now >= start && now <= end;
    }

    public static DateTime GetTodayCloseTradeTime()
    {
        var date = DateTime.UtcNow.Date;
        var result = date.AddHours(IsCurrentDSTLosAngeles(date) ? 20 : 21)
            .AddMinutes(59)
            .AddSeconds(59);
        return result;
    }

    public static JsonSerializerSettings UtcJsonSerializerSettings
        => new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

    public static DateTime ParseToUTC(string dateString)
    {
        // var dt = DateTime.ParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fffffff",
        //     CultureInfo.InvariantCulture);
        // dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        var dt = DateTime.Parse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public static async Task<long> GenerateUniqueIdAsync(Func<long, Task<bool>> uidExistValidator)
    {
        long uid;
        bool hasUid;
        var zeroTime = new DateTime(1, 1, 1);

        var start = new DateTime(2021, 1, 1);
        var now = DateTime.UtcNow;
        var years = (zeroTime + (now - start)).Year - 1;
        var days = 10;

        do
        {
            if (days > 9)
            {
                days = 1;
            }

            var uidString = $"7{years}{new Random().Next(1, 999):D3}{new Random().Next(1, 999):D3}{days}";
            uid = long.Parse(uidString);
            hasUid = await uidExistValidator(uid);
            days++;
        } while (hasUid);

        return uid;
    }

    public static long GenerateUniqueId(Func<long, bool>? uidExistValidator = null)
    {
        long uid;
        var hasUid = uidExistValidator != null;
        var zeroTime = new DateTime(1, 1, 1);

        var start = new DateTime(2021, 1, 1);
        var now = DateTime.UtcNow;
        var years = (zeroTime + (now - start)).Year - 1;
        var days = 10;

        do
        {
            if (days > 9)
            {
                days = 1;
            }

            var uidString = $"{years}{new Random().Next(1, 999):D3}{new Random().Next(1, 999):D3}{days}";
            uid = long.Parse(uidString);
            if (uidExistValidator != null)
            {
                hasUid = uidExistValidator(uid);
            }

            days++;
        } while (hasUid);

        return uid;
    }

    public static long ToAmountInCents(this decimal amount)
        => amount == 0 ? 0 : (long)Math.Round(amount * 100, 0);

    public static long ToAmountInCents(this double amount)
        => amount == 0 ? 0 : (long)Math.Round(amount * 100, 0);

    public static bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false;
        }

        try
        {
            var e = new System.Net.Mail.MailAddress(email);
            return e.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }

    public static string Md5Hash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
        //        return Convert.ToBase64String(hashBytes);
        var sb = new StringBuilder();
        foreach (var t in hashBytes)
        {
            sb.Append(t.ToString("x2"));
        }

        return sb.ToString();
    }

    public static string Sha512Hash(string rawData)
    {
        var bytes = SHA512.HashData(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }

        return builder.ToString();
    }

    public static string Sha256Hash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2")); // Convert to hexadecimal
        }

        return builder.ToString();
    }

    // HMAC-SHA256
    public static string HmacSha256(string message, string secret)
    {
        var keyByte = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        using var hmacSha256 = new HMACSHA256(keyByte);
        var output = hmacSha256.ComputeHash(messageBytes);
        return BitConverter.ToString(output).Replace("-", "");
    }

    public static string HmacSha512(string message, string secret)
    {
        // Convert the data and key to byte arrays
        byte[] dataBytes = Encoding.UTF8.GetBytes(message);
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);

        // Compute the HMAC-SHA512
        using (var hmacsha512 = new HMACSHA512(keyBytes))
        {
            byte[] hashBytes = hmacsha512.ComputeHash(dataBytes);

            // Optional: Convert to Base64 string if needed
            string base64Hash = Convert.ToBase64String(hashBytes);
            return base64Hash;

            // Output the result
        }
    }

    // Base64 encode
    public static string Base64Encode(string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

    public static string AesEncrypt(string plainText, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, iv))
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                string encryptedMessage = Convert.ToBase64String(encryptedBytes);
                string encryptedIV = Convert.ToBase64String(iv);

                string encryptedPath =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedMessage + "::" + encryptedIV));

                return encryptedPath;
            }
        }
    }

    static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException(nameof(plainText));
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException(nameof(Key));
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException(nameof(IV));
        byte[] encrypted;

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        return encrypted;
    }

    public static string MD5(string encryptString)
    {
        var result = Encoding.UTF8.GetBytes(encryptString);
        var output = System.Security.Cryptography.MD5.HashData(result);
        var encryptResult = BitConverter.ToString(output).Replace("-", "");
        return encryptResult.ToLower();
    }

    public static string SHA1(string encryptString)
    {
        var result = Encoding.UTF8.GetBytes(encryptString);
        var output = System.Security.Cryptography.SHA1.HashData(result);
        var encryptResult = BitConverter.ToString(output).Replace("-", "");
        return encryptResult.ToLower();
    }

    public static string TripleDESEncrypt(string encryptStr, string encryptKey)
    {
        var encoding = Encoding.UTF8;
        using SymmetricAlgorithm dESCSP = TripleDES.Create();
        var keyByte = encoding.GetBytes(encryptKey);
        dESCSP.Key = keyByte;
        //使用密钥的前8位作为对称算法的初始化向量
        var ivByte = encoding.GetBytes(encryptKey[..8]);
        dESCSP.IV = ivByte;
        using var ct = dESCSP.CreateEncryptor();
        var byt = encoding.GetBytes(encryptStr);
        using var ms = new MemoryStream();
        var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
        cs.Write(byt, 0, byt.Length);
        cs.FlushFinalBlock();
        cs.Close();
        return Bytes2HexString(ms.ToArray());
    }

    public static string TripleDESDecrypt(string decryptStr, string decryptKey)
    {
        var encoding = Encoding.UTF8;
        try
        {
            using SymmetricAlgorithm dESCSP = TripleDES.Create();
            var keyByte = encoding.GetBytes(decryptKey);
            dESCSP.Key = keyByte;
            var ivByte = encoding.GetBytes(decryptKey[..8]);
            dESCSP.IV = ivByte;
            using var ct = dESCSP.CreateDecryptor();
            var byt = HexStringToByteArray(decryptStr);
            using var ms = new MemoryStream();
            var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();
            cs.Close();
            return encoding.GetString(ms.ToArray());
        }
        catch
        {
            return decryptStr;
        }
    }

    private static string Bytes2HexString(IEnumerable<byte> b)
    {
        var ret = "";
        foreach (var t in b)
        {
            var hex = t.ToString("x");
            if (hex.Length == 1)
            {
                hex = '0' + hex;
            }

            ret += hex;
        }

        return ret;
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
    }

    /// <summary>
    /// Hashes the input string using the private key.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="privateKeyString"></param>
    /// <returns>Base64 String</returns>
    public static string SignSha265HashWithPkcs8PrivateKey(string input, string privateKeyString)
    {
        var privateKeyBytes = Convert.FromBase64String(privateKeyString);
        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
        var hashValue = rsa.SignHash(SHA256.HashData(Encoding.UTF8.GetBytes(input)), HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(hashValue);
    }

    public static string GetLastChars(string input, int length = 4)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return input.Length < length ? input : input[^length..];
    }

    /// <summary>
    /// Verify the input string using the public & private key.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="signature"></param>
    /// <param name="publicKey"></param>
    /// <returns>bool</returns>
    public static bool VerifySignatureForSha265Hash(string message, string signature, string publicKey)
    {
        publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("\n", "")
            .Trim();

        var publicKeyBytes = Convert.FromBase64String(publicKey);

        byte[] modulus;
        byte[] exponent;
        ExtractPublicKeyParameters(publicKeyBytes, out modulus, out exponent);
        var rsaParam = new RSAParameters()
        {
            Modulus = modulus,
            Exponent = exponent
        };

        var rsa = new RSACryptoServiceProvider();
        rsa.ImportParameters(rsaParam);

        // return rsa.VerifyData(Encoding.UTF8.GetBytes(message), Convert.FromBase64String(publicKey), HashAlgorithmName.SHA256,
        //     RSASignaturePadding.Pkcs1);

        // return rsa.VerifyData(Encoding.UTF8.GetBytes(message), Convert.FromBase64String(signature),
        //     HashAlgorithmName.SHA256,
        //     RSASignaturePadding.Pkcs1);
        // var verifier = new RSAPKCS1SignatureDeformatter(rsa);
        // verifier.SetHashAlgorithm("SHA256");
        // var bytesToVerify = Encoding.UTF8.GetBytes(message);
        // var hashToVerify = rsa.SignData(bytesToVerify, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        // result = verifier.VerifySignature(hashToVerify, Convert.FromBase64String(signature));
        // using var hmac = new HMACSHA256(publicKeyBytes);
        // var hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        //
        var result = false;
        var verifier = new RSAPKCS1SignatureDeformatter(rsa);
        verifier.SetHashAlgorithm("SHA256");
        var bytesToVerify = Encoding.UTF8.GetBytes(message);
        var hashToVerify = SHA256.HashData(bytesToVerify);
        result = verifier.VerifySignature(hashToVerify, Convert.FromBase64String(signature));
        return result;
    }

    static readonly byte[] SeqOid =
        { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

    public static void ExtractPublicKeyParameters(byte[] publicKey, out byte[] modulus, out byte[] exponent)
    {
        modulus = new byte[0];
        exponent = new byte[0];

        var seq = new byte[15];

        // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
        var mem = new MemoryStream(publicKey);
        var binr = new BinaryReader(mem); //wrap Memory Stream with BinaryReader for easy reading
        byte bt = 0;
        ushort twobytes = 0;

        try
        {
            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte(); //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16(); //advance 2 bytes
            else
                return;

            seq = binr.ReadBytes(15); //read the Sequence OID
            if (!CompareBytearrays(seq, SeqOid)) //make sure Sequence for OID is correct
                return;

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                binr.ReadByte(); //advance 1 byte
            else if (twobytes == 0x8203)
                binr.ReadInt16(); //advance 2 bytes
            else
                return;

            bt = binr.ReadByte();
            if (bt != 0x00) //expect null byte next
                return;

            twobytes = binr.ReadUInt16();
            if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                binr.ReadByte(); //advance 1 byte
            else if (twobytes == 0x8230)
                binr.ReadInt16(); //advance 2 bytes
            else
                return;

            twobytes = binr.ReadUInt16();
            byte lowbyte = 0x00;
            byte highbyte = 0x00;

            if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                lowbyte = binr.ReadByte(); // read next bytes which is bytes in modulus
            else if (twobytes == 0x8202)
            {
                highbyte = binr.ReadByte(); //advance 2 bytes
                lowbyte = binr.ReadByte();
            }
            else
                return;

            byte[] modint =
                { lowbyte, highbyte, 0x00, 0x00 }; //reverse byte order since asn.1 key uses big endian order
            var modsize = BitConverter.ToInt32(modint, 0);

            var firstbyte = binr.PeekChar();
            if (firstbyte == 0x00)
            {
                //if first byte (highest order) of modulus is zero, don't include it
                binr.ReadByte(); //skip this null byte
                modsize -= 1; //reduce modulus buffer size by 1
            }

            modulus = binr.ReadBytes(modsize); //read the modulus bytes

            if (binr.ReadByte() != 0x02) //expect an Integer for the exponent data
                return;
            var expbytes =
                (int)binr.ReadByte(); // should only need one byte for actual exponent data (for all useful values)
            exponent = binr.ReadBytes(expbytes);
        }

        finally
        {
            binr.Close();
        }
    }

    private static bool CompareBytearrays(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        var i = 0;
        foreach (var c in a)
        {
            if (c != b[i])
                return false;
            i++;
        }

        return true;
    }

    public static RSA ParsePublicKey(string publicKey)
    {
        publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("\n", "")
            .Trim();

        var publicKeyBytes = Convert.FromBase64String(publicKey);
        var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
        return rsa;
    }

    public static string GenerateSimplePassword(int length = 8)
    {
        const string valid = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ";
        const string chars = "!@#";
        const string numbers = "23456789";
        var res = new StringBuilder();
        var rnd = new Random();
        length -= 2;
        res.Append(numbers[rnd.Next(numbers.Length)]);
        while (0 < length--)
        {
            var ch = valid[rnd.Next(valid.Length)];
            res.Append(length % 2 == 0 ? char.ToLower(ch) : char.ToUpper(ch));
        }

        // res.Append(chars[rnd.Next(chars.Length)]);
        // put chars[rnd.Next(chars.Length)] in the middle
        var charIndex = rnd.Next(res.Length);
        res.Insert(charIndex, chars[rnd.Next(chars.Length)]);

        return res.ToString();
    }

    public static string WordWrap(string input, int length, string breakSymbol = "\n")
    {
        if (length <= 0)
            return input;

        var result = new StringBuilder();
        var words = input.ToCharArray();

        var currentLineLength = 0;

        foreach (var word in words)
        {
            result.Append(word);
            currentLineLength++;
            if (currentLineLength % length == 0 && currentLineLength != 0)
                result.Append(breakSymbol);
        }

        return result.ToString();
    }

    public static JsonSerializerSettings AppJsonSerializerSettings
        => new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    public static JsonSerializerSettings AppJsonSerializerSettingsIgnoreNull
        => new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

    public static string JsonSerializeObject(object obj, bool ignoreNullValue = false)
        => JsonConvert.SerializeObject(obj,
            ignoreNullValue
                ? AppJsonSerializerSettingsIgnoreNull
                : AppJsonSerializerSettings);

    public static void TrimStrings(object obj)
    {
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.PropertyType != typeof(string) || !property.CanRead || !property.CanWrite) continue;

            if (property.GetValue(obj) is string currentValue)
            {
                property.SetValue(obj, currentValue.Trim());
            }
        }
    }

    public static T JsonDeserializeObjectWithDefault<T>(string json) where T : new()
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json) ?? new T();
        }
        catch
        {
            return new T();
        }
    }

    public static string ToCamelCase(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;

        var words = str.Split(new char[] { ' ', '-', '_', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return string.Empty;

        var firstWord = words[0].ToLowerInvariant();
        var camelCaseWords = words.Skip(1).Select(w => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(w.ToLower()));

        return firstWord + string.Concat(camelCaseWords);
    }

    public static string PascalToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0]))
        {
            return input;
        }

        // Convert the first character to lowercase
        var camelCase = char.ToLower(input[0]) + input.Substring(1);
        return camelCase;
    }

    private static DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// To Unix time in ticks
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static long ToUnixTime(DateTime utcDateTime)
    {
        return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc).Ticks - UnixEpoch.Ticks;
    }

    /// <summary>
    /// TO 10-digit timestamp
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public static long ToTimestamp(DateTime utcDateTime)
    {
        return (long)(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc) - UnixEpoch).TotalSeconds;
    }

    public static DateTime RoundUpToHour(DateTime? utcDateTime)
    {
        if (utcDateTime == null)
            return DateTime.MinValue;

        var dt = utcDateTime.Value;
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc);
    }

    public static DateTime AdjustedStartTimeToday(double timezoneOffset = 0)
        => DateTime.UtcNow.AddHours(timezoneOffset).Date.AddHours(-timezoneOffset);

    public static dynamic JsonDeserializeDynamic(string json)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new CamelCasePropertyNamesConverter() }
            };

            var value = JsonConvert.DeserializeObject<dynamic>(json, settings) ?? new { };

            return value;
        }
        catch
        {
            return new { };
        }
    }


    public static (DateTime StartTime, DateTime EndTime) CalculateTradePeriod(string period = "", DateTime? from = null,
        DateTime? to = null)
    {
        const string timeZoneId = "Pacific Standard Time"; // 洛杉矶时区的ID
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var now = DateTime.UtcNow;

        // 根据日期参数选择相应的日期范围
        DateTime rangeStart;
        DateTime rangeEnd;

        switch (period.ToLower())
        {
            case "year":
                rangeStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeEnd = new DateTime(now.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                break;
            case "lastyear":
                rangeStart = new DateTime(now.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeEnd = new DateTime(now.Year - 1, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                break;
            case "month":
                rangeStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                rangeEnd = rangeStart.AddMonths(1).AddSeconds(-1);
                break;
            case "lastmonth":
                rangeStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
                rangeEnd = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(-1);
                break;
            case "today":
                rangeStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                rangeEnd = rangeStart.AddDays(1).AddSeconds(-1);
                break;
            case "yesterday":
                rangeStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                rangeEnd = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59, DateTimeKind.Utc);
                break;
            case "week":
                int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                rangeStart = now.AddDays(-1 * diff).Date;
                rangeEnd = rangeStart.AddDays(7).AddSeconds(-1);
                break;
            case "lastweek":
                diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                rangeStart = now.AddDays(-1 * diff).Date.AddDays(-7);
                rangeEnd = rangeStart.AddDays(7).AddSeconds(-1);
                break;
            case "custom":
                if (from == null || to == null)
                    throw new ArgumentException("From and To date must be provided for custom date range.");

                rangeStart = new DateTime(from.Value.Year, from.Value.Month, from.Value.Day, 0, 0, 0, DateTimeKind.Utc);
                rangeEnd = new DateTime(to.Value.Year, to.Value.Month, to.Value.Day, 23, 59, 59, DateTimeKind.Utc);
                break;
            default:
                rangeStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                rangeEnd = rangeStart.AddDays(1).AddSeconds(-1);
                break;
        }

        // 转换为洛杉矶时区时间并计算是否为夏令时，以调整小时数
        var startTimeDst = timeZoneInfo.IsDaylightSavingTime(rangeStart);
        var endTimeDst = timeZoneInfo.IsDaylightSavingTime(rangeEnd);
        var adjustmentStartHours = startTimeDst ? 4 : 3;
        var adjustmentEndHours = endTimeDst ? 4 : 3;

        // 调整开始时间和结束时间
        var startTime = rangeStart.AddHours(-adjustmentStartHours);
        var endTime = rangeEnd.AddHours(-adjustmentEndHours);

        return (startTime, endTime);
    }

    [GeneratedRegex(@"password=([^;]+)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MyRegex();
}

public class CamelCasePropertyNamesConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(object);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var token = JToken.Load(reader);
        return ProcessToken(token);
    }

    private static JToken ProcessToken(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
            {
                var obj = new JObject();
                foreach (var property in (JObject)token)
                {
                    var camelCaseName = char.ToLower(property.Key[0]) + property.Key.Substring(1);
                    if (property.Value != null) obj[camelCaseName] = ProcessToken(property.Value);
                }

                return obj;
            }
            case JTokenType.Array:
            {
                var array = new JArray();
                foreach (var item in (JArray)token)
                {
                    array.Add(ProcessToken(item));
                }

                return array;
            }
            case JTokenType.None:
            case JTokenType.Constructor:
            case JTokenType.Property:
            case JTokenType.Comment:
            case JTokenType.Integer:
            case JTokenType.Float:
            case JTokenType.String:
            case JTokenType.Boolean:
            case JTokenType.Null:
            case JTokenType.Undefined:
            case JTokenType.Date:
            case JTokenType.Raw:
            case JTokenType.Bytes:
            case JTokenType.Guid:
            case JTokenType.Uri:
            case JTokenType.TimeSpan:
            default:
                return token;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
    }
}