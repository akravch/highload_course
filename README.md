# Running the application

## TLDR

Run the following command in terminal:

```bash
cd Src && \
export DB_USER=postgres DB_PASSWORD=password AUTH_KEY=1234567890 && \
docker compose up
```

The service should start on `http://localhost:5000`.

## Step by step

1. Set the following environment variables:
   - `DB_USER` - username for PostgreSQL
   - `DB_PASSWORD` - password for PostgreSQL
   - `AUTH_KEY` - a string key used to generate authentication tokens

   You can either directly set them in terminal:

   ```bash
   export DB_USER=postgres
   export DB_PASSWORD=password
   export AUTH_KEY=1234567890
   ```

   or by creating a text file `.env` in `Src` with the following content:

   ```
   DB_USER=postgres
   DB_PASSWORD=password
   AUTH_KEY=1234567890
   ```

2. Run `docker compose up` in `Src`.
3. The API should now be available on `http://localhost:5000`.
