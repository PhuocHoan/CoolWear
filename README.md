# CoolWear

## How to run the project

### Create and connect Postgres database using Docker

#### Pull the Postgres image from Docker Hub

```bash
docker pull postgres
```

#### Check the image

```bash
docker images
```

#### Create a new container from the Postgres image

```bash
docker run -d --name db -e POSTGRES_PASSWORD=1234 -p 5432:5432 postgres
```

#### Check the container

```bash
docker ps -a
```

#### Start the container

```bash
docker start db
```

#### Create a new database

Step by step run the following commands in the terminal:

```bash
docker exec -it db /bin/bash
psql -U postgres # connect to postgres database
create database MyShop; # create a new database named MyShop
\q # exit psql
exit # exit container
```

#### Connect to the database using any tool like pgAdmin, etc.

Configuration:

- Host: localhost (127.0.0.1)
- Port: 5432
- Database: MyShop
- User: postgres
- Password: 1234

#### Run `CoolWear.sql` to create tables and insert data

### Enjoy the project
