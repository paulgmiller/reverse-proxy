FROM  mcr.microsoft.com/dotnet/core/sdk:5.0
#COPY *.csproj  .
#RUN dotnet restore
WORKDIR  /src
COPY ./ ./
#Don't be a chump do a two stage build 
WORKDIR  /src/samples/Ingress.Sample
RUN ls -A .
RUN dotnet -d run 