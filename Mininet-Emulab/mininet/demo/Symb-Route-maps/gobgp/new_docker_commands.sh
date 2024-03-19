docker exec -it gobgp_1 gobgp policy statement statement1 del action reject
docker exec -it gobgp_1 gobgp policy statement statement1 add action accept
docker exec -it gobgp_1 gobgp neighbor 3.0.0.3 softreset
sleep 10
docker exec -it gobgp_1 gobgp global rib > out.txt
docker-compose down
rm -rf gobgp1/gobgp.yml