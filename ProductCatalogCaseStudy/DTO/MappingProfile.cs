using AutoMapper;
using ProductCatalogCaseStudy.Models;

namespace ProductCatalogCaseStudy.DTO
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map DTO back to Entity for PATCH updates:
            CreateMap<ProductPatchDto, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
