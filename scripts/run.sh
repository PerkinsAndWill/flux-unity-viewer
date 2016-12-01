sudo docker run -ti \
  -p $4:80 \
  --link mongo:mongo \
  --link neo4j:neo4j \
  --env NODE_ENV=$3 \
  --name $2 \
  $1/$2
