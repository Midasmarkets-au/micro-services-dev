using System.Diagnostics;
using Bacera.Gateway.Vendor.Amazon;
using Bacera.Gateway.Vendor.Amazon.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
var listPath = Path.Combine(rootPath, "_list.txt");
var emailList = await File.ReadAllLinesAsync(listPath);

var successPath = Path.Combine(rootPath, "_success.txt");
var failedPath = Path.Combine(rootPath, "_error.txt");

if (!File.Exists(successPath))
    await File.WriteAllTextAsync(successPath, "");

if (!File.Exists(failedPath))
    await File.WriteAllTextAsync(failedPath, "");

await Task.Delay(100);
var successList = await File.ReadAllLinesAsync(successPath);
var failedList = await File.ReadAllLinesAsync(failedPath);

const string layoutHtml =
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
                    <td style="font-size: 26px; font-weight: 700;padding-left: 2rem;height:30px;">{{Title}}</td>
                  </tr>
                  <tr><td style="padding-left: 2rem;height:10px;"><div style="width: 12rem; border-top: 3px solid #ffd400"></div></td></tr>
                  <tr>
                    <td style="font-weight: 600;padding-left: 2rem;font-size: 16px;height:30px;">{{Subtitle}}</td>
                  </tr>
                  <tr><td style="height:30px;"></td></tr>
                  <tr>
                    <td style="padding-left: 2rem;height:30px;"><a href="https://cfds.thebcr.com" style="color: white;font-size:16px;text-decoration: none;">
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
              </td>
            </tr>
          </table>
        </div>
        <div style="background-color:#efeeee;width:100%;height:100%">
          <table width="600" style="background-color:#ffffff;max-width: 600px;border:none;border-spacing:0;margin:auto;">
            <tr>
              <td></td><td></td>
            </tr>
            <tr>
              <td colspan="2" style="border-bottom: 2px solid #ffd400;background-color: #e9edeb;height: 15px;"></td>
            </tr>
            <tr>
              <td colspan="2" style="font-size:11px;text-align: center;height: 35px;padding:10px 20px;">
                BCR is committed to providing you with professional and transparent
                service in the financial services industry. Be sure to check our
                website at <a href="https://cfds.thebcr.com">cfds.thebcr.com</a> for the latest updates to our <a href="https://cfds.thebcr.com/disclosure-documents">Terms and Conditions</a>
              </td>
            </tr>
          </table>
          <table style="background-color:#ffffff;max-width: 600px;border:none;border-spacing:0;margin:auto;" width="100%">
            <tr >
              <td style="text-align:center;" width="34%">
                <a href="https://cfds-portal.thebcr.com/" style="align-items: center;text-transform: uppercase;text-decoration: none;color: #0053ad;">
                  <img src="https://img.thebcr.com/email/interface-user-multiple.png"style="width: 4rem; height: 4rem"/>
                  <p>Account</p>
                </a>
              </td>
              <td style="text-align:center;" width="33%">
                <a href="https://cfds-portal.thebcr.com" style="align-items: center;text-transform: uppercase;text-decoration: none;color: #0053ad;">
                  <img src="https://img.thebcr.com/email/money-atm-card-1.png"style="width: 4rem; height: 4rem"/>
                  <p>Deposit</p>
                </a>
              </td>
              <td style="text-align:center;" width="33%">
                <a href="https://cfds.thebcr.com/contact-us" style="align-items: center;text-transform: uppercase;text-decoration: none;color: #0053ad;">
                  <img src="https://img.thebcr.com/email/help-question-message.png"style="width: 4rem; height: 4rem"/>
                  <p>Get Help</p>
                </a>
              </td>
            </tr>
          </table>
          <table width="100%" style="background-color:#ffffff;max-width: 600px;border:none;border-spacing:0;margin:auto;">
            <tr>
              <td></td><td></td>
            </tr>
            <tr style='background-color: #e9edeb;'><td col-span="2"><img src="https://img.thebcr.com/email/emailrewardnew.jpg" alt="thebcr" style="width: 100.5%" /></td></tr>
            <tr style="background-color: #e9edeb;">
              <td col=span="2" style="padding: 0px 0px">
                <table style="width:100.5%;background-color: #e9edeb;">
                  <tr >
                    <td width="40%" style="text-align: left;"><img src="https://img.thebcr.com/logo/logo.png" alt="thebcr" width="140"/></td>
                    <td widht="60%" style="text-align: right;padding: 10px 20px;">
                      <a href="https://www.facebook.com/BCRbridgethedifference"><img src="https://img.thebcr.com/email/facebook_black.png" style="width: 23px" /></a>
                      <a href="https://www.instagram.com/the__bcr/"><img src="https://img.thebcr.com/email/ig_black.png" style="width: 23px" /></a>
                      <a href="https://www.linkedin.com/company/thebcr"><img src="https://img.thebcr.com/email/linkedin_black.png" style="width: 23px" /></a>
                      <a href="https://twitter.com/the__bcr"><img src="https://img.thebcr.com/email/twitter_black.png" style="width: 23px" /></a>
                      <a href="https://www.youtube.com/@BCRMarketsAu"><img src="https://img.thebcr.com/email/ytb_black.png" style="width: 23px" /></a>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
            <tr style='background-color: #e9edeb;'>
              <td colspan="2" style="padding: 10px 20px;">
                <a href="tel:+44 3300010590" style="text-decoration: none;color: #0053ad;">+44 3300010590</a>
                &nbsp;&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;&nbsp;
                <a href="mailto:info@thebcr.com" style="text-decoration: none; color: #0053ad">info@thebcr.com</a>
              </td>
            </tr>
            <tr style='background-color: #e9edeb;'>
              <td colspan="2" style="font-size:12px;padding: 10px 20px;">
                Address: BCR Co Pty Ltd, Trident Chambers, Wickham’s Cay 1, Road Town, <br>Tortola, British Virgin Islands
              </td>
            </tr>
            <tr style='background-color: #e9edeb;'>
              <td colspan="2" style="font-size:11px;padding: 10px 20px;">
                <p style="text-align: justify;"> Risk Disclosure: Trading CFDs involves significant risk and can result in the loss of your invested capital. You should not invest more than you can afford to lose and should ensure that you fully understand the risks involved. Trading leveraged products may not be suitable for all investors. You do not own or have any interest in the underlying asset. BCR does not issue advice, recommendations, or opinions in relation to acquiring, holding, or disposing of a CFD. BCR is not a financial advisor and all services are provided on an execution only basis. Please consider our Product Disclosure Statement and Financial Services Guide available from BCR before entering any transaction with us.</p>
                <p style="text-align: justify;">The material above does not contain (and should not be construed as containing) personal financial or investment advice or other recommendations, or an offer of, or solicitation for, a transaction in any financial instrument. No representation or warranty is given as to the accuracy or completeness of the above information. Consequently, any person acting on it does so entirely at his or her own risk. The information does not have regard to the specific investment objectives, financial situation and needs of any specific person who may receive it. BCR accepts no responsibility for any use that may be made of these comments and for any consequences that result</p>
                <p style="text-align: justify;">The material above is not intended for residents of the United States, or use by any person in any country or jurisdiction where such distribution or use would be contrary to local law or regulation. </p>         <br />
                Copyright @ 2023 BCR
              </td>
            </tr>
          </table>
        </div>
      </body>
    </html>
    """;

const string contentHtml =
    """
    <div style="font-size: 14px; line-height: 20px">
    <p>Dear customer:</p>
    <p>Due to upgrades to MyBCR, the Client Portal will be unavailable after Market Close on Friday, <b>26 August 2023</b>, and will be available again for regular use before Market Open on <b>Monday, 28 August 2023.</b></p>
    <p>If you need client service for any reason over the weekend, please contact BCR Support at info@thebcr.com.</p>
    <p>If you have any further questions regarding the above upgrades, please contact us via online chat or email us at info@thebcr.com. We look forward to providing you with the best service in the financial services industry.</p>
    <br />
    Regards,
    <br />
    <b>BCR</b>
    <br />
    </div>
    """;


const string title = "MyBCR Important Message";
const string subtitle = "Upgrades to MyBCR.";
var html = layoutHtml
        .Replace("{{Title}}", title)
        .Replace("{{Subtitle}}", subtitle)
        .Replace("{{Content}}", contentHtml)
    ;
var sent = 0;
var failed = 0;
var successListBatch = successList.ToList();
var failedListBatch = failedList.ToList();


var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var optionsValue = new AwsSesOptions();
configuration.GetSection("Options").Bind(optionsValue);
optionsValue.Region = Amazon.RegionEndpoint.GetBySystemName(configuration.GetSection("Options:Region").Value);
var options = Options.Create(optionsValue);
var svc = new AwsEmailSender(options);

const int countPerSecond = 5;
var stopwatch = new Stopwatch();
var count = 0;

stopwatch.Start();
foreach (var email in emailList)
{
    if (successListBatch.Any(x => x.Equals(email, StringComparison.OrdinalIgnoreCase)))
        continue;
    if (failedListBatch.Any(x => x.Equals(email, StringComparison.OrdinalIgnoreCase)))
        continue;
    try
    {
        var result = await svc.SendEmailAsync(email, title, html);
        count++;

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

        if (count >= countPerSecond)
        {
          if (stopwatch.ElapsedMilliseconds < 1000) // less than 1 second
          {
            await Task.Delay(1000 - (int)stopwatch.ElapsedMilliseconds); // wait till next second
          }

          // reset stopwatch
          count = 0;
          stopwatch.Reset();
        }
        
        if ((sent + failed) % 100 == 0)
            Console.WriteLine($"Sent: {sent}, Failed: {failed}, Total: {sent + failed}");
    }
    catch (Exception)
    {
        failed++;
        failedListBatch.Add(email);
        await File.AppendAllLinesAsync(failedPath, new[] { email });
    }

    await Task.Delay(20);
}

Console.WriteLine("Sent: " + sent);
Console.WriteLine("Failed: " + failed);
Console.WriteLine("Failed count: " + failedListBatch.Count);