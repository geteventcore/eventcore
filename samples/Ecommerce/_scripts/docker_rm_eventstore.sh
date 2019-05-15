echo "Stopping and removing Event Store container, if exists."
docker container stop eventcore.samples.ecommerce.evenstoreregionx
docker container rm eventcore.samples.ecommerce.evenstoreregionx