TRUNCATE account RESTART IDENTITY CASCADE;
TRUNCATE account_info RESTART IDENTITY CASCADE;

INSERT INTO account (hash, salt)
SELECT md5(random()::text), md5(random()::text)
FROM generate_series(1, 1000000);

COPY account_info (account_id, first_name, second_name, biography, city, birthdate)
FROM '/usr/scripts/data.csv' DELIMITER ',';

--INSERT INTO account_info (account_id, first_name, second_name, biography, city, birthdate)
--SELECT id, md5(random()::text), md5(random()::text), md5(random()::text), md5(random()::text), NOW() - (random() * (INTERVAL '90 years'))
--FROM generate_series(1, 10) AS id;
