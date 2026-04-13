
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Controllers;

[ApiController]
[Route("/api/type")]
[Tags("Public/Type")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
public class TypeController : BaseController
{
    /// <summary>
    /// Get System defined types
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Index() =>
        Ok(GetTypes());

    private static object GetTypes() =>
        new
        {
            LeverageTypes = LeverageTypes(),
            AccountTypes = EnumToDictionary<AccountTypes>(),
            CurrencyTypes = EnumToDictionary<CurrencyTypes>(),
            AccountRoleTypes = EnumToDictionary<AccountRoleTypes>(),
        };

    private static Dictionary<int, int> LeverageTypes() =>
        new()
        {
            { 20, 20 },
            { 25, 25 },
            { 30, 30 },
            { 50, 50 },
            { 100, 100 },
            { 200, 200 },
            { 400, 400 },
        };

    private static Dictionary<string, int> EnumToDictionary<T>()
        where T : Enum
    {
        var dict = new Dictionary<string, int>();
        foreach (var value in Enum.GetValues(typeof(T)))
        {
            //var intValue = (int)value;
            var intValue = (int)value;
            var stringValue = Enum.GetName(typeof(T), value);
            if (stringValue != null) dict.Add(stringValue, intValue);
        }

        return dict;
    }
}