using Microsoft.AspNetCore.Mvc;

namespace volunteerplatform.Controllers
{
    public class ErrorsController : Controller
    {
        [Route("Errors/{code:int}")]
        public IActionResult HandleError(int code)
        {
            ViewData["ErrorCode"] = code;
            
            return code switch
            {
                404 => View("NotFound"),
                403 => View("Forbidden"),
                _ => View("Error")
            };
        }

        [Route("Errors/500")]
        public IActionResult Error500()
        {
            return View("Error");
        }
    }
}
