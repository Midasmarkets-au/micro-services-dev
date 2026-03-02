using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace Bacera.Gateway.Services;

/// <summary>
/// 交易密码加密服务
/// 使用 ASP.NET Core Data Protection API 进行AES-256-GCM加密
/// 优势：自动密钥轮换、集成 Azure Key Vault / AWS KMS、防止密文篡改
/// </summary>
public interface ITradePasswordEncryptionService
{
    /// <summary>
    /// 加密明文密码
    /// </summary>
    string Encrypt(string plainPassword);
    
    /// <summary>
    /// 解密密文密码
    /// </summary>
    string Decrypt(string encryptedPassword);
    
    /// <summary>
    /// 计算密码的SHA256哈希值（用于比较，不可逆）
    /// </summary>
    string ComputeHash(string password);
}

public class TradePasswordEncryptionService : ITradePasswordEncryptionService
{
    private readonly IDataProtector _protector;
    
    public TradePasswordEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        // 使用特定purpose创建protector，确保密钥隔离
        _protector = dataProtectionProvider.CreateProtector("Bacera.Gateway.TradePassword.v1");
    }
    
    /// <summary>
    /// 加密明文密码
    /// </summary>
    /// <param name="plainPassword">明文密码</param>
    /// <returns>加密后的密文（Base64编码）</returns>
    public string Encrypt(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
            throw new ArgumentNullException(nameof(plainPassword));
            
        return _protector.Protect(plainPassword);
    }
    
    /// <summary>
    /// 解密密文密码
    /// </summary>
    /// <param name="encryptedPassword">加密的密文</param>
    /// <returns>明文密码</returns>
    public string Decrypt(string encryptedPassword)
    {
        if (string.IsNullOrEmpty(encryptedPassword))
            throw new ArgumentNullException(nameof(encryptedPassword));
            
        return _protector.Unprotect(encryptedPassword);
    }
    
    /// <summary>
    /// 计算密码的SHA256哈希值
    /// 用于判断密码是否被修改过，不可逆
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>SHA256哈希值（Base64编码）</returns>
    public string ComputeHash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password));
            
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashBytes);
    }
}

