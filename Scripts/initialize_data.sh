#!/bin/bash

psql -U $POSTGRES_USER -d $POSTGRES_DB -a -f /usr/scripts/initialize_data.sql
