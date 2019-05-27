
Ports...

2113 - Event Store Web Admin
1113 - Event Store TCP
5633 - Aggregate Root States DB
5733 - Projections DB
5501, 5502 - Service API


Getting started...

cd /_scripts
sh reset_infrastructure.sh
sh run_infrastructure.sh
... Wait 30 seconds to make sure containers have started.
sh initialize_infrastructure.sh
sh run_applications.sh

Monitoring events in real time...
sh listen_all_events.sh