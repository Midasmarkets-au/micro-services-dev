using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Bacera.Gateway.Core.Utility;

public class RSAHelper
{
    public enum SecretKeyModel
    {
        XML = 0,
        PEM = 1
    }

    private string PublicXmlKey { get; set; }
    private string? PrivateXmlKey { get; set; }
    private RSACryptoServiceProvider Rsa { get; set; }

    public RSAHelper(string publicKey, string? privateKey = null, int length = 1024,
        SecretKeyModel skModel = SecretKeyModel.XML)
    {
        Rsa = new RSACryptoServiceProvider(length);
        switch (skModel)
        {
            case SecretKeyModel.XML:
                PrivateXmlKey = privateKey;
                PublicXmlKey = publicKey;
                break;
            case SecretKeyModel.PEM:
            {
                PublicXmlKey = ConvertToXmlPublicKey(publicKey);
                if (privateKey != null)
                {
                    PrivateXmlKey = ConvertToXmlPrivateKey(privateKey);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(skModel), skModel, null);
        }
    }

    //公钥加密
    public string? PublicKeyEncrypt(string plaintext)
    {
        try
        {
            Rsa.FromXmlString(PublicXmlKey);
            var plaintextData = Encoding.UTF8.GetBytes(plaintext);
            var maxBlockSize = Rsa.KeySize / 8 - 11;
            if (plaintextData.Length <= maxBlockSize)
            {
                return Convert.ToBase64String(this.Rsa.Encrypt(plaintextData, false));
            }

            using var plaintStream = new MemoryStream(plaintextData);
            using var cryptStream = new MemoryStream();
            var buffer = new byte[maxBlockSize];
            var blockSize = plaintStream.Read(buffer, 0, maxBlockSize);
            while (blockSize > 0)
            {
                var toEncrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toEncrypt, 0, blockSize);

                var cryptography = Rsa.Encrypt(toEncrypt, false);
                cryptStream.Write(cryptography, 0, cryptography.Length);
                blockSize = plaintStream.Read(buffer, 0, maxBlockSize);
            }

            return Convert.ToBase64String(cryptStream.ToArray(), Base64FormattingOptions.None);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    //私钥加密
    public string? PrivateKeyEncrypt(string plaintext)
    {
        try
        {
            if (PrivateXmlKey != null) Rsa.FromXmlString(PrivateXmlKey);
            var keyPair = DotNetUtilities.GetKeyPair(this.Rsa);
            var c = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
            c.Init(true, keyPair.Private);

            var plaintextData = Encoding.UTF8.GetBytes(plaintext);

            var maxBlockSize = this.Rsa.KeySize / 8 - 11;
            if (plaintextData.Length <= maxBlockSize)
            {
                return Convert.ToBase64String(c.DoFinal(plaintextData));
            }

            using var plainStream = new MemoryStream(plaintextData);
            using var cryptStream = new MemoryStream();
            var buffer = new byte[maxBlockSize];
            var blockSize = plainStream.Read(buffer, 0, maxBlockSize);
            while (blockSize > 0)
            {
                var toEncrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toEncrypt, 0, blockSize);

                var cryptography = c.DoFinal(toEncrypt);
                cryptStream.Write(cryptography, 0, cryptography.Length);
                blockSize = plainStream.Read(buffer, 0, maxBlockSize);
            }

            return Convert.ToBase64String(cryptStream.ToArray(), Base64FormattingOptions.None);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    //公钥解密
    public string? PublicKeyDecrypt(string ciphertext)
    {
        try
        {
            Rsa.FromXmlString(PublicXmlKey);
            var rp = Rsa.ExportParameters(false);
            AsymmetricKeyParameter pbk = DotNetUtilities.GetRsaPublicKey(rp);
            var c = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
            c.Init(false, pbk);

            var ciphertextData = Convert.FromBase64String(ciphertext);
            var maxBlockSize = this.Rsa.KeySize / 8;

            if (ciphertextData.Length <= maxBlockSize)
            {
                return Encoding.UTF8.GetString(c.DoFinal(ciphertextData));
            }

            using var cryptStream = new MemoryStream(ciphertextData);
            using var plainStream = new MemoryStream();
            var buffer = new byte[maxBlockSize];
            var blockSize = cryptStream.Read(buffer, 0, maxBlockSize);

            while (blockSize > 0)
            {
                var toDecrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toDecrypt, 0, blockSize);

                var plaintext = c.DoFinal(toDecrypt);
                plainStream.Write(plaintext, 0, plaintext.Length);

                blockSize = cryptStream.Read(buffer, 0, maxBlockSize);
            }

            return Encoding.UTF8.GetString(plainStream.ToArray());
        }
        catch
        {
            // ignored
        }

        return null;
    }

    //私钥解密
    public string? PrivateKeyDecrypt(string ciphertext)
    {
        try
        {
            if (PrivateXmlKey != null) Rsa.FromXmlString(PrivateXmlKey);
            var ciphertextData = Convert.FromBase64String(ciphertext);
            var maxBlockSize = this.Rsa.KeySize / 8;

            if (ciphertextData.Length <= maxBlockSize)
            {
                return Encoding.UTF8.GetString(this.Rsa.Decrypt(ciphertextData, false));
            }

            using var cryptStream = new MemoryStream(ciphertextData);
            using var plainStream = new MemoryStream();
            var buffer = new byte[maxBlockSize];
            var blockSize = cryptStream.Read(buffer, 0, maxBlockSize);

            while (blockSize > 0)
            {
                var toDecrypt = new byte[blockSize];
                Array.Copy(buffer, 0, toDecrypt, 0, blockSize);

                var plaintext = this.Rsa.Decrypt(toDecrypt, false);
                plainStream.Write(plaintext, 0, plaintext.Length);

                blockSize = cryptStream.Read(buffer, 0, maxBlockSize);
            }

            return Encoding.UTF8.GetString(plainStream.ToArray());
        }
        catch
        {
            // ignored
        }

        return null;
    }

    //私钥pem 转 xml
    public static string ConvertToXmlPrivateKey(string prvPEMKey)
    {
        var privateKeyParam =
            (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(prvPEMKey));
        var xmlPrivateKey =
            $"<RSAKeyValue><Modulus>{Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned())}</Modulus><Exponent>{Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned())}</Exponent><P>{Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned())}</P><Q>{Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned())}</Q><DP>{Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned())}</DP><DQ>{Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned())}</DQ><InverseQ>{Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned())}</InverseQ><D>{Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned())}</D></RSAKeyValue>";
        return xmlPrivateKey;
    }

    //公钥pem 转 xml
    public static string ConvertToXmlPublicKey(string pubPEMKey)
    {
        var publicKeyParam =
            (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(pubPEMKey));
        var xmlPublicKey =
            $"<RSAKeyValue><Modulus>{Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned())}</Modulus><Exponent>{Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())}</Exponent></RSAKeyValue>";
        return xmlPublicKey;
    }
}

public static class RSAExtensions
{
    /// <summary>
    ///  把java的私钥转换成.net的xml格式
    /// </summary>
    /// <param name="rsa"></param>
    /// <param name="privateJavaKey"></param>
    /// <returns></returns>
    public static string ConvertToXmlPrivateKey(this RSA rsa, string privateJavaKey)
    {
        // Add defensive programming for better error handling
        if (string.IsNullOrWhiteSpace(privateJavaKey))
        {
            throw new ArgumentException("Private key cannot be null or empty. Check payment method configuration.", nameof(privateJavaKey));
        }

        try
        {
            RsaPrivateCrtKeyParameters privateKeyParam =
                (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateJavaKey));
            string xmlPrivateKey = string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
            return xmlPrivateKey;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"Invalid base64 private key format: {privateJavaKey.Substring(0, Math.Min(50, privateJavaKey.Length))}...", nameof(privateJavaKey), ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert Java private key to XML format. Key: {privateJavaKey.Substring(0, Math.Min(50, privateJavaKey.Length))}...", ex);
        }
    }

    /// <summary>
    /// RSA加载JAVA  PrivateKey
    /// </summary>
    /// <param name="privateJavaKey">java提供的第三方私钥</param>
    /// <returns></returns>
    public static void FromPrivateKeyJavaString(this RSA rsa, string privateJavaKey)
    {
        string xmlPrivateKey = rsa.ConvertToXmlPrivateKey(privateJavaKey);
        rsa.FromXmlString(xmlPrivateKey);
    }

    /// <summary>
    /// 把java的公钥转换成.net的xml格式
    /// </summary>
    /// <param name="privateKey">java提供的第三方公钥</param>
    /// <returns></returns>
    public static string ConvertToXmlPublicJavaKey(this RSA rsa, string publicJavaKey)
    {
        // Add defensive programming for better error handling
        if (string.IsNullOrWhiteSpace(publicJavaKey))
        {
            throw new ArgumentException("Public key cannot be null or empty. Check payment method configuration.", nameof(publicJavaKey));
        }

        try
        {
            RsaKeyParameters publicKeyParam =
                (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicJavaKey));
            string xmlpublicKey = string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
            return xmlpublicKey;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException($"Invalid base64 public key format: {publicJavaKey.Substring(0, Math.Min(50, publicJavaKey.Length))}...", nameof(publicJavaKey), ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert Java public key to XML format. Key: {publicJavaKey.Substring(0, Math.Min(50, publicJavaKey.Length))}...", ex);
        }
    }

    /// <summary>
    /// 把java的私钥转换成.net的xml格式
    /// </summary>
    /// <param name="privateKey">java提供的第三方公钥</param>
    /// <returns></returns>
    public static void FromPublicKeyJavaString(this RSA rsa, string publicJavaKey)
    {
        string xmlpublicKey = rsa.ConvertToXmlPublicJavaKey(publicJavaKey);
        rsa.FromXmlString(xmlpublicKey);
    }
    ///// <summary>
    ///// RSA公钥格式转换，java->.net
    ///// </summary>
    ///// <param name="publicKey">java生成的公钥</param>
    ///// <returns></returns>
    //private static string ConvertJavaPublicKeyToDotNet(this RSA rsa,string publicKey)
    //{           
    //    RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
    //    return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
    //        Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
    //        Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
    //}

    /// <summary>Extension method for initializing a RSACryptoServiceProvider from PEM data string.</summary>

    #region Methods

    /// <summary>Extension method which initializes an RSACryptoServiceProvider from a DER public key blob.</summary>
    public static void LoadPublicKeyDER(this RSACryptoServiceProvider provider, byte[] DERData)
    {
        byte[] RSAData = GetRSAFromDER(DERData);
        byte[] publicKeyBlob = GetPublicKeyBlobFromRSA(RSAData);
        provider.ImportCspBlob(publicKeyBlob);
    }

    /// <summary>Extension method which initializes an RSACryptoServiceProvider from a DER private key blob.</summary>
    public static void LoadPrivateKeyDER(this RSACryptoServiceProvider provider, byte[] DERData)
    {
        byte[] privateKeyBlob = GetPrivateKeyDER(DERData);
        provider.ImportCspBlob(privateKeyBlob);
    }

    /// <summary>Extension method which initializes an RSACryptoServiceProvider from a PEM public key string.</summary>
    public static void LoadPublicKeyPEM(this RSACryptoServiceProvider provider, string sPEM)
    {
        byte[] DERData = GetDERFromPEM(sPEM);
        LoadPublicKeyDER(provider, DERData);
    }

    /// <summary>Extension method which initializes an RSACryptoServiceProvider from a PEM private key string.</summary>
    public static void LoadPrivateKeyPEM(this RSACryptoServiceProvider provider, string sPEM)
    {
        byte[] DERData = GetDERFromPEM(sPEM);
        LoadPrivateKeyDER(provider, DERData);
    }

    /// <summary>Returns a public key blob from an RSA public key.</summary>
    internal static byte[] GetPublicKeyBlobFromRSA(byte[] RSAData)
    {
        byte[] data = null;
        UInt32 dwCertPublicKeyBlobSize = 0;
        if (CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING,
                new IntPtr((int)CRYPT_OUTPUT_TYPES.RSA_CSP_PUBLICKEYBLOB), RSAData, (UInt32)RSAData.Length,
                CRYPT_DECODE_FLAGS.NONE,
                data, ref dwCertPublicKeyBlobSize))
        {
            data = new byte[dwCertPublicKeyBlobSize];
            if (!CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING,
                    new IntPtr((int)CRYPT_OUTPUT_TYPES.RSA_CSP_PUBLICKEYBLOB), RSAData, (UInt32)RSAData.Length,
                    CRYPT_DECODE_FLAGS.NONE,
                    data, ref dwCertPublicKeyBlobSize))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        else
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return data;
    }

