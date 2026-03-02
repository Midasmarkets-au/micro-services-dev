using Microsoft.AspNetCore.Mvc;

namespace Bacera.Gateway.Web.Areas.Admin.Controllers;

using M = Medium;

public class MediaController : AdminBaseController
{
    private readonly CentralDbContext _centralDbContext;

    public MediaController( CentralDbContext centralDbContext
        )
    {
        _centralDbContext = centralDbContext;
    }
    
    // [HttpGet]
    // public IActionResult Index([FromQuery] M.Criteria criteria)
    // {
    //     criteria ??= new M.Criteria();
    //     var items= await _centralDbContext.Media
    //     return View();
    // }

}