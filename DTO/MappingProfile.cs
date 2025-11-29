using AutoMapper;
using ProductCatalogCaseStudy.Models;

namespace ProductCatalogCaseStudy.DTO
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity to DTO map
            CreateMap<Product, ProductPatchDto>();

            // Map DTO back to Entity for PATCH updates:
            CreateMap<ProductPatchDto, Product>();
        }
    }
}
