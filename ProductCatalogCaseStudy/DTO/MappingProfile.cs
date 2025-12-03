using AutoMapper;
using ProductCatalogCaseStudy.Models;

namespace ProductCatalogCaseStudy.DTO
{
    /// <summary>
    /// Profile for mapping configurations using AutoMapper
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// List of all mappings in the application.
        /// </summary>
        public MappingProfile()
        {
            // Map DTO back to Entity for PATCH updates:
            CreateMap<ProductPatchDto, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
