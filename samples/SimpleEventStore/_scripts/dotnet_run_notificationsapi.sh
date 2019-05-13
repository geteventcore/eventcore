ARGS=${1:-"urls=http://localhost:5601;https://localhost:5602 environment=Development"}
dotnet run -p ../EventCore.Samples.SimpleEventStore.NotificationsApi $ARGS