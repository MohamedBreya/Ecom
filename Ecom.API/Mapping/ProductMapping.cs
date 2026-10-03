using AutoMapper;
using Ecom.core.DTO;
using Ecom.core.Entities.Product;

namespace Ecom.API.Mapping
{
    public class ProductMapping : Profile
    {
        public ProductMapping()
        {
            CreateMap<Product, ProductDTO>()
                .ForMember(dto => dto.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<Photo, PhotoDTO>();

            // Photos are stored by the repository, so the form files are never mapped onto the entity.
            CreateMap<AddProductDTO, Product>()
                .ForMember(p => p.Id, opt => opt.Ignore())
                .ForMember(p => p.Photos, opt => opt.Ignore());
            CreateMap<UpdateProductDTO, Product>()
                .ForMember(p => p.Id, opt => opt.Ignore())
                .ForMember(p => p.Photos, opt => opt.Ignore());
        }
    }
}
