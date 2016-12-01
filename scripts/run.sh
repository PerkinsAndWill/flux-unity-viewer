sudo docker run -ti \
  -p $4:8091\
  --env NODE_ENV=$3 \
  --name $2 \
  $1/$2
