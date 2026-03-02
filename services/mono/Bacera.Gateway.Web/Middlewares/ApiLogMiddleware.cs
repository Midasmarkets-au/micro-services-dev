using System.Web;
using System.Text;
using System.Net;
using Bacera.Gateway.MyException;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using UAParser;


namespace Bacera.Gateway.Web.Middleware;

public class ApiLogMiddleware(
    TenantDbContext tenantDbContext,
    Tenancy tenancy,
    ITenantService tenantService,
    IMyCache cache,
    CentralDbContext centralDbContext)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        
        var tenantId = tenancy.GetTenantId();
        var tenant = await tenantService.GetAsync(tenantId);
        if (!tenant.IsEmpty() && tenant.ApiLogEnable == false)
        {
            await next(context);
            return;
        }

        var partyIdHashed = context.User.Claims.FirstOrDefault(x => x.Type == "PartyId")?.Value;
        var partyId = partyIdHashed == null ? 0 : Party.HashDecode(partyIdHashed);

        var userAgent = context.Request.Headers.TryGetValue("User-Agent", out var userAgentString)
            ? userAgentString.FirstOrDefault()
            : string.Empty;

        var agentStr = userAgent;

        const int expireTimeInMinutes = 5;
        _ = Task.Run(async () =>
        {
            try
            {
                if (agentStr!.Contains("BCR Trader"))
                {
                    var deviceName = agentStr.Contains("ios", StringComparison.OrdinalIgnoreCase)
                        ? "app.iOS"
                        : "app.Android";

                    var key = $"portal_online:{tenantId}:{partyId}:app:{deviceName}";
                    await cache.SetStringAsync(key, agentStr, TimeSpan.FromMinutes(expireTimeInMinutes));
                }
                else if (agentStr.Contains("dart", StringComparison.OrdinalIgnoreCase))
                {
                    var key = $"portal_online:{tenantId}:{partyId}:app:dart";
                    await cache.SetStringAsync(key, agentStr, TimeSpan.FromMinutes(expireTimeInMinutes));
                }
                else
                {
                    var parser = await Parser.GetDefaultAsync();
                    var clientInfo = await parser.ParseAsync(agentStr);
                    var browserFamily = clientInfo.Browser.Family;
                    var deviceFamily = clientInfo.Device.Family;
                    var key = $"portal_online:{tenantId}:{partyId}:{browserFamily}:{deviceFamily}";
                    await cache.SetStringAsync(key, JsonConvert.SerializeObject(clientInfo),
                        TimeSpan.FromMinutes(expireTimeInMinutes));
                }
            }
            catch (Exception e)
            {
                BcrLog.Slack($"Error parsing user agent: {e.Message}, {partyId}, {agentStr}");
            }
        });

        var requestPath = context.Request.Path.Value ?? "/none";
        if (ExcludedPaths.Any(path => requestPath == path || requestPath.StartsWith(path)))
        {
            await next(context);
            return;
        }

        if (!requestPath.Contains("payment/callback") && context.Request.Method == "GET")
        {
            await next(context);
            return;
        }

        var apiLog = new ApiLog();
        var centralApiLog = new CentralApiLog();
        try
        {
            

            var referer = context.Request.Headers.TryGetValue("Referer", out var refererString)
                ? refererString.FirstOrDefault()
                : string.Empty;

          
            var godPartyIdHashed = context.User.Claims.FirstOrDefault(x => x.Type == UserClaimTypes.GodPartyId)?.Value;
            var godPartyId = godPartyIdHashed == null ? 0 : Party.HashDecode(godPartyIdHashed);

            if (godPartyId != 0)
            {
                userAgent += $" GodPartyId:{godPartyId}";
            }

            apiLog = new ApiLog
            {
                Method = context.Request.Method,
                Action = context.Request.Path,
                ConnectionId = context.Connection.Id,
                Parameters = GetQueryString(context),
                RequestContent = await GetRequestBody(context),
                Ip = GetRemoteIpAddress(context),
                PartyId = partyId,
                UserAgent = userAgent,
                Referer = referer,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            centralApiLog = apiLog.ToCentralApiLog();
            try
            {
                if (tenancy.IsTenantSet() == false)
                {
                    await centralDbContext.CentralApiLogs.AddAsync(centralApiLog);
                    await centralDbContext.SaveChangesAsync();
                }
                else
                {
                    await tenantDbContext.ApiLogs.AddAsync(apiLog);
                    await tenantDbContext.SaveChangesAsync();
                }
            }
            catch
            {
                // ignored
            }

            using var responseBody = new MemoryStream();
            var originalBodyStream = context.Response.Body;
            context.Response.Body = responseBody;

            await next(context);
            // await ExecuteNext(context, next);
            apiLog.StatusCode = context.Response.StatusCode;
            centralApiLog.StatusCode = context.Response.StatusCode;
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            apiLog.ResponseContent = await new StreamReader(responseBody).ReadToEndAsync();
            centralApiLog.ResponseContent = apiLog.ResponseContent;
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception e)
        {
            apiLog.StatusCode = 500;
            apiLog.ResponseContent = e.Message;
            centralApiLog.StatusCode = 500;
            centralApiLog.ResponseContent = e.Message;
            throw;
        }
        finally
        {
            try
            {
                apiLog.UpdatedOn = DateTime.UtcNow;
                centralApiLog.UpdatedOn = DateTime.UtcNow;
                if (tenancy.IsTenantSet() == false)
                {
                    centralDbContext.CentralApiLogs.Update(centralApiLog);
                    await centralDbContext.SaveChangesAsync();
                }
                else
                {
                    tenantDbContext.ApiLogs.Update(apiLog);
                    await tenantDbContext.SaveChangesAsync();
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    private static string GetQueryString(HttpContext context)
    {
        var queryString = context.Request.QueryString.Value;
        if (string.IsNullOrEmpty(queryString)) return "{}";
        try
        {
            var nameValueCollection = HttpUtility.ParseQueryString(queryString);
            if (nameValueCollection.Get("password") != null)
            {
                nameValueCollection.Set("password", "*****");
            }

            if (nameValueCollection.Get("confirmPassword") != null)
            {
                nameValueCollection.Set("confirmPassword", "*****");
            }

            var dict = nameValueCollection.AllKeys
                .Where(k => !string.IsNullOrEmpty(k))
                .ToDictionary(k => k!, k => nameValueCollection[k] ?? "");

            var json = JsonConvert.SerializeObject(dict);
            return json;
        }
        catch
        {
            return "{\"query\":\"" + queryString + "\"}";
        }
    }

    private static string GetRemoteIpAddress(HttpContext context, bool allowForwarded = true)
    {
        if (!allowForwarded) return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;


        // 尝试从 X-Forwarded-For 头部获取 IP 地址
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"];
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            var ips = forwardedHeader.ToString().Split(',');
            if (ips.Length > 0)
            {
                return ips[0]; // 假定第一个是客户端的真实 IP
            }
        }

        // 如果没有 X-Forwarded-For 头部，或者需要直接获取连接的远程 IP 地址
        return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    private static async Task<string> GetRequestBody(HttpContext context)
    {
        if (context.Request.Method == "GET") return "{}";

        context.Request.EnableBuffering(); //允许多次读取HTTP请求的正文
        using var reader = new StreamReader(context.Request.Body
            , encoding: Encoding.UTF8
            , detectEncodingFromByteOrderMarks: false
            , leaveOpen: true);
        string requestContent;
        try
        {
            requestContent = await reader.ReadToEndAsync();
        }
        catch (Exception e)
        {
            BcrLog.Slack($"Error reading request body: {e.Message}, {context.Request.Path.Value ?? "/none"}");
            return "{}";
        }
            
        context.Request.Body.Position = 0;

        // check if password is in the request json body
        try
        {
            var json = JsonConvert.DeserializeObject<dynamic>(requestContent, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            });
            if (json?.password != null)
            {
                json.password = "*****";
            }

            if (json?.confirmPassword != null)
            {
                json.confirmPassword = "*****";
            }

            if (json?.oldPassword != null)
            {
                json.oldPassword = "*****";
            }

            if (json?.newPassword != null)
            {
                json.newPassword = "*****";
            }

            if (json?.currentPassword != null)
            {
                json.currentPassword = "*****";
            }

            if (json?.request != null)
            {
                if (json.request.ccNumber != null)
                {
                    string number = json.request.ccNumber;
                    var last4 = number.Length > 4 ? number[^4..] : number;
                    json.request.ccNumber = $"*****{last4}";
                }

                if (json.request.ccCvc != null)
                {
                    json.request.ccCvc = "***";
                }
            }

            return JsonConvert.SerializeObject(json, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            });
        }
        catch
        {
            return "{}";
        }
    }

    private static readonly HashSet<string> ExcludedPaths =
    [
        "/api/status/ping",
        "/api/v1/tenant/media",
        "/none",
        "/hf_manage",
        "/hub/client/negotiate",
        "/hub/client",
        "/api/v1/auth/c"
    ];
}