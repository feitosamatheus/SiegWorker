FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ./*.sln ./
COPY ./Sieg.Application/Sieg.Application.csproj ./Sieg.Application/
COPY ./Sieg.Domain/Sieg.Domain.csproj ./Sieg.Domain/
COPY ./Sieg.Infrastructure/Sieg.Infrastructure.csproj ./Sieg.Infrastructure/
COPY ./Sieg.IoC/Sieg.IoC.csproj ./Sieg.IoC/
COPY ./Sieg.ConsumerWorker/Sieg.ConsumerWorker.csproj ./Sieg.ConsumerWorker/

RUN dotnet restore ./Sieg.ConsumerWorker/Sieg.ConsumerWorker.csproj

COPY . ./
WORKDIR /app/Sieg.ConsumerWorker
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Sieg.ConsumerWorker.dll"]
