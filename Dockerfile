FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./src .
RUN dotnet restore

# copy everything else and build app
RUN dotnet publish -c release -o /publish --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /publish ./

EXPOSE 5672
EXPOSE 25676

ENTRYPOINT ["dotnet", "biodisel.Web.Server.dll"]