using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace ProductCatalogCaseStudy
{
    /// <summary>
    /// Tweak the swagger configuration to ensure the versions of the API are correctly handled in a way agnostic of the actual versions
    /// </summary>
    public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider = provider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options">The swagger documentation generator options</param>
        public void Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
        {
            // Dynamically create a Swagger document for each discovered API version
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                var info = CreateInfoForApiVersion(description);
                options.SwaggerDoc(description.GroupName, info);
            }
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = $"Product Catalog API",
                Version = description.ApiVersion.ToString(),
                Description = "An ASP.NET Core Web API for a catalog of products. Study Case part of the Alza recruitement process. "
                    + (description.IsDeprecated ? "This API version is deprecated and will be removed soon." : "Current Version of the API")
                                ,
                Contact = new OpenApiContact
                {
                    Name = "Author",
                    Url = new Uri("https://www.linkedin.com/in/pierre-francois-giraud-835288108/")
                }
            };
            return info;
        }
    }
}
