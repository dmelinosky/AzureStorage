# Gobie74.AzureStorage
The purpose of this library was to make storing data in Azure Table Storage just a bit easier.  Making this available as a package on Nuget.org because I was getting tired of copying the source around and wanted to try out some build pipelines and other tech.

## Using

The data types that this library stores must be derived from [Microsoft.Azure.Cosmos.Table.ITableEntity](https://docs.microsoft.com/en-us/azure/cosmos-db/tutorial-develop-table-dotnet#define-the-entity).

### Construction

Once you have a class you want to store, create a `TableAccess<T>` object.  Currently the constructor takes a connection string, a table name, and optionally a logger interface.

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

Currently the library supports querying data by the PartitionKey and RowKey only.  Query by property is in development.

To Query for an entity, supply the partition key and row key to the `GetSingleAsync` method.

#### Example

```C#
            var readThing = await tableAccess.GetSingleAsync("things", "1");
```

---

## Status

[![Build status](https://dev.azure.com/gobie74/github/_apis/build/status/dmelinosky.AzureStorage)](https://dev.azure.com/gobie74/github/_build/latest?definitionId=15)