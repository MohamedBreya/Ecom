using AutoMapper;
using Ecom.core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly IUnitOfWork work;
        protected readonly IMapper mapper;

        public BaseController(IUnitOfWork work, IMapper mapper)
        {
            this.work = work;
            this.mapper = mapper;
        }
    }
}
