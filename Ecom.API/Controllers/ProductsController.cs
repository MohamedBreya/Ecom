using AutoMapper;
using Ecom.API.Models;
using Ecom.core.DTO;
using Ecom.core.Entities.Product;
using Ecom.core.Exceptions;
using Ecom.core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers
{
    [Route("api/products")]
    public class ProductsController : BaseController
    {
        public ProductsController(IUnitOfWork work, IMapper mapper) : base(work, mapper)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var products = await work.ProductRepository.GetAllAsync(p => p.Category, p => p.Photos);
            return Ok(new ApiResponse<List<ProductDTO>>(200, mapper.Map<List<ProductDTO>>(products)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await FindAsync(id);
            return Ok(new ApiResponse<ProductDTO>(200, mapper.Map<ProductDTO>(product)));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromForm] AddProductDTO addProductDTO)
        {
            var created = await work.ProductRepository.AddAsync(addProductDTO);
            var product = await FindAsync(created.Id);

            var response = new ApiResponse<ProductDTO>(201, mapper.Map<ProductDTO>(product), "Product created.");
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateProductDTO updateProductDTO)
        {
            await work.ProductRepository.UpdateAsync(id, updateProductDTO);
            var product = await FindAsync(id);

            return Ok(new ApiResponse<ProductDTO>(200, mapper.Map<ProductDTO>(product), "Product updated."));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await work.ProductRepository.DeleteAsync(id))
                throw new NotFoundException($"Product with id {id} was not found.");

            return Ok(new ApiResponse(200, "Product deleted."));
        }

        private async Task<Product> FindAsync(int id)
            => await work.ProductRepository.GetByIdAsync(id, p => p.Category, p => p.Photos)
               ?? throw new NotFoundException($"Product with id {id} was not found.");
    }
}
