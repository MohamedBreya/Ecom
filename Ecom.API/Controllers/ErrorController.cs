using Ecom.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers
{
    /// <summary>
    /// Target of <c>UseStatusCodePagesWithReExecute</c>: gives bodiless responses (unknown route, 405, ...)
    /// the standard error envelope.
    /// </summary>
    [Route("errors/{statusCode:int}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch]
        public IActionResult Error(int statusCode)
            => new ObjectResult(new ApiErrorResponse(statusCode)) { StatusCode = statusCode };
    }
}
