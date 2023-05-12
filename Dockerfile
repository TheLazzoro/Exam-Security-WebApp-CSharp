#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Exam-Security-WebApp-CSharp/Exam-Security-WebApp-CSharp.csproj", "Exam-Security-WebApp-CSharp/"]
RUN dotnet restore "Exam-Security-WebApp-CSharp/Exam-Security-WebApp-CSharp.csproj"
COPY . .
WORKDIR "/src/Exam-Security-WebApp-CSharp"
RUN dotnet build "Exam-Security-WebApp-CSharp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Exam-Security-WebApp-CSharp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Exam-Security-WebApp-CSharp.dll"]