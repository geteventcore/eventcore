ARGS=${1:-"urls=http://localhost:5511;https://localhost:5512 environment=Development"}
echo "Running Public UI project."
dotnet run -p ../EventCore.Samples.Ecommerce.PublicUi $ARGS