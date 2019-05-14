docker run --name eventcore.samples.gyeventstore.streamclient -d -p 2113:2113 -p 1113:1113 eventstore/eventstore \
	-e EVENTSTORE_RUN_PROJECTIONS=All -e EVENTSTORE_START_STANDARD_PROJECTIONS=true

echo "Running."