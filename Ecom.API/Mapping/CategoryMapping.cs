using AutoMapper;
using Ecom.core.DTO;
using Ecom.core.Entities.Product;

namespace Ecom.API.Mapping
{
    public class CategoryMapping:Profile
    {
        public CategoryMapping()
        {
            CreateMap<CategoryDTO,Category>().ReverseMap();
            CreateMap<UpdateCategoryDTO, Category>().ReverseMap();
        }
    }
}
