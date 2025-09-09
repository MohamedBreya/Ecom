using AutoMapper;
using Ecom.API.Helper;
using Ecom.core.DTO;
using Ecom.core.Entities.Product;
using Ecom.core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace Ecom.API.Controllers
{
    public class CategoriesController : BaseController
    {
        public CategoriesController(IUnitOfWork work, IMapper mapper) : base(work, mapper)
        {
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> get()
        {
            try
            {
                var category= await work.CategoryRepository.GetAllAsync();
                if (category is null)
                    return BadRequest(new ResponseAPI(400));
                else
                    return Ok(category);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> getbyId(int id)
        {
            try
            {
                var category = await work.CategoryRepository.GetByIdAsync(id);
                if (category is null)
                    return BadRequest(new ResponseAPI(400,$"Not Found Category Of Id : {id}"));
                else
                    return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("add-category")]
        public async Task<IActionResult> add(CategoryDTO categoryDTO)
        {
            try
            {
                var category = mapper.Map<Category>(categoryDTO);
                await work.CategoryRepository.AddAsync(category);
                return Ok(new ResponseAPI(200 , "New Item Has Been Added" ));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("update-category")]
        public async Task<IActionResult>update(UpdateCategoryDTO updatecategoryDTO)
        {
            try
            {
                var category = mapper.Map<Category>(updatecategoryDTO);
                await work.CategoryRepository.UpdateAsync(category);
                return Ok(new ResponseAPI(200,"Item Has Been Updated"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("delete-category/{id}")]
        public async Task<IActionResult>delete(int id)
        {
            try
            {
                await work.CategoryRepository.DeleteAsync(id);
                return Ok(new ResponseAPI(200,"New Item Has Been Deleted" ));
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}
