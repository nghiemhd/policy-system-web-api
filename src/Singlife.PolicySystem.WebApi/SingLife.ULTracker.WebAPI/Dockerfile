#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
#Usage: docker build --build-arg publishPath=published_source_path -t image_name .

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update && apt-get install -y lsof

FROM base AS final
WORKDIR /app
ARG publishPath
COPY ${publishPath} .
ENTRYPOINT ["dotnet", "SingLife.ULTracker.WebAPI.dll"]