# Replication report

### 1. Setup common network in `docker-compose.yml`

```yml
networks:
  db_network:
    name: db_network
    driver: bridge
```

### 2. Setup async replication on master

#### 1. Change `/var/lib/postgresql/data/postgresql.conf/postgresql.conf` on `highload_course_db_1`

```
ssl = off
wal_level = replica
max_wal_senders = 4
```

#### 2. Create a user for replication

```sh
docker exec -it highload_course_db_1 su - postgres -c psql
create role replicator with login replication password 'pass';
exit
```

#### 3. In `/var/lib/postgresql/data/pg_hba.conf` configure network

```sh
docker network inspect db_network | grep Subnet
```

Add the result to `/var/lib/postgresql/data/pg_hba.conf`:

```
host    replication     replicator      172.22.0.0/16           md5
```

#### 4. Restart master

```sh
docker restart highload_course_db_1
```

#### 5. Back-up the data for the replicas

```sh
docker exec -it highload_course_db_1 bash
mkdir /backup
pg_basebackup -h highload_course_db_1 -D /backup -U replicator -v -P --wal-method=stream
exit
```

### 3. Setup replication on the slave

#### 1. Copy the backup to the slave

```sh
docker cp highload_course_db_1:/backup/. ~/postgresql/highload_course_db_2/
```

#### 2. Create a signal file

```sh
touch ~/postgresql/highload_course_db_2/standby.signal
```

#### 3. Change `/var/lib/postgresql/data/postgresql.conf/postgresql.conf` on `highload_course_db_2`

```
primary_conninfo = 'host=highload_course_db_1 port=5432 user=replicator password=pass application_name=highload_course_db_2'
```

#### 9. Restart the container

```sh
docker restart highload_course_db_2
```

### 4. Compare the load on master with and without the replica

| Container                          | CPU     | MEM USAGE / LIMIT   | MEM % | NET I/O         | BLOCK I/O | PIDS |
|------------------------------------|---------|---------------------|-------|-----------------|-----------|------|
| Without replica (read from master) | 950.10% | 507MiB / 14.64GiB   | 3.38% | 13.6MB / 15.2MB | 0B / 0B   | 52   |
| With replica (read from replica)   | 0.00%   | 148.3MiB / 14.64GiB | 0.99% | 14.8kB / 15.1MB | 0B / 0B   | 7    |

### 5. Setup synchronous replication

#### 1. Add another replica into docker-compose

```yml
db_3:
  image: postgres
  container_name: highload_course_db_3
  hostname: highload_course_db_3
  environment:
    - POSTGRES_USER=${DB_USER}
    - POSTGRES_PASSWORD=${DB_PASSWORD}
  ports:
    - "35432:5432"
  volumes:
    - ~/postgresql/highload_course_db_3:/var/lib/postgresql/data
  networks:
    - db_network
  depends_on:
    - db_1
```

#### 2. Backup and restore `highload_course_db_3` same as done for `highload_course_db_2`

#### 3. Setup synchronous replication on `highload_course_db_1`

Change `/var/lib/postgresql/data/postgresql.conf/postgresql.conf`:

```
synchronous_commit = on
synchronous_standby_names = 'highload_course_db_2,highload_course_db_3'
```

#### 4. Restart containers

```sh
docker restart highload_course_db_1
docker restart highload_course_db_3
```

### 6. Promote new master

#### 1. Start write load on `highload_course_db_1`

```sh
while true; do
    psql -U postgres -d highload_course -c "INSERT INTO account (hash, salt) VALUES ('hash', 'salt');"
done;
```

#### 2. Kill the process on master

```sh
docker exec -it highload_course_db_1 pkill -f postgres
```

#### 3. Promote `highload_course_db_2` to master

```sh
docker exec -it highload_course_db_2 su - postgres -c psql
select pg_promote();
exit;
```

#### 4. Switch `highload_course_db_3` to the new master

Change `/var/lib/postgresql/data/postgresql.conf/postgresql.conf` on `highload_course_db_3`

```
primary_conninfo = 'host=highload_course_db_2 port=5432 user=replicator password=pass application_name=highload_course_db_3'
```

And restart

```sh
docker restart highload_course_db_3
```

#### 5. Check that the new master is replicating to `highload_course_db_3`

```sh
docker exec -it highload_course_db_2 su - postgres -c psql
select application_name, sync_state from pg_stat_replication;
exit;
```

```
   application_name   | sync_state
----------------------+------------
 highload_course_db_3 | async
```

#### 6. Compare the number of rows on `highload_course_db_1`, `highload_course_db_2` and `highload_course_db_3`

The number of inserted rows in `account` is the same across all instances, nothing has been lost. Looks like synchronous replication makes sure of it.
