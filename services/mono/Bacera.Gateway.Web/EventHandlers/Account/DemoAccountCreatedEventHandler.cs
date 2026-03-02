using Bacera.Gateway.Core.Types;
using Bacera.Gateway.Services;
using Bacera.Gateway.Web.BackgroundJobs;
using Hangfire;
using MediatR;

namespace Bacera.Gateway.Web.EventHandlers;

public class DemoAccountCreatedEvent : INotification
{
    public long PartyId { get; }
    public string Name { get; }
    public string Email { get; }
    public string PhoneNumber { get; }
    public long AccountNumber { get; }
    public string Password { get; }
    public string ServerName { get; }
    public string? Utm { get; }

    public DemoAccountCreatedEvent(
        long partyId
        , string password
        , string name
        , string email
        , string phoneNumber
        , long accountNumber
        , string serverName
        , string? utm = null
    )
    {
        PartyId = partyId;
        Password = password;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        AccountNumber = accountNumber;
        ServerName = serverName;
        Utm = utm;
    }
}

public class DemoAccountCreatedEventHandler : INotificationHandler<DemoAccountCreatedEvent>
{
    private readonly ILeadService _leadService;
    private readonly Tenancy _tenancy;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<DemoAccountCreatedEventHandler> _logger;

    public DemoAccountCreatedEventHandler(
        Tenancy tenancy
        , ILeadService leadService
        , IBackgroundJobClient backgroundJobClient
        , ILogger<DemoAccountCreatedEventHandler> logger
    )
    {
        _leadService = leadService;
        _tenancy = tenancy;
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public async Task Handle(DemoAccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        var password = notification.Password;
        var viewModel = new TradeDemoAccountCreatedViewModel
        {
            Email = notification.Email,
            Password = password,
            AccountNumber = notification.AccountNumber,
            NativeName = notification.Name,
            ServerName = notification.ServerName
        };

        _backgroundJobClient.Enqueue<IGeneralJob>(x =>
            x.TradeDemoAccountCreatedAsync(_tenancy.GetTenantId(), viewModel, "en-us"));
        var spec = Lead.CreateSpec.Build(
            notification.PartyId
            , notification.Name
            , notification.Email
            , notification.PhoneNumber
            , LeadSourceTypes.DemoAccount
            , LeadStatusTypes.DemoAccountCreated
        );

        if (notification.Utm != null)
        {
            spec.Supplement = new Dictionary<string, LeadItem>
            {
                { "utm", new LeadItem { Data = notification.Utm } }
            };
        }

        await _leadService.CreateAsync(spec);
    }
}