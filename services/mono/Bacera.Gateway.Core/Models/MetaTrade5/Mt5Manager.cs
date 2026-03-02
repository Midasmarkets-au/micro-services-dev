using System;
using System.Collections.Generic;

namespace Bacera.Gateway.Integration;

public partial class Mt5Manager
{
    public ulong Login { get; set; }

    public long Timestamp { get; set; }

    public string Name { get; set; } = null!;

    public string Mailbox { get; set; } = null!;

    public ulong Server { get; set; }

    public uint RequestLimitLogs { get; set; }

    public uint RequestLimitReports { get; set; }

    public string Groups { get; set; } = null!;

    public string Access { get; set; } = null!;

    public uint RightAdmin { get; set; }

    public uint RightManager { get; set; }

    public uint RightCfgServers { get; set; }

    public uint RightCfgAccess { get; set; }

    public uint RightCfgTime { get; set; }

    public uint RightCfgHolidays { get; set; }

    public uint RightCfgGroups { get; set; }

    public uint RightCfgManagers { get; set; }

    public uint RightCfgRequests { get; set; }

    public uint RightCfgGateways { get; set; }

    public uint RightCfgPlugins { get; set; }

    public uint RightCfgDatafeeds { get; set; }

    public uint RightCfgReports { get; set; }

    public uint RightCfgSymbols { get; set; }

    public uint RightCfgHstSync { get; set; }

    public uint RightCfgEcn { get; set; }

    public uint RightSrvJournals { get; set; }

    public uint RightSrvReports { get; set; }

    public uint RightCharts { get; set; }

    public uint RightEmail { get; set; }

    public uint RightNews { get; set; }

    public uint RightExport { get; set; }

    public uint RightTechsupport { get; set; }

    public uint RightMarket { get; set; }

    public uint RightAccountant { get; set; }

    public uint RightAccRead { get; set; }

    public uint RightAccDetails { get; set; }

    public uint RightAccManager { get; set; }

    public uint RightAccDelete { get; set; }

    public uint RightAccOnline { get; set; }

    public uint RightConfirmActions { get; set; }

    public uint RightNotifications { get; set; }

    public uint RightTradesRead { get; set; }

    public uint RightTradesManager { get; set; }

    public uint RightTradesDelete { get; set; }

    public uint RightTradesDealer { get; set; }

    public uint RightTradesSupervisor { get; set; }

    public uint RightQuotesRaw { get; set; }

    public uint RightQuotes { get; set; }

    public uint RightSymbolDetails { get; set; }

    public uint RightRiskManager { get; set; }

    public uint RightGroupMargin { get; set; }

    public uint RightGroupCommission { get; set; }

    public uint RightReports { get; set; }

    public uint RightFintezaAccess { get; set; }

    public uint RightFintezaWebsites { get; set; }

    public uint RightFintezaCampaigns { get; set; }

    public uint RightFintezaReports { get; set; }

    public uint RightClientsAccess { get; set; }

    public uint RightClientsCreate { get; set; }

    public uint RightClientsEdit { get; set; }

    public uint RightClientsDelete { get; set; }

    public uint RightDocumentsAccess { get; set; }

    public uint RightDocumentsCreate { get; set; }

    public uint RightDocumentsEdit { get; set; }

    public uint RightDocumentsDelete { get; set; }

    public uint RightDocumentsFilesAdd { get; set; }

    public uint RightDocumentsFilesDelete { get; set; }

    public uint RightCommentsAccess { get; set; }

    public uint RightCommentsCreate { get; set; }

    public uint RightCommentsDelete { get; set; }
}
