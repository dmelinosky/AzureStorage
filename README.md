# Gobie74.AzureStorage
The purpose of this library was to make storing data in Azure Table Storage just a bit easier.  Making this available as a package on Nuget.org because I was getting tired of copying the source around and wanted to try out some build pipelines and other tech.

## Using

The library targets .NET 10 and is built on [Azure.Data.Tables](https://learn.microsoft.com/dotnet/api/overview/azure/data.tables-readme).

The data types that this library stores must implement [Azure.Data.Tables.ITableEntity](https://learn.microsoft.com/dotnet/api/azure.data.tables.itableentity).

> **Upgrading from 1.x:** version 2.0 replaces the deprecated `Microsoft.Azure.Cosmos.Table` package. Entities now implement `Azure.Data.Tables.ITableEntity` (which adds `Timestamp` and `ETag` properties), and the constructor that took a `CloudTable` now takes a `TableClient`.

### Construction

Once you have a class you want to store, create a `TableAccess<T>` object.  The constructor takes one of the following, plus an optional logger:

- a connection string and a table name,
- an existing `Azure.Data.Tables.TableClient`, or
- an `Azure.Data.Tables.TableServiceClient` and a table name.

The connection string can be found in the Azure Portal or in Azure Storage Explorer, and possibly other places.

The table name is any non-empty string you want.

The logger interface is a `Microsoft.Extensions.Logging.Abstractions` logger with the category of `TableAccess<T>` and is optional.

#### Example
Using dependency injection during ConfigureServices might look something like this:

```c#
            services.AddTransient<ITableAccess<Thing>, TableAccess<Thing>>((ctx) =>
            {
                var logger = ctx.GetService<ILogger<TableAccess<Thing>>>();

                return new TableAccess<Thing>(connection, "test", logger);
            });
```

#### Example with Microsoft.Extensions.Azure

If you register Azure clients with `AddAzureClients` from [Microsoft.Extensions.Azure](https://learn.microsoft.com/dotnet/azure/sdk/dependency-injection), pass the registered `TableServiceClient` and a table name. This library doesn't depend on Microsoft.Extensions.Azure itself.

```c#
            services.AddAzureClients(clients =>
            {
                clients.AddTableServiceClient(connection);
                clients.AddTableServiceClient(otherConnection).WithName("other");
            });

            // Default client
            services.AddTransient<ITableAccess<Thing>>(ctx =>
                new TableAccess<Thing>(
                    ctx.GetRequiredService<TableServiceClient>(),
                    "things",
                    ctx.GetService<ILogger<TableAccess<Thing>>>()));

            // Named client via the factory
            services.AddTransient<ITableAccess<OtherThing>>(ctx =>
                new TableAccess<OtherThing>(
                    ctx.GetRequiredService<IAzureClientFactory<TableServiceClient>>().CreateClient("other"),
                    "otherthings",
                    ctx.GetService<ILogger<TableAccess<OtherThing>>>()));
```

### Adding Data

Add data to the table by using the `AddAsync` method.

#### Example

```c#
            Thing aThing = new Thing { PartitionKey = "things", RowKey = "1", Name = "Hello" };

            await tableAccess.AddAsync(aThing);
```

### Updating Data

Update an entity's properties and then call the `InsertOrReplaceAsync` method.

#### Example

```c#
            aThing.Name = "Hello World";

            await tableAccess.InsertOrReplaceAsync(aThing);
```

### Deleting Data

To delete an entity from the table, call the `DeleteAsync` method.

#### Example

```c#
            await tableAccess.DeleteAsync(readThing);
```

### Query Data

The library supports `GetSingleAsync` (partition key + row key), `FindAllByPartitionKey`, `FindAllByRowKey`, and `FindFirstRowWithProperty` (row key + a property name/value).

To Query for an entity, supply the partition key and row key to the `GetSingleAsync` method.

#### Example

```C#
            var readThing = await tableAccess.GetSingleAsync("things", "1");
```

---

## Building and testing

```sh
dotnet build AzureStorage.slnx
dotnet test --solution AzureStorage.slnx
```

The tests are integration tests written with [TUnit](https://tunit.dev). They use, in order of preference, a `StorageConnectionString` setting, `AccountName` + `AccountKey` user secrets, or the local [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) emulator:

```sh
docker run -p 10002:10002 mcr.microsoft.com/azure-storage/azurite azurite-table --tableHost 0.0.0.0
```

## Releasing

Pushes to `master` (merged PRs) are published to [nuget.org](https://www.nuget.org/packages/gobie74.azurestorage/) by the `publish` job in `.github/workflows/build.yml`, using NuGet Trusted Publishing (no API key is stored in GitHub). To release, bump `<Version>` and `<PackageReleaseNotes>` in `src/Gobie74.AzureStorage.csproj` in your PR; merges that don't change the version are skipped. Each published version also gets a `v<version>` tag and a GitHub release with the package attached.

---

## Status

[![Build](https://github.com/dmelinosky/AzureStorage/actions/workflows/build.yml/badge.svg)](https://github.com/dmelinosky/AzureStorage/actions/workflows/build.yml)

[![nuget](https://img.shields.io/nuget/v/Gobie74.AzureStorage.svg?logo=nuget)](https://www.nuget.org/packages/gobie74.azurestorage/)

