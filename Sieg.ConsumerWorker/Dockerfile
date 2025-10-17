# ----------------------------------------------------------------
# ETAPA 1: BUILD
# Usa o SDK para compilar e publicar o projeto.
# ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# 1. Copia o arquivo de solução (.sln) e todos os arquivos .csproj
# Isso permite que o 'dotnet restore' resolva as dependências entre projetos.
COPY *.sln ./
COPY ./Sieg.Application/Sieg.Application.csproj ./Sieg.Application/
COPY ./Sieg.Domain/Sieg.Domain.csproj ./Sieg.Domain/
COPY ./Sieg.Infrastructure/Sieg.Infrastructure.csproj ./Sieg.Infrastructure/
COPY ./Sieg.IoC/Sieg.IoC.csproj ./Sieg.IoC/
COPY ./Sieg.ConsumerWorker/Sieg.ConsumerWorker.csproj ./Sieg.ConsumerWorker/

# 2. Restaura as dependências.
# Target o projeto principal (ConsumerWorker) para restaurar a solução completa.
RUN dotnet restore ./Sieg.ConsumerWorker/Sieg.ConsumerWorker.csproj

# 3. Copia todo o restante do código-fonte
COPY . ./

# 4. Publica o projeto ConsumerWorker
WORKDIR /app/Sieg.ConsumerWorker
RUN dotnet publish -c Release -o /publish /p:UseAppHost=false


# ----------------------------------------------------------------
# ETAPA 2: RUNTIME
# Usa a imagem base mais leve (runtime) para o serviço de console/worker.
# ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
# Copia os arquivos publicados da etapa de build
COPY --from=build /publish .

# Define o ponto de entrada para iniciar o Worker Service
ENTRYPOINT ["dotnet", "Sieg.ConsumerWorker.dll"]
