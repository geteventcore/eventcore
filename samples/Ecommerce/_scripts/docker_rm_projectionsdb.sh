echo "Stopping and removing SQL Server (Projections DB) container, if exists."
docker container stop eventcore.samples.ecommerce.projectionsdb
docker container rm eventcore.samples.ecommerce.projectionsdb