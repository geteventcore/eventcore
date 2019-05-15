echo "Stopping and removing SQL Server (Agg Root States DB) container, if exists."
docker container stop eventcore.samples.ecommerce.aggrootstatesdb
docker container rm eventcore.samples.ecommerce.aggrootstatesdb