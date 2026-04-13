using Bacera.Gateway.DTO;
using Bacera.Gateway.Vendor;
using Bacera.Gateway.Vendor.BigPay;
using Bacera.Gateway.Vendor.BigPay.Models;
using Bacera.Gateway.Vendor.BipiPay;
using Bacera.Gateway.Vendor.Buy365;
using Bacera.Gateway.Vendor.ChipPay;
using Bacera.Gateway.Vendor.ExLink.Models;
using Bacera.Gateway.Vendor.FivePayF2F;
using Bacera.Gateway.Vendor.FivePayVA;
using Bacera.Gateway.Vendor.GPay;
using Bacera.Gateway.Vendor.Help2Pay;
using Bacera.Gateway.Vendor.Help2Pay.Models;
using Bacera.Gateway.Vendor.Long77Pay;
using Bacera.Gateway.Vendor.PaymentAsia;
using Bacera.Gateway.Vendor.Poli;
using Bacera.Gateway.Vendor.Poli.Models;
using Bacera.Gateway.Vendor.UnionePay;
using Bacera.Gateway.Vendor.UsePay;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Services;

public partial class DepositService
{
    public async Task<List<Deposit.ClientPageModel>> QueryForClientAsync(Deposit.ClientCriteria criteria)
    {
        var list = await tenantCtx.Deposits.PagedFilterBy(criteria).ToClientPageModel().ToListAsync();
        foreach (var row in list)
        {
            row.Info = new Deposit.ClientDepositInfo
            {
                PayType = row.Platform == (int)PaymentPlatformTypes.QrCodeTunnel ? "qrcode" : ""
            };
        }

        return list;
    }
}