# Use the official Microsoft SDK image to build the application
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
# This step is often cached if only source code changes
COPY *.csproj .
RUN dotnet restore

# Copy the rest of the application source code
COPY . .

# Publish the application in Release configuration
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
# Use the official ASP.NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose the internal container port
EXPOSE 8080
EXPOSE 8081

# Entrypoint for the application
ENTRYPOINT ["dotnet", "ProductCatalogCaseStudy.dll"]