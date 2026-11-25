# Verification

## Unit tests

`dotnet test src/NetCoreMQTTExampleCluster.sln` runs every test project on `net10.0` and `net11.0`:

- `NetCoreMQTTExampleCluster.TopicCheck.Tests`: topic filter matching.
- `NetCoreMQTTExampleCluster.Validation.Tests`: connection, publish, and subscription validation.
- `NetCoreMQTTExampleCluster.Grains.Interfaces.Tests`: Orleans serialization round trips of the types passed to the grains and the broker observers.

## End to end

The brokers validate every connection, subscription, and publish through the Orleans silo, which needs PostgreSQL with TimescaleDB. The brokers forward the accepted messages to each other over MQTT.

1. Start a disposable database, for example `docker run -d --name mqtt-test-db -e POSTGRES_PASSWORD=postgres -p 127.0.0.1:5432:5432 timescale/timescaledb:latest-pg17`.
2. Run `NetCoreMQTTExampleCluster.DatabaseSetup` from its output folder. Without arguments it keeps an existing database and only adds what is missing, `--drop-database` deletes it first. It ends with `Console.ReadKey()`, which throws with redirected input after the setup has finished.
3. Start `NetCoreMQTTExampleCluster.SiloHost`. It joins the membership table about 15 seconds after the gateway port 7102 opens. The dashboard is at `http://localhost:4321/dashboard`.
4. Start two `NetCoreMQTTExampleCluster.Cluster` instances from two copies of the output folder with the `appsettings.json` files from `doc/Example setup/Broker1` and `doc/Example setup/Broker2`. They use the ports 1883 and 8883, 1884 and 8884, and an unencrypted synchronization connection, because the example certificate is self-signed. Each broker logs `Forwarding messages to broker ...` as soon as the other one registers.
5. Test with MQTT clients. The user `Test` with password `test` may publish to `a/b` and `a/d` and subscribe to `d/e` and `d`. The user `mqtt-broker-sync` with password `Test` may use `#` with client identifiers that start with `mqtt-broker-sync`, which makes it a convenient subscriber. Check at least:
   - A message published on one broker arrives exactly once on the other broker and exactly once locally, in both directions.
   - A payload that is not valid UTF-8 (for example with the bytes `FF FE 00 80`) and the MQTT 5 properties (content type, response topic, correlation data, user properties) arrive unchanged.
   - Many QoS 1 messages arrive complete and in order. Each broker logs `Connected to the broker for forwarding.` once per other broker, not once per message.
   - A rejected publish (`Test` on `x/y`) gets a PUBACK with `NotAuthorized` and arrives nowhere. A rejected subscription gets a SUBACK with `NotAuthorized`.
   - `Test` resuming or taking over a session of `mqtt-broker-sync` with the same client identifier can't publish to `x/y`.
   - After 20 seconds, `publishmessage` holds each message once, a binary payload as Base64 with `"IsBase64": true`.
6. Optional failure cases: stop the silo and start it again, the brokers register again within `ServiceDelayInMilliseconds`, and `Test` on a resumed session of `mqtt-broker-sync` is still validated as `Test` by the new client grain. Kill one broker hard and start it again, the other broker replaces the old entry as soon as it registers.

Set `ASPNETCORE_ENVIRONMENT` to a name other than `Development` and `LogFolderPath` to a temporary folder to get log files; `Development` logs to the console only. Stop the services with Ctrl+C so that the brokers disconnect from the repository grain. If a process ignores Ctrl+C, for example because it runs in its own process group, Ctrl+Break stops it the same way. Remove the test container afterwards.
