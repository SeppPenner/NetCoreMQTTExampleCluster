# Verification

## Unit tests

`dotnet test src/NetCoreMQTTExampleCluster.sln` runs every test project on `net10.0` and `net11.0`:

- `NetCoreMQTTExampleCluster.TopicCheck.Tests`: topic filter matching.
- `NetCoreMQTTExampleCluster.Validation.Tests`: connection, publish, and subscription validation.
- `NetCoreMQTTExampleCluster.Grains.Interfaces.Tests`: Orleans serialization round trips of the types passed to the grains.

## End to end

The brokers validate every connection, subscription, and publish through the Orleans silo, which needs PostgreSQL with TimescaleDB.

1. Start a disposable database, for example `docker run -d --name mqtt-test-db -e POSTGRES_PASSWORD=postgres -p 127.0.0.1:5432:5432 timescale/timescaledb:latest-pg17`.
2. Run `NetCoreMQTTExampleCluster.DatabaseSetup` from its output folder. It ends with `Console.ReadKey()`, which throws with redirected input after the setup has finished.
3. Start `NetCoreMQTTExampleCluster.SiloHost`. It joins the membership table about 15 seconds after the gateway port 7102 opens. The dashboard is at `http://localhost:4321/dashboard`.
4. Start two `NetCoreMQTTExampleCluster.Cluster` instances from copies of the output folder with different `Port` and `UnencryptedPort` values, `BrokerConnectionSettings.ClientId` values that start with `mqtt-broker-sync`, and different web ports through `ASPNETCORE_URLS`. Point `BrokerConnectionSettings` at the unencrypted port with `UseTls: false`, because the example certificate is self-signed.
5. Test with MQTT clients. The user `Test` with password `test` may publish to `a/b` and `a/d` and subscribe to `d/e` and `d`. The user `mqtt-broker-sync` with password `Test` may use `#` with client identifiers that start with `mqtt-broker-sync`. A message published on one broker must arrive on the other.

Set `ASPNETCORE_ENVIRONMENT` to a name other than `Development` and `LogFolderPath` to a temporary folder to get log files; `Development` logs to the console only. Stop the services with Ctrl+C so that the brokers disconnect from the repository grain, and remove the test container afterwards.
