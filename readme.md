This project acts as a base for learning devops, and is a minimal twitter clone.

To run the project either cd into Chirp Project/src/web and run `dotnet run`, then head to http://localhost:port, to see the project running.

Another way to run the project is through docker.

Start by building the docker image while in Chirp Project directory using: `docker build -t userid/imagename .`.

Then run the image using: `docker run -p 8080:8080 userid/imagename`.

And a third way is to use vagrant to provision a VM on Digital Ocean or locally on your own machine.

Digital ocean:

```
cd "Chirp Project"
vagrant up
```

Then head to Digital Ocean and find the ip of the droplet and head to <droplet-id>:8080, to see the applicaiton running.

Locally:
Rename Vagrantfile => Vagrantfile.remote
Rename Vagrantfile.local => Vagrantfile
Then:

```
cd "Chirp Project"
vagrant up
```

Then head to http://localhost:8080 to see the application running.

Remember that each time you update any docker stuff, remember to write the following to make sure it can run.

docker pull nickychengde/itu-minitwit
