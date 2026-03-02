using System.Diagnostics;
using Bacera.Gateway.Vendor.Amazon.Options;
using Bacera.Gateway.Vendor.Amazon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Bacera.Gateway.Infrastructure.Tests;

[Trait(TraitTypes.Types, TraitTypes.Value.Service)]
[Trait(TraitTypes.Parties, TraitTypes.Value.ThirdParty)]
public class AwsSendMailTests : Startup
{
    private readonly IEmailSender _svc;

    public AwsSendMailTests()
    {
        var options = ServiceProvider.GetRequiredService<IOptions<AwsSesOptions>>();
        _svc = new AwsEmailSender(options);
    }

    [Fact]
    public async Task SendHtmlTest()
    {
        // Act
        var result = await _svc.SendEmailAsync("feng@bacera.com", "Test subject (from Unit Test)",
            "<html><body><H1>Hello World</H1><br/><strong>From Unit Test</strong></body></html>");

        // Assert
        result.Item1.ShouldBeTrue();
        result.Item2.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task SendNoticeEvent()
    {
        var title = "MyBCR Important Message";
        var subtitle = "Upgrades to MyBCR.";
        var listPath = "C:/develop/debug/email_list.txt";
        var emailList = await File.ReadAllLinesAsync(listPath);
        emailList.Length.ShouldBe(14904);

        var successPath = "c:/develop/debug/sendmail.success.txt";
        var failedPath = "c:/develop/debug/sendmail.failed.txt";

        if (!File.Exists(successPath))
            File.Create(successPath);

        if (!File.Exists(failedPath))
            File.Create(failedPath);

        var successList = await File.ReadAllLinesAsync(successPath);
        var failedList = await File.ReadAllLinesAsync(failedPath);

        var html = Layout
                .Replace("{{Title}}", title)
                .Replace("{{Subtitle}}", subtitle)
                .Replace("{{Content}}", _body)
            ;
        var sent = 0;
        var failed = 0;
        var successListBatch = successList.ToList();
        var failedListBatch = failedList.ToList();
        foreach (var email in emailList)
        {
            if (successListBatch.Any(x => x.Equals(email, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (failedListBatch.Any(x => x.Equals(email, StringComparison.OrdinalIgnoreCase)))
                continue;

            try
            {
                var result = await _svc.SendEmailAsync(email, title, html);

                if (result.Item1)
                {
                    sent++;
                    successListBatch.Add(email);
                    await File.AppendAllLinesAsync(successPath, new[] { email });
                }
                else
                {
                    failed++;
                    failedListBatch.Add(email);
                    await File.AppendAllLinesAsync(failedPath, new[] { email });
                }
            }
            catch (Exception)
            {
                failed++;
                failedListBatch.Add(email);
                await File.AppendAllLinesAsync(failedPath, new[] { email });
            }

            await Task.Delay(100);
        }

        Debug.WriteLine("Sent: " + sent);
        Debug.WriteLine("Failed: " + failed);
        Debug.WriteLine("Failed count: " + failedListBatch.Count);
    }

    private const string Layout =
        """
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta http-equiv="Content-Type" content="text/html charset=UTF-8" />
    <title>Thebcr Email</title>
  </head>
  <body
    style="
      font-family: Lato, sans-serif;
    "
  >
<div style="background-color:#efeeee;width:100%;height:100%">
  <table width="600" style="background-color:#ffffff;max-width: 600px;border:none;border-spacing:0;margin:auto;">
<tr>
<td></td><td></td>
</tr>
    <tr>
      <td align="center" col-span=2><img src="https://img.thebcr.com/logo/logo.png" alt="" width="120px" /></td>
    </tr>
    <tr>
      <td colspan="2" style="height:15rem;background: url(https://img.thebcr.com/email/bg-people.jpeg);background-repeat: no-repeat;background-size: cover;">
        <table style="color: white; background: rgba(0, 0, 0, 0.5);height:100%;width: 100%;">
          <tr><td style="height:50px;"></td></tr>
          <tr>
            <td style="font-size: 24px; font-weight: 500;padding-left: 2rem;height:30px;">{{Title}}</td>
          </tr>
          <tr><td style="padding-left: 2rem;height:10px;"><div style="width: 12rem; border-top: 3px solid #ffd400"></div></td></tr>
          <tr>
            <td style="font-weight: 500;padding-left: 2rem;font-size: 16px;height:30px;">{{Subtitle}}</td>
          </tr>
          <tr><td style="height:30px;"></td></tr>
          <tr>
            <td style="padding-left: 2rem;height:30px;"><a href="https://thebcr.com" style="color: white;font-size:16px;text-decoration: none;">
              <div style="display: inline-block;border: 6px solid transparent;border-left-color: #ffd400;"></div>
              Login MyBCR</a>
            </td>
          </tr>
          <tr><td></td></tr>
        </table>
      </td>
    </tr>
    <tr><td colspan="2" style="padding: 30px 20px;">
      <table style="width:100%;">
        <tr><td>{{Content}}</td></tr>
      </table>
    </td></tr>
    <tr>
      <td colspan="2" style="border-bottom: 2px solid #ffd400;background-color: #e9edeb;height: 15px;"></td>
    </tr>
    <tr>
      <td colspan="2" style="font-size:11px;text-align: center;height: 35px;padding:10px 20px;">
        BCR is committed to providing you with professional and transparent
            service in the financial services industry. Be sure to check our
            website at <a href="https://thebcr.com">thebcr.com</a> for the latest updates to our <a href="https://au.thebcr.com/en/about-us/term-of-use">Terms and Conditions</a>
      </td>
    </tr>
<tr><td col-span="2"><img src="https://img.thebcr.com/email/emailreward.jpg" alt="thebcr" style="width: 100%" /></td></tr>
    <tr>
      <td col=span="2" style="padding: 0px 0px">
        <table style="width:100%;">
          <tr>
            <td width="50%" style="text-align: left;"><img src="https://img.thebcr.com/logo/logo.png" alt="thebcr" width="160"/></td>
            <td widht="50%" style="text-align: right;">
              <a href="https://www.facebook.com/BCRbridgethedifference"><img src="https://img.thebcr.com/email/facebook_black.png" style="width: 25px" /></a>
              <a href="https://www.instagram.com/the__bcr/"><img src="https://img.thebcr.com/email/ig_black.png" style="width: 25px" /></a>
              <a href="https://www.linkedin.com/company/thebcr"><img src="https://img.thebcr.com/email/linkedin_black.png" style="width: 25px" /></a>
              <a href="https://twitter.com/the__bcr"><img src="https://img.thebcr.com/email/twitter_black.png" style="width: 25px" /></a>
              <a href="https://www.youtube.com/@BCRMarketsAu"><img src="https://img.thebcr.com/email/ytb_black.png" style="width: 25px" /></a>
            </td>
          </tr>
        </table>
      </td>
    </tr>
    <tr>
      <td colspan="2" style="padding: 10px 20px;">
        <a href="tel:+61284598050" style="text-decoration: none;color: #0053ad;">+61 2 8459 8050</a>
        &nbsp;&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;&nbsp;
        <a href="mailto:info@au.thebcr.com" style="text-decoration: none; color: #0053ad">info@au.thebcr.com</a>
      </td>
    </tr>
    <tr>
      <td colspan="2" style="font-size:12px;padding: 10px 20px;">
        Head office: Level 24, 171 Sussex Street, Sydney, NSW 2000
        <br />
        Level 35, International Tower One, 100 Barangaroo Avenue, Sydney
        2000
        <br />
        Melbourne office: Suit 5005, 447 Collins Street, Melbourne 3000
      </td>
    </tr>
    <tr>
      <td colspan="2" style="font-size:11px;padding: 10px 20px;">
        Risk Disclosure: Trading CFDs involves significant risk and can
        result in the loss of your invested capital. You should not invest
        more than you can afford to lose and should ensure that you fully
        understand the risks involved. Trading leveraged products may not be
        suitable for all investors. You do not own or have any interest in
        the underlying asset. BCR does not issue advice, recommendations, or
        opinions in relation to acquiring, holding, or disposing of a CFD.
        BCR is not a financial advisor and all services are provided on an
        execution only basis. Please consider our Product Guide and Terms of
        Service available from BCR before entering any transaction with us.
        <br />
        <br />
        The material above does not contain (and should not be construed as
        containing) personal financial or investment advice or other
        recommendations, or an offer of, or solicitation for, a transaction
        in any financial instrument. No representation or warranty is given
        as to the accuracy or completeness of the above information.
        Consequently, any person acting on it does so entirely at his or her
        own risk. The information does not have regard to the specific
        investment objectives, financial situation and needs of any specific
        person who may receive it. BCR accepts no responsibility for any use
        that may be made of these comments and for any consequences that
        result
        <br />
        <br />
        The material above is not intended for residents of the United
        States, or use by any person in any country or jurisdiction where
        such distribution or use would be contrary to local law or
        regulation.
        <br />
        <br />
        Copyright @ 2023 BCR
      </td>
    </tr>
  </table>
</div>
  </body>
  </html>
""";

    private string _body = """
<div style="font-size: 14px; line-height: 20px">
<p>Due to upgrades to MyBCR, the Client Portal will be unavailable after Market Close on <b>Friday, 21 July 2023</b>, and will be available again for regular use before Market Open on <b>Monday, 24 July 2023</b>.
</p>
<p>If you need client service for any reason over the weekend, please contact BCR Support at <a href="mailto:info@au.thebcr.com">info@au.thebcr.com</a>.</p>
<p>If you have any further questions regarding the above upgrades, please contact us via online chat or email us at info@au.thebcr.com. We look forward to providing you with the best service in the financial services industry.</p>
<br />
Regards,
<br />
<b>BCR</b>
<br />
</div>
""";

    private List<string> _emailList = new()
    {
        "feng@bacera.com",
    };
}