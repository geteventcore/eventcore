docker run --name eventcore.samples.simpleeventstore.eventstoredb -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=passw0rd!' -e 'MSSQL_PID=Express' -p 5733:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu