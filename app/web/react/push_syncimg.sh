aws s3 sync images s3://mm-front-public/images


#docker build -f app/web/react/Dockerfile -t front-portal --build-arg PROJECT_NAME=tenant app/web/react/
#docker tag front-portal:latest 961341529237.dkr.ecr.ap-southeast-2.amazonaws.com/front-portal:latest
#docker push 961341529237.dkr.ecr.ap-southeast-2.amazonaws.com/front-portal:latest
#kubectl rollout restart deployment front-portal