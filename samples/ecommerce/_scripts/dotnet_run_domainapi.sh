ARGS=${1:-"urls=http://localhost:5501;https://localhost:5502 environment=Development"}
dotnet run -p ../EventCore.Samples.Ecommerce.DomainApi $ARGS