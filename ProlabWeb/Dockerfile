# Étape 1 : Base ASP.NET avec dépendances pour System.Drawing
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Dépendances nécessaires à System.Drawing (GDI+) et aux polices
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    libxrender1 \
    libxext6 \
    fontconfig \
    fonts-dejavu-core \
    fonts-liberation \
    fonts-freefont-ttf \
    && rm -rf /var/lib/apt/lists/*

# Activer la prise en charge complète de System.Drawing
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false


# Étape 2 : Build avec le SDK .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# Étape 3 : Runtime final avec fonts visibles pour SkiaSharp
FROM base AS final
WORKDIR /app

# Copier l'app publiée
COPY --from=build /app/out ./

# Copier les polices dans le conteneur final
COPY wwwroot/fonts/arial /usr/share/fonts/truetype/arial
COPY wwwroot/fonts/himalaya /usr/share/fonts/truetype/himalaya
COPY wwwroot/fonts/timesnewroman /usr/share/fonts/truetype/timesnewroman

# Donner les droits et reconstruire le cache des polices (nécessaire pour SkiaSharp)
RUN chmod -R 644 /usr/share/fonts/truetype && \
    fc-cache -f -v

# Démarrage de l'application
ENTRYPOINT ["dotnet", "ProlabWeb.dll"]
