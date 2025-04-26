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
create database coolwear; # create a new database named coolwear
\q # exit psql
exit # exit container
```

#### Connect to the database using any tool like pgAdmin, etc.

Configuration:

- Host: localhost (127.0.0.1)
- Port: 5432
- Database: coolwear
- User: postgres
- Password: 1234

#### Run `CoolWear.sql` to create tables and insert data

#### Add `connectionString` to `secrets.json`

- Open file `secrets.json` explorer with path: `%APPDATA%\Microsoft\UserSecrets\12608b1b-0e4c-4f1f-a102-496fd5390d44\secrets.json`
- Add the following line to `secrets.json`:

```json
{
  "ConnectionStrings": {
    "PostgresDatabase": "Host=localhost;Database=coolwear;Username=postgres;Password=1234"
  }
}
```

### Run the project

#### Create new account for store owner

- **username**: admin
- **password**: 123 (default password, _not hashed_)

Or you can use your wanted password by doing this: (_hashed_ password)

- Type username, password you want, click "Remember me" and click "Đăng nhập"
  => It will display "Không thể đăng nhập"
- Close the app, then run it again
- Type username, password you just typed, click "Đăng nhập"
  => You can login successfully

This function is because this app only have 1 user that is store owner, so the app should not have Register function. But if user want to use their wanting password (hashed), they could do it by this way.