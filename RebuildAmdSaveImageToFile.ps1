docker stop bot
docker rm bot
docker rmi bot_image
docker build -t bot_image -f Dockerfile .
docker create --name bot --interactive bot_image
docker save -o ./bot_image_file bot_image
docker rm bot
docker rmi bot_image