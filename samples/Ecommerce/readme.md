
Ports...

2113 - EventStore Web Admin
1113 - EventStore TCP
5633 - Aggregate Root States DB
5733 - Projections DB
5501, 5502 - Domain API


Getting started...

cd /_scripts
sh reset_infrastructure.sh
sh run_infrastructure.sh
... Wait 30 seconds to make sure containers have started.
sh initialize_infrastructure.sh