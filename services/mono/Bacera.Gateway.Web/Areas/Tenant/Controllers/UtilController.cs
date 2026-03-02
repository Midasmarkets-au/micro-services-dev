using OpenIddict.Validation.AspNetCore;
using Bacera.Gateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Tenant.Controllers;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Tags("Tenant/Util")]
public class UtilController(GptService gptService, TenantDbContext ctx) : TenantBaseController
{
    [HttpGet("available-lang")]
    public IActionResult AvailableLang() => Ok(LanguageTypes.All);

    [HttpPost("translate")]
    public async Task<IActionResult> Translate(ToolExtension.TranslateSpec spec)
    {
        if (!LanguageTypes.LangEnumToWord.TryGetValue(spec.TargetLanguage, out var lang))
            return BadRequest("__INVALID_TARGET_LANGUAGE__");

        if (string.IsNullOrWhiteSpace(spec.Text))
            return BadRequest("__TEXT_IS_REQUIRED__");

        var (result, content) = await gptService.TranslateTextToTargetAsync(lang, spec.Text);
        if (!result) return BadRequest(content);
        return Ok(content);
    }



}

public abstract class ToolExtension
{
    public class TranslateSpec
    {
        public string Text { get; set; } = string.Empty;
        public string TargetLanguage { get; set; } = string.Empty;
    }
}