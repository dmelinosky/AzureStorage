# Gobie74.AzureStorage
The purpose of this library was to make storing data in Azure Table Storage just a bit easier.  Making this available as a package on Nuget.org because I was getting tired of copying the source around and wanted to try out some build pipelines and other tech.

## Using

The library targets .NET 10 and is built on [Azure.Data.Tables](https://learn.microsoft.com/dotnet/api/overview/azure/data.tables-readme).

The data types that this library stores must implement [Azure.Data.Tables.ITableEntity](https://learn.microsoft.com/dotnet/api/azure.data.tables.itableentity).

> **Upgrading from 1.x:** version 2.0 replaces the deprecated `Microsoft.Azure.Cosmos.Table` package. Entities now implement `Azure.Data.Tables.ITableEntity` (which adds `Timestamp` and `ETag` properties), and the constructor that took a `CloudTable` now takes a `TableClient`.

### Construction

Once you have a class you want to store, create a `TableAccess<T>` object.  The constructor takes either a connection string and a table name, or an existing `Azure.Data.Tables.TableClient`, plus an optional logger.

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

---

## Status

[![Build](https://github.com/dmelinosky/AzureStorage/actions/workflows/build.yml/badge.svg)](https://github.com/dmelinosky/AzureStorage/actions/workflows/build.yml)

[![Build status](https://dev.azure.com/gobie74/github/_apis/build/status/dmelinosky.AzureStorage)](https://dev.azure.com/gobie74/github/_build/latest?definitionId=15)

[![gobie74.azurestorage package in packageSource feed in Azure Artifacts](https://feeds.dev.azure.com/gobie74/_apis/public/Packaging/Feeds/8bd63402-db7d-41c2-ad0f-21c6b6106e0b/Packages/0b0cfcf4-272c-40dd-8398-898da22b1085/Badge)](https://dev.azure.com/gobie74/github/_packaging?_a=package&feed=8bd63402-db7d-41c2-ad0f-21c6b6106e0b&package=0b0cfcf4-272c-40dd-8398-898da22b1085&preferRelease=true)

[![nuget](https://img.shields.io/nuget/v/Gobie74.AzureStorage.svg?logo=nuget)](https://www.nuget.org/packages/gobie74.azurestorage/)

