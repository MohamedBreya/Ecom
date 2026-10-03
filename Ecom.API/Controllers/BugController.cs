using AutoMapper;
using Ecom.core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers
{
    /// <summary>Endpoints for exercising error handling. They only respond in the Development environment.</summary>
    [Route("api/bug")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BugController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public BugController(IUnitOfWork work, IMapper mapper, IWebHostEnvironment environment) : base(work, mapper)
        {
            _environment = environment;
        }

        [HttpGet("not-found")]
        public IActionResult GetNotFound() => NotFound();

        [HttpGet("server-error")]
        public IActionResult GetServerError()
        {
            if (!_environment.IsDevelopment())
                return NotFound();

            throw new InvalidOperationException("Simulated server error.");
        }

        [HttpGet("bad-request/{id:int}")]
        public IActionResult GetBadRequest(int id)
            => _environment.IsDevelopment() ? Ok() : NotFound();

        [HttpGet("bad-request")]
        public IActionResult GetBadRequest()
            => _environment.IsDevelopment() ? BadRequest() : NotFound();
    }
}
