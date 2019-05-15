echo "Running Event Store container(s)."
docker run --name eventcore.samples.ecommerce.evenstoreregionx -d -p 2113:2113 -p 1113:1113 eventstore/eventstore \
	-e EVENTSTORE_RUN_PROJECTIONS=All -e EVENTSTORE_START_STANDARD_PROJECTIONS=True