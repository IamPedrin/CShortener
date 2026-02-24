FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CShortener.csproj", "./"]
RUN dotnet restore "CShortener.csproj"

COPY . .
RUN dotnet publish "CShortener.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT [ "dotnet", "CShortener.dll" ]