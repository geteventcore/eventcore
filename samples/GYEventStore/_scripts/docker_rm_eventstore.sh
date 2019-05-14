echo "Stopping and removing Event Store container, if exists."
docker container stop eventcore.samples.gyeventstore.regionx
docker container rm eventcore.samples.gyeventstore.regionx