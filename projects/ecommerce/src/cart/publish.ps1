(Get-ECRLoginCommand -Region us-west-2 -ProfileName uw-homework).Password | docker login --username AWS --password-stdin 489734993754.dkr.ecr.us-west-2.amazonaws.com
# docker build -t th-ecom-cart:latest -f .\cart\Dockerfile --progress plain --no-cache .
& docker build -t tidyhaus/cart -f .\cart\Dockerfile --progress plain .
& docker tag tidyhaus/cart:latest 489734993754.dkr.ecr.us-west-2.amazonaws.com/tidyhaus/cart:latest
& docker push 489734993754.dkr.ecr.us-west-2.amazonaws.com/tidyhaus/cart:latest
