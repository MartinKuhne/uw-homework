(Get-ECRLoginCommand -Region us-west-2 -ProfileName uw-homework).Password | docker login --username AWS --password-stdin 489734993754.dkr.ecr.us-west-2.amazonaws.com
& docker build -t uw-homework -f .\Product\Dockerfile --progress plain .
& docker tag uw-homework:latest 489734993754.dkr.ecr.us-west-2.amazonaws.com/uw-homework:latest
& docker push 489734993754.dkr.ecr.us-west-2.amazonaws.com/uw-homework:latest