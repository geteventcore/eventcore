. docker-reset-eventstore.sh
. docker-run-eventstore.sh

echo "Waiting 10 seconds, then creating projection(s)..."
sleep 10s
curl -i --data "@all_non_system.json" http://localhost:2113/projections/continuous?name=all_non_system%26type=js%26enabled=true%26emit=true%26trackemittedstreams=false -u admin:changeit