
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bacera.Gateway.Web.Areas.Client.Controllers;

[Tags("Client/Symbol")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
    Roles = UserRoleTypesString.ClientOrTenantAdmin)]
public class SymbolController(TenantDbContext dbContext) : ClientBaseController
{
    /// <summary>
    /// Get all Symbols
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Index()
    {
        var items = await dbContext.Symbols
            .GroupBy(x => x.Code)
            .Select(x => new { Code = x.Key })
            .ToListAsync();
        return Ok(Result.Of(items));
    }

    // /// <summary>
    // /// Get Symbol by Id
    // /// </summary>
    // /// <param name="id"></param>
    // /// <returns></returns>
    // [HttpGet("{id:int}")]
    // [ProducesResponseType(typeof(Symbol.ResponseModel), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<Symbol.ResponseModel>> Get(int id)
    // {
    //     var item = await _dbContext.Symbols
    //         .Include(x => x.Category)
    //         .Include(x => x.SymbolInfo)
    //         .SingleOrDefaultAsync(x => x.Id == id);
    //     return item == null ? NotFound() : Ok(item.ToResponseModel());
    // }
    //
    // /// <summary>
    // /// Get Symbol by Code
    // /// </summary>
    // /// <param name="code"></param>
    // /// <returns></returns>
    // [HttpGet("{code}")]
    // [ProducesResponseType(typeof(Symbol.ResponseModel), StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<ActionResult<Symbol.ResponseModel>> Get(string code)
    // {
    //     var item = await _dbContext.Symbols
    //         .Include(x => x.Category)
    //         .Include(x => x.SymbolInfo)
    //         .SingleOrDefaultAsync(x => x.Code == code);
    //     return item == null ? NotFound() : Ok(item.ToResponseModel());
    // }
    //
    // /// <summary>
    // /// Symbol Categories
    // /// </summary>
    // /// <returns></returns>
    // [HttpGet("category")]
    // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SymbolCategory>))]
    // public async Task<IActionResult> Category()
    // {
    //     var items = await _dbContext.SymbolCategories
    //         .Include(x => x.Symbols)
    //         .OrderBy(x => x.Id)
    //         .ToListAsync();
    //     return Ok(items);
    // }
}

