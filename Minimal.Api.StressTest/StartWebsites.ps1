docker run -d -p 5020:80 -m "50mb" --cpus ".15" --name "minimal-api-demo" danielfitz1010/minimalapidemo 
docker run -d -p 5021:80 -m "50mb" --cpus ".15" --name "minimal-api-demo-controller" danielfitz1010/minimalapidemocontroller
docker run -d -p 5022:80 -m "50mb" --cpus ".15" --name "minimal-api-demo-mix" danielfitz1010/minimalapidemocontroller