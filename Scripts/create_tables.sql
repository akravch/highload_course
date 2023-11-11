CREATE TABLE IF NOT EXISTS account (
  id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  hash CHAR(128) NOT NULL,
  salt CHAR(64) NOT NULL
);

CREATE TABLE IF NOT EXISTS account_info (
  id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
  account_id BIGINT NOT NULL,
  first_name VARCHAR(128) NOT NULL,
  second_name VARCHAR(128) NOT NULL,
  biography VARCHAR(2048) NOT NULL,
  city VARCHAR(128) NOT NULL,
  birthdate DATE NOT NULL,
  CONSTRAINT fk_account
    FOREIGN KEY(account_id) REFERENCES account(id)
);