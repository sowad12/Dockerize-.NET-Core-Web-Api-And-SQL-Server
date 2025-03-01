## Table of Contents
- [Requirements](#Requirements)
- [Short_brief](#Short_brief)
- [Web API](#Web_API)
- [Dockerfile](#Dockerfile)
- [Docker_Compose](#Docker_Compose)
- [Docker_Repository](#Docker_Repository)
- [SQL_SERVER_Connection](#SQL_SERVER_Connection)
- [SQL_SERVER_Connection_Issue](#SQL_SERVER_Connection_Issue)
  

# Requirements
.NET 8.0,Microsoft SQL Server 2022 And Docker Desktop

# Short_brief

I containerized a .NET Core Web API and Microsoft SQL Server 2022 using Docker Compose for an efficient, portable development environment. I created Dockerfiles to package both services and configured them to work together within a shared Docker network. Finally, I pushed the built images to a Docker repository for easy deployment and scalability.

# Web_API

I created a simple Web API with two API endpoints. One retrieves all data from the database, and the other seeds (inserts) data.

![swagger-api](https://github.com/user-attachments/assets/01831d8f-3c30-40b9-8d1c-9d4e1cbaa18f)

# Dockerfile
When creating a Web API with 'Enable Container Support' selected, Visual Studio automatically generates a Dockerfile containing the necessary instructions to build your image.

![dockerfileCreation](https://github.com/user-attachments/assets/1d9f0db3-ada6-4b0f-9151-e8b6bf2cc416)

**Dockerfile**
```dockerfile
 # See https://aka.ms/customizecontainer...
# Provides a link to documentation for customizing the container and debugging with Visual Studio.

# This stage is used when running from VS in fast mode...
# Introduces the base stage used for fast debugging (default in Debug configuration).
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Uses the ASP.NET 8.0 runtime image as the base image, labeling this stage as base.

USER app
# Switches the user context to app for better security.

WORKDIR /app
# Sets the working directory inside the container to /app.

EXPOSE 8080
# Exposes port 8080 for HTTP traffic.

EXPOSE 443
# Exposes port 443 for HTTPS traffic.

# This stage is used to build the service project
# Introduces the build stage for compiling the Web API project.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Uses the .NET SDK 8.0 image as the build environment, labeling this stage as build.

ARG BUILD_CONFIGURATION=Release
# Defines a build argument for the configuration, defaulting to Release.

WORKDIR /src
# Sets the working directory to /src in the build container.

COPY ["main/main.csproj", "main/"]
# Copies the project file from the local main folder to the container’s main folder.

RUN dotnet restore "./main/main.csproj"
# Restores NuGet packages for the project.

COPY . .
# Copies the entire source code into the container.

WORKDIR "/src/main"
# Changes the working directory to /src/main where the project is located.

RUN dotnet build "./main.csproj" -c $BUILD_CONFIGURATION -o /app/build
# Builds the project using the specified configuration, outputting binaries to /app/build.

# This stage is used to publish the service project...
# Introduces the publish stage that creates the deployable output.
FROM build AS publish
# Starts a new stage named publish based on the build stage.

ARG BUILD_CONFIGURATION=Release
# Re-declares the build configuration argument for this stage.

RUN dotnet publish "./main.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
# Publishes the project for deployment, outputting to /app/publish and disabling the use of an app host.

# This stage is used in production or when running from VS in regular mode...
# Introduces the final stage for production or standard Visual Studio runs (non-Debug mode).
FROM base AS final
# Begins the final stage using the runtime image from the base stage, labeling it final.

WORKDIR /app
# Sets the working directory in the final container to /app.

COPY --from=publish /app/publish .
# Copies the published output from the publish stage into the final image.

ENTRYPOINT ["dotnet", "main.dll"]
# Specifies the command to run when the container starts (launching the Web API).

```

# Docker_Compose
To add a docker-compose.yml file from Visual Studio, right-click on your Web API project and select Add → Container Orchestrator Support. The default options will be pre-selected in the following steps.

![selection](https://github.com/user-attachments/assets/5b11c2ad-29f8-4153-bec6-1c87014bf07f)
![docker-compose-01](https://github.com/user-attachments/assets/fc6f920f-cdab-43c8-ab66-216697c25d27)

![docker-compose-02](https://github.com/user-attachments/assets/2e346053-fbc8-4b50-99a5-558791e2955b)

**docker-compose.yml**
```yml
version: '3.4'  # Defines the Docker Compose file format version.

services:
  SqlServerDb:  # Defines the SQL Server service.
    container_name: sqlserverdb  # Sets a custom name for the container.
    image: mcr.microsoft.com/mssql/server:2022-latest  # Uses the official SQL Server 2022 image.
    ports:
      - 8002:1433  # Maps port 1433 inside the container to port 8002 on the host.
    environment:
      - ACCEPT_EULA=Y  # Accepts the SQL Server license agreement.
      - MSSQL_SA_PASSWORD=ewww@20230302  # Sets the SA (admin) password for SQL Server.
    volumes:
      - sqlserver_data:/var/opt/mssql  # Creates a persistent volume for storing database files(it works as a backup if we remove container).
    networks:
      - myAppNetwork  # Connects this service to the custom network.

  main:  # Defines the Web API service.
    container_name: webApiContainer  # Sets a custom name for the Web API container.
    image: ${DOCKER_REGISTRY-}dockerintro  # Uses an optional registry prefix and the "dockerintro" image.
    environment:
      - ASPNETCORE_ENVIRONMENT=Development  # Sets the ASP.NET Core environment to Development.
      - ConnectionStrings__DefaultConnection=Server=sqlserverdb,1433; Database=dockerintro; User Id=sa; Password=ewww@20230302; TrustServerCertificate=True;  # Database connection string.
    ports:
      - 8001:8080  # Maps port 8080 inside the container to port 8001 on the host.
    build:
      context: .  # Uses the current directory as the build context.
      dockerfile: ./main/Dockerfile  # Specifies the path to the Dockerfile for building the Web API.
    depends_on:
      - SqlServerDb  # Ensures the database container starts before this service.
    networks:
      - myAppNetwork  # Connects the Web API container to the same network as the database.

networks:
  myAppNetwork:  # Defines a custom network for the services to communicate.

volumes:
  sqlserver_data:  # Defines a named volume to persist SQL Server data.

```
Now in this project directory run this command for Creation of respective docker images and containers
```cmd
docker-compose -f docker-compose.yml up -d
```
then got images and containers

![docker-images](https://github.com/user-attachments/assets/4571a7d6-af27-46d2-b73c-101e05498c6a)

![docker-containers](https://github.com/user-attachments/assets/852ec16e-27aa-4fc7-8c5a-9d58b92bb719)

# Docker_Repository
For Push this image need to follow some steps 

**Step-1: At first login docker desktop**

**Step-2: Now go the windows powershell or cmd**

**Step-3: Run Docker login command for access repository**
```cmd
docker login
```
**Step-4: Create new image tag for push this image(use account name ensuring the image is associated with your account and to avoid naming conflicts.)**

```docker tag <target image name>:<tag> <account name>/<anyname for latest image>:<tag>``` 

My Host machine:
```cmd
docker tag dockerintro:latest sowad/dockerintro:1.0
```

**Step-5: At last push newly tagged image to the repository**

```docker push <newly tagged image name>:<tag>``` 

My Host machine:
```cmd
docker tag dockerintro:latest dockerintro:1.0
```

![dockerRepo](https://github.com/user-attachments/assets/22eacd01-bdca-4e39-98d8-7f5ef8b20024)

For pulling image:

```cmd
docker push <acc_name>/<image_name>:<tagname>
```

# SQL_SERVER_Connection
In this demo mssql server Port Mapping is 8002:1433 where
- **8002 (Host Machine Port):** The port on your local machine where you can access the SQL Server container.  
- **1433 (Container Port):** The default port SQL Server uses inside the container.

For MS SQL Server, we need to define two connection strings: one for use outside Docker and another for use inside Docker.
  
**Outside Docker (Local Development):** Connection Between Host Machine and SQL Server Container.

**In appsettings.json:**

 ```"Server=localhost,<host port>; Database=<databse name>; User Id=sa; Password=<password>; TrustServerCertificate=True;"```
 
 My Host machine:
```json
   "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,8002; Database=dockerintro; User Id=sa; Password=ewww@20230302; TrustServerCertificate=True;"
  }
```
**Inside Docker:** services within the same Docker network communicate via container names.In this way mssql server connection works inside docker

**In docker-compose.yml:**

Server=<mssql_container_name>,1433; Database=<databse_name>; User Id=sa; Password=<password>; TrustServerCertificate=True;

For my setup, I use:
```yml
  environment:
   - ConnectionStrings__DefaultConnection=Server=sqlserverdb,1433; Database=dockerintro; User Id=sa; Password=ewww@20230302; TrustServerCertificate=True;
```

**Mssql Server Connection check wtih container:**

Before testing the connection, first start the MSSQL Server container if it is stopped.

![mssqlServerConnection](https://github.com/user-attachments/assets/6cd1c489-6111-44e6-9535-0e1d4d740549)


# SQL_SERVER_Connection_Issue

If you face tcp or network related issue.open Sql server configuration management.Then enable TCP/IP

![tcpconnection](https://github.com/user-attachments/assets/9c56d1a3-d686-4f02-8b5c-3f08dcba0186)

Then Click windows+R write services.msc 

![msc](https://github.com/user-attachments/assets/b83efcb7-92d2-4815-a429-12ecb605918c)

After this it's better restart your pc.

If this solution does not work, try inspecting the MSSQL Server container to check if there is any issue with the Dockerfile or docker-compose.yml.









