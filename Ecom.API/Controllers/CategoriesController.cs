using AutoMapper;
using Ecom.API.Models;
using Ecom.core.DTO;
using Ecom.core.Entities.Product;
using Ecom.core.Exceptions;
using Ecom.core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers
{
    [Route("api/categories")]
    public class CategoriesController : BaseController
    {
        public CategoriesController(IUnitOfWork work, IMapper mapper) : base(work, mapper)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryResponseDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await work.CategoryRepository.GetAllAsync();
            return Ok(new ApiResponse<List<CategoryResponseDTO>>(200, mapper.Map<List<CategoryResponseDTO>>(categories)));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await FindAsync(id);
            return Ok(new ApiResponse<CategoryResponseDTO>(200, mapper.Map<CategoryResponseDTO>(category)));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CategoryDTO categoryDTO)
        {
            var category = mapper.Map<Category>(categoryDTO);
            await work.CategoryRepository.AddAsync(category);

            var response = new ApiResponse<CategoryResponseDTO>(201, mapper.Map<CategoryResponseDTO>(category), "Category created.");
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, response);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, CategoryDTO categoryDTO)
        {
            var category = await FindAsync(id);
            mapper.Map(categoryDTO, category);
            await work.CategoryRepository.UpdateAsync(category);

            return Ok(new ApiResponse<CategoryResponseDTO>(200, mapper.Map<CategoryResponseDTO>(category), "Category updated."));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            await FindAsync(id);
            if (await work.CategoryRepository.HasProductsAsync(id))
                throw new ConflictException("The category still has products. Move or delete them first.");

            await work.CategoryRepository.DeleteAsync(id);
            return Ok(new ApiResponse(200, "Category deleted."));
        }

        private async Task<Category> FindAsync(int id)
            => await work.CategoryRepository.GetByIdAsync(id)
               ?? throw new NotFoundException($"Category with id {id} was not found.");
    }
}
