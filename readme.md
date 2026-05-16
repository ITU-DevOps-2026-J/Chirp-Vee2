# Chirp Vee2

## Monitoring demonstration
![Monitoring-video-demonstration](Monitoring.gif)

## Logging demonstration
![Logging-video-demonstration](Logging.gif)

## IAC demonstration
![IAC-video-demonstration](IAC.gif)

## Description

This project acts as a base for learning devops, and is a minimal twitter clone.

## How to install

Once the repo is downloaded onto your computer, here is a few stuff you need to do to have the project up and running.

Installing Docker:

To install docker, here is what you need to run on your computer.

`sudo apt`
`sudo apt install docker-ce`
## Prerequisites
The following is a list of things expected to be installed (and setup) such that you can run this project on your machine:
* `Dotnet 10`
* `Docker`
* `Vagrant`
  - `vagrant-digitalocean`
  - `vagrant-scp`
  - `vagrant-reload`

## How to run the program locally

To run the project either cd into Chirp Project/src/web and run `dotnet run`, then head to http://localhost:port, to see the project running.

Another way to run the project is through docker.

Start by building the docker image while in Chirp Project directory using: `docker build -t userid/imagename .`.

Then run the image using: `docker run -p 8080:8080 userid/imagename`.

## How to set up the pipeline

You can run the project locally or online with the digital ocean droplet. run the following commands to have it up and running.

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


