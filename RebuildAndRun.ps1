docker stop bot
docker rm bot
docker rmi bot_image
docker build -t bot_image -f Dockerfile .
docker create --name bot --interactive bot_image
docker start bot
docker ps
docker logs bot