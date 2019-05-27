ARGS=${1:-"urls=http://localhost:5501;https://localhost:5502 environment=Development"}
echo "Running Service API project."
dotnet run -p ../EventCore.Samples.Ecommerce.ServiceApi $ARGS