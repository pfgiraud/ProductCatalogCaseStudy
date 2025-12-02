# ProductAPI_CaseStudy
REST API service providing all available products of an eshop and enabling the partial update of one product.

## How to launch the app

### Docker Compose

This is the recommended way (the app is shipped with the database that get initialized automatically). Assuming that you have a functionning docker installation, you can follow those steps:
- Fill the variables in the ".env" file (SA_PASSWORD, DB_USER, DB_PASSWORD) with values of your choice, to configure database connection.
    - The passwords should be at least 8 characters long and have 3 of the following character types: Number, Uppercase, Lowercase, Symbols.
- Launch the docker compose file either by:
    - In Visual Studio, choosing the project "docker-compose" in the solution and launch the application
    - Using the command "docker compose up"

### Classic way

You can also launch the application without docker either by:
- In Visual Studio, choosing the project "ProductCatalogCaseStudy" and a profile (like https)
- Running the command "dotnet run" inside the "ProductCatalogCaseStudy" project (and preferably specifying the profile with the --launch-profile option)

Either way you need to first specify the connection data to an existing SQL Server database by setting the following env vars:
- DB_HOST: 127.0.0.1 for local database or some server where the database is situated
- DB_NAME: the name of the database you want to use
- DB_USER: The login of an account with access to the database
- DB_PASSWORD: The corresponding password

The env vars can be set for example in the Properties/launchSettings.json file, for a specific profile under the profiles.{profile}.environmentVariables path.

### Without docker

## Release Notes

- API v2:
    - Updated the GET /products endpoint: Now support pagination.
- API v1:
    - Added the GET /products endpoint: List all the products in the catalog
    - Added the GET /products/{id} endpoint: Get the details of a specific product in the catalog
    - Added the PATCH /products/{id} endpoint: Update one or more fields of a specific product in the catalog

For further details about the operations available, please check the documentation in your browser at the root URL (which for local use would likely be: https://localhost:8081)