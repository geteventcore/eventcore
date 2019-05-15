echo "Stopping and removing Event Store container(s), if exists."
docker container stop eventcore.samples.ecommerce.evenstoreregionx
docker container rm eventcore.samples.ecommerce.evenstoreregionx