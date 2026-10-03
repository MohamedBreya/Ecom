using AutoMapper;
using Ecom.core.DTO;
using Ecom.core.Entities.Product;

namespace Ecom.API.Mapping
{
    public class CategoryMapping : Profile
    {
        public CategoryMapping()
        {
            CreateMap<Category, CategoryResponseDTO>();
            CreateMap<CategoryDTO, Category>().ForMember(c => c.Id, opt => opt.Ignore());
        }
    }
}
