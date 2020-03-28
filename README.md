# Postgres Pigeon

This is a throw-away project to understand how the ADO.NET connector for PostgreSQL in [npgsql](https://github.com/npgsql/npgsql) works by re-implementing from scratch in order to run a basic select query.

## Walkthrough

At the moment this project only implements the absolute bare minimum necessary to connect to the database server and send messages. The code performs the following steps:

- User provides a connection string which should be validated which we store in a connection instance.
- User calls open and we create a connector and set our connection state to opening. The connector performs a DNS lookup for the `host` entry in the connection string. For each IP which is returned for the DNS lookup we try and open a TCP (IPV4) or IP (IPV6) socket connection to the corresponding port.
- Once we open the connection we send a `StartupMessage` to the server as defined in the [Postgres protocol specification](https://www.postgresql.org/docs/current/protocol-message-formats.html) then read the responses over the network stream and send any authentication details required to complete the connection (plain-text passwords and trust (no password) only supported at the moment).
- Once we receive the `ReadyForQuery` message we set the connection state to open and the user can then start running queries.

## Project Status

Throw-away / unmaintained.
