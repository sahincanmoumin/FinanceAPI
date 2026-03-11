using BusinessLayer.Abstract;
using BusinessLayer.Concrete;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiLayer.Controllers
{
    
    public class BaseController : ControllerBase
    {

        protected int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        protected bool IsAdmin() => User.IsInRole("Admin");
        

    }
}