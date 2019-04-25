ARGS=${1:-"urls=http://localhost:8081;https://localhost:8082 environment=Development"}
dotnet run -p ../samples/EventCore.Samples.Ecommerce.DomainApi $ARGS