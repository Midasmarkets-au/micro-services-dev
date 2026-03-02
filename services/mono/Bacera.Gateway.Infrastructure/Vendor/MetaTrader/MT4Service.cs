using System.Collections;
using System.Net.Sockets;
using System.Text;
using Bacera.Gateway.Vendor.MetaTrader.Models;
using Bacera.Gateway.Web.Services;
using LinqKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bacera.Gateway.Vendor.MetaTrader;

public class MT4Service : IMetaTraderService
{
    private readonly int _port;
    private readonly string _host;
    private const int SendTimeout = 5000;
    private const int ReceiveTimeout = 3000;
    private readonly ILogger<MT4Service> _logger;

    public MT4Service(IOptions<ApiOptions> options, ILoggerFactory? loggerFactory = null)
    {
        _host = options.Value.Host;
        _port = options.Value.Port;
        _logger = loggerFactory == null ? NullLogger<MT4Service>.Instance : loggerFactory.CreateLogger<MT4Service>();
    }

    public MT4Service(string host, int port, ILoggerFactory? loggerFactory = null)
    {
        _host = host;
        _port = port;
        _logger = loggerFactory == null ? NullLogger<MT4Service>.Instance : loggerFactory.CreateLogger<MT4Service>();
    }

    public async Task<ApiResult<GetAccountInfoResult>> GetAccountInfoAsync(long login)
        => await GetAccountInfoRequest.Build(login)
            .SendBySocketRequestAsync<GetAccountInfoRequest, GetAccountInfoResult>(_host, _port, _logger,
                SendTimeout, ReceiveTimeout);
    //
    // public Task<ApiResult<GetAccountBalanceInfoResult>> GetAccountBalanceInfoAsync(long login)
    // {
    //     throw new NotImplementedException();
    // }

    public async Task<ApiResult<CreateAccountResponse>> CreateAccountAsync(CreateAccountRequest request)
        => await request
            .SendBySocketRequestAsync<CreateAccountRequest, CreateAccountResponse>(_host, _port, _logger,
                SendTimeout, ReceiveTimeout);

    public Task<ApiResult<UpdateAccountResponse>> UpdateAccountAsync(UpdateAccountRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResult<ChangeBalanceResponse>> ChangeBalanceAsync(ChangeBalanceRequest request)
    {
        var response = await request
            .SendBySocketRequestAsync<ChangeBalanceRequest, ChangeBalanceResponse>(_host, _port, _logger,
                SendTimeout, ReceiveTimeout);
        _logger.LogInformation("ChangeBalanceAsync: {Message}", JsonConvert.SerializeObject(response));
        if (response.Status != ApiResultStatus.Success)  
        {
            BcrLog.Slack($"MT4_ChangeBalanceAsync_not_success: {response.Message}, " +
                         $"{request.RequestQuery()}, " +
                         $"{JsonConvert.SerializeObject(request)}, " +
                         $"{JsonConvert.SerializeObject(response)}");
        }
        return response;
    }

    public async Task<ApiResult<ChangeCreditResponse>> ChangeCreditAsync(ChangeCreditRequest request)
    {
        var response = await request
            .SendBySocketRequestAsync<ChangeCreditRequest, ChangeCreditResponse>(_host, _port, _logger,
                SendTimeout, ReceiveTimeout);
        _logger.LogInformation("ChangeCreditAsync: {Message}", JsonConvert.SerializeObject(response));
        if (response.Status != ApiResultStatus.Success)
        {
            BcrLog.Slack($"ChangeCreditAsync_not_success: {response.Message}");
        }
        return response;
    }

    public async Task<ApiResult<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordRequest request)
        => await request
            .SendBySocketRequestAsync<ChangePasswordRequest, ChangePasswordResponse>(_host, _port, _logger,
                SendTimeout, ReceiveTimeout);

    public async Task<ApiResult<CheckPasswordResponse>> CheckPasswordAsync(CheckPasswordRequest request)
        => await request
            .SendBySocketRequestAsync<CheckPasswordRequest, CheckPasswordResponse>(_host, _port, _logger,
                SendTimeout, ReceiveTimeout);

    public async Task<ApiResult<ChangeLeverageResponse>> ChangeLeverageAsync(ChangeLeverageRequest request)
        => await request
            .SendBySocketRequestAsync<ChangeLeverageRequest, ChangeLeverageResponse>(_host, _port, _logger,
                SendTimeout, ReceiveTimeout);

    public async Task<string> GetGroupAndSymbols(string group, string symbol, int transId)
    {
        await Task.Delay(0);
        return "";
    }

    public async Task<ApiResult<List<AccountDailyReportResponse>>> GetDailyReport(long login, DateTime from,
        DateTime to)
    {
        await Task.Delay(0);

        return ApiResult<List<AccountDailyReportResponse>>.Error(ApiResultStatus.SystemError, "Not implemented");
    }
}

public static class MT4ServiceExt
{
    public static async Task<ApiResult<TR>> SendBySocketRequestAsync<T, TR>(
        this T me, string host, int port,
        ILogger logger,
        int sendTimeoutInMs = 5000, int receiveTimeoutInMs = 5000)
        where T : class, IRequest, IHasRequestQuery, new()
        where TR : class, IResponse, new()
    {
        string message;
        try
        {
            var query = me.RequestQuery();
            logger.LogInformation("query: {query}", query);
            var responseQuery =
                await RequestQueryAsync(host, port, query, sendTimeoutInMs, receiveTimeoutInMs);
            logger.LogInformation("Response: {Response}", responseQuery);
            return ApiResult.Build<TR>().FromQueryString(responseQuery);
        }
        catch (Exception ex)
        {
            logger.LogError("API Request error: {Message}", ex.Message);
            message = ex.Message;
        }

        return ApiResult<TR>.Error(ApiResultStatus.SystemError, message);
    }

    private static async Task<string> RequestQueryAsync(string host, int port, string queryString,
        int sendTimeoutInMs = 5000, int receiveTimeoutInMs = 5000)
    {
        using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        socket.SendTimeout = sendTimeoutInMs;
        socket.ReceiveTimeout = receiveTimeoutInMs;
        var result = socket.BeginConnect(host, port, null, null);
        var success = result.AsyncWaitHandle.WaitOne(sendTimeoutInMs, true);
        if (success)
        {
            socket.EndConnect(result);
        }
        else
        {
            socket.Close();
            throw new SocketException(10060); // Connection timed out.
        }

        var requestBytes = Encoding.UTF8.GetBytes(queryString + '\0');
        var totalBytesToSend = requestBytes.Length;
        var bytesSent = 0;
        while (bytesSent < totalBytesToSend)
        {
            var buffer = new ArraySegment<byte>(requestBytes, bytesSent, totalBytesToSend - bytesSent);
            bytesSent += await socket.SendAsync(buffer, SocketFlags.None);
        }

        var receiverBuffer = new byte[1024];
        var resultStringBuilder = new StringBuilder();
        var contentSize = 0L;
        do
        {
            await socket.ReceiveAsync(receiverBuffer);
            var results = Encoding.ASCII.GetString(receiverBuffer)
                .Trim()
                .Trim('\0')
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (!results.Any())
                break;

            var item = results.First();

            if (long.TryParse(item, out var contentLength))
            {
                contentSize = contentLength;
                item = results.Skip(1).FirstOrDefault();

                if (item == null) continue;
            }

            resultStringBuilder.Append(item);
        } while (resultStringBuilder.Length < contentSize);

        var response = resultStringBuilder.ToString().Trim().Trim('\0');
        return response;
    }
}