    /// <summary>Converts DER binary format to a CAPI CRYPT_PRIVATE_KEY_INFO structure.</summary>
    internal static byte[] GetPrivateKeyDER(byte[] DERData)
    {
        byte[] data = null;
        UInt32 dwRSAPrivateKeyBlobSize = 0;
        IntPtr pRSAPrivateKeyBlob = IntPtr.Zero;
        if (CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING,
                new IntPtr((int)CRYPT_OUTPUT_TYPES.PKCS_RSA_PRIVATE_KEY),
                DERData, (UInt32)DERData.Length, CRYPT_DECODE_FLAGS.NONE, data, ref dwRSAPrivateKeyBlobSize))
        {
            data = new byte[dwRSAPrivateKeyBlobSize];
            if (!CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING,
                    new IntPtr((int)CRYPT_OUTPUT_TYPES.PKCS_RSA_PRIVATE_KEY),
                    DERData, (UInt32)DERData.Length, CRYPT_DECODE_FLAGS.NONE, data, ref dwRSAPrivateKeyBlobSize))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        else
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return data;
    }

    /// <summary>Converts DER binary format to a CAPI CERT_PUBLIC_KEY_INFO structure containing an RSA key.</summary>
    internal static byte[] GetRSAFromDER(byte[] DERData)
    {
        byte[] data = null;
        byte[] publicKey = null;
        CERT_PUBLIC_KEY_INFO info;
        UInt32 dwCertPublicKeyInfoSize = 0;
        IntPtr pCertPublicKeyInfo = IntPtr.Zero;
        if (CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING,
                new IntPtr((int)CRYPT_OUTPUT_TYPES.X509_PUBLIC_KEY_INFO),
                DERData, (UInt32)DERData.Length, CRYPT_DECODE_FLAGS.NONE, data, ref dwCertPublicKeyInfoSize))
        {
            data = new byte[dwCertPublicKeyInfoSize];
            if (CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING,
                    new IntPtr((int)CRYPT_OUTPUT_TYPES.X509_PUBLIC_KEY_INFO),
                    DERData, (UInt32)DERData.Length, CRYPT_DECODE_FLAGS.NONE, data, ref dwCertPublicKeyInfoSize))
            {
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    info = (CERT_PUBLIC_KEY_INFO)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                        typeof(CERT_PUBLIC_KEY_INFO));
                    publicKey = new byte[info.PublicKey.cbData];
                    Marshal.Copy(info.PublicKey.pbData, publicKey, 0, publicKey.Length);
                }
                finally
                {
                    handle.Free();
                }
            }
            else
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        else
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return publicKey;
    }

    /// <summary>Extracts the binary data from a PEM file.</summary>
    internal static byte[] GetDERFromPEM(string sPEM)
    {
        UInt32 dwSkip, dwFlags;
        UInt32 dwBinarySize = 0;

        if (!CryptStringToBinary(sPEM, (UInt32)sPEM.Length, CRYPT_STRING_FLAGS.CRYPT_STRING_BASE64HEADER, null,
                ref dwBinarySize, out dwSkip, out dwFlags))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        byte[] decodedData = new byte[dwBinarySize];
        if (!CryptStringToBinary(sPEM, (UInt32)sPEM.Length, CRYPT_STRING_FLAGS.CRYPT_STRING_BASE64HEADER, decodedData,
                ref dwBinarySize, out dwSkip, out dwFlags))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return decodedData;
    }

    #endregion Methods

    #region P/Invoke Constants

    /// <summary>Enumeration derived from Crypto API.</summary>
    internal enum CRYPT_ACQUIRE_CONTEXT_FLAGS : uint
    {
        CRYPT_NEWKEYSET = 0x8,
        CRYPT_DELETEKEYSET = 0x10,
        CRYPT_MACHINE_KEYSET = 0x20,
        CRYPT_SILENT = 0x40,
        CRYPT_DEFAULT_CONTAINER_OPTIONAL = 0x80,
        CRYPT_VERIFYCONTEXT = 0xF0000000
    }

    /// <summary>Enumeration derived from Crypto API.</summary>
    internal enum CRYPT_PROVIDER_TYPE : uint
    {
        PROV_RSA_FULL = 1
    }

    /// <summary>Enumeration derived from Crypto API.</summary>
    internal enum CRYPT_DECODE_FLAGS : uint
    {
        NONE = 0,
        CRYPT_DECODE_ALLOC_FLAG = 0x8000
    }

    /// <summary>Enumeration derived from Crypto API.</summary>
    internal enum CRYPT_ENCODING_FLAGS : uint
    {
        PKCS_7_ASN_ENCODING = 0x00010000,
        X509_ASN_ENCODING = 0x00000001,
    }

    /// <summary>Enumeration derived from Crypto API.</summary>
    internal enum CRYPT_OUTPUT_TYPES : int
    {
        X509_PUBLIC_KEY_INFO = 8,
        RSA_CSP_PUBLICKEYBLOB = 19,
        PKCS_RSA_PRIVATE_KEY = 43,
        PKCS_PRIVATE_KEY_INFO = 44
    }

    /// <summary>Enumeration derived from Crypto API.</summary>
    internal enum CRYPT_STRING_FLAGS : uint
    {
        CRYPT_STRING_BASE64HEADER = 0,
        CRYPT_STRING_BASE64 = 1,
        CRYPT_STRING_BINARY = 2,
        CRYPT_STRING_BASE64REQUESTHEADER = 3,
        CRYPT_STRING_HEX = 4,
        CRYPT_STRING_HEXASCII = 5,
        CRYPT_STRING_BASE64_ANY = 6,
        CRYPT_STRING_ANY = 7,
        CRYPT_STRING_HEX_ANY = 8,
        CRYPT_STRING_BASE64X509CRLHEADER = 9,
        CRYPT_STRING_HEXADDR = 10,
        CRYPT_STRING_HEXASCIIADDR = 11,
        CRYPT_STRING_HEXRAW = 12,
        CRYPT_STRING_NOCRLF = 0x40000000,
        CRYPT_STRING_NOCR = 0x80000000
    }

    #endregion P/Invoke Constants

    #region P/Invoke Structures

    /// <summary>Structure from Crypto API.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CRYPT_OBJID_BLOB
    {
        internal UInt32 cbData;
        internal IntPtr pbData;
    }

    /// <summary>Structure from Crypto API.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CRYPT_ALGORITHM_IDENTIFIER
    {
        internal IntPtr pszObjId;
        internal CRYPT_OBJID_BLOB Parameters;
    }

    /// <summary>Structure from Crypto API.</summary>
    [StructLayout(LayoutKind.Sequential)]
    struct CRYPT_BIT_BLOB
    {
        internal UInt32 cbData;
        internal IntPtr pbData;
        internal UInt32 cUnusedBits;
    }

    /// <summary>Structure from Crypto API.</summary>
    [StructLayout(LayoutKind.Sequential)]
    struct CERT_PUBLIC_KEY_INFO
    {
        internal CRYPT_ALGORITHM_IDENTIFIER Algorithm;
        internal CRYPT_BIT_BLOB PublicKey;
    }

    #endregion P/Invoke Structures

    #region P/Invoke Functions

    /// <summary>Function for Crypto API.</summary>
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CryptDestroyKey(IntPtr hKey);

    /// <summary>Function for Crypto API.</summary>
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CryptImportKey(IntPtr hProv, byte[] pbKeyData, UInt32 dwDataLen, IntPtr hPubKey,
        UInt32 dwFlags, ref IntPtr hKey);

    /// <summary>Function for Crypto API.</summary>
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CryptReleaseContext(IntPtr hProv, Int32 dwFlags);

    /// <summary>Function for Crypto API.</summary>
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CryptAcquireContext(ref IntPtr hProv, string pszContainer, string pszProvider,
        CRYPT_PROVIDER_TYPE dwProvType, CRYPT_ACQUIRE_CONTEXT_FLAGS dwFlags);

    /// <summary>Function from Crypto API.</summary>
    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CryptStringToBinary(string sPEM, UInt32 sPEMLength, CRYPT_STRING_FLAGS dwFlags,
        [Out] byte[] pbBinary, ref UInt32 pcbBinary, out UInt32 pdwSkip, out UInt32 pdwFlags);

    /// <summary>Function from Crypto API.</summary>
    [DllImport("crypt32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CryptDecodeObjectEx(CRYPT_ENCODING_FLAGS dwCertEncodingType, IntPtr lpszStructType,
        byte[] pbEncoded, UInt32 cbEncoded, CRYPT_DECODE_FLAGS dwFlags, IntPtr pDecodePara, ref byte[] pvStructInfo,
        ref UInt32 pcbStructInfo);

    /// <summary>Function from Crypto API.</summary>
    [DllImport("crypt32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CryptDecodeObject(CRYPT_ENCODING_FLAGS dwCertEncodingType, IntPtr lpszStructType,
        byte[] pbEncoded, UInt32 cbEncoded, CRYPT_DECODE_FLAGS flags, [In, Out] byte[] pvStructInfo,
        ref UInt32 cbStructInfo);

    #endregion P/Invoke Functions
}
