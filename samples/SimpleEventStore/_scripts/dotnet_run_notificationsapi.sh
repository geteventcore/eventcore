ARGS=${1:-"urls=http://localhost:5701;https://localhost:5702 environment=Development"}
dotnet run -p ../EventCore.Samples.SimpleEventStore.NotificationsApi $ARGS