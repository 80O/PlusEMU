Run the unit tests with:

```powershell
dotnet test Tests/Plus.Trading.Tests/Plus.Trading.Tests.csproj -p:OS=Unix
```

`OS=Unix` disables the emulator's Windows post-build action that writes a machine environment variable. It does not
change the target platform.

The persistence tests run against a real MySQL/MariaDB server. Set `PLUS_TRADE_TEST_MYSQL` to a connection string for an
isolated test server with CREATE/DROP DATABASE permissions. Each test creates and removes its own randomly named
`plus_trade_test_*` database. Without that variable, database tests are reported as skipped.

For example, start a disposable database container, set the variable, and run the same test command:

```powershell
docker run --detach --name plus-trade-tests --publish 127.0.0.1:33316:3306 --env MARIADB_ROOT_PASSWORD=trade-test-only mariadb:11.4
$env:PLUS_TRADE_TEST_MYSQL = 'Server=127.0.0.1;Port=33316;User ID=root;Password=trade-test-only;SslMode=None'
dotnet test Tests/Plus.Trading.Tests/Plus.Trading.Tests.csproj -p:OS=Unix
docker rm --force plus-trade-tests
```

Wait for the database container to finish starting before running the tests.

Trading requires `items`, `users`, and `logs_client_trade` to use InnoDB. The implementation checks this before making
changes. Review and migrate any legacy MyISAM tables before enabling trading; no automatic schema migration is
performed.
