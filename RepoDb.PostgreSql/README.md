[![PostgreSqlBuild](https://img.shields.io/appveyor/ci/mikependon/repodb-xb4rk)](https://ci.appveyor.com/project/mikependon/repodb-xb4rk)
[![Home](https://img.shields.io/badge/home-github-important)](https://github.com/mikependon/RepoDb)
[![Wiki](https://img.shields.io/badge/wiki-information-yellow)](https://github.com/mikependon/RepoDb/wiki)
[![PostgreSqlVersion](https://img.shields.io/nuget/v/RepoDb.PostgreSql)](https://www.nuget.org/packages/RepoDb.PostgreSql)
[![PostgreSqlDL](https://img.shields.io/nuget/dt/repodb.postgresql)](https://www.nuget.org/packages/RepoDb.PostgreSql)
[![PostgreSqlUnitTests](https://img.shields.io/appveyor/tests/mikependon/repodb-a63f5?label=unit%20tests)](https://ci.appveyor.com/project/mikependon/repodb-a63f5/build/tests)
[![PostgreSqlIntegrationTests](https://img.shields.io/appveyor/tests/mikependon/repodb-uf6o7?label=integration%20tests)](https://ci.appveyor.com/project/mikependon/repodb-uf6o7/build/tests)

## RepoDb.PostgreSql - a hybrid .NET ORM library for PostgreSql.

RepoDb is an ORM that bridge the gaps between micro-ORMs and macro-ORMs. It helps the developer to simplify the switch-over of when to use the “basic” and “advance” operations during the development.

## Important Pages

- [GitHub Home Page](https://github.com/mikependon/RepoDb) - to learn more about the core library.
- [Wiki Page](https://github.com/mikependon/RepoDb/wiki) - usabilities, benefits, features, capabilities, learnings, topics and FAQs. 

## Community engagements

- [GitHub](https://github.com/mikependon/RepoDb/issues) - for any issues, requests and problems.
- [StackOverflow](https://stackoverflow.com/questions/tagged/repodb) - for any technical questions.
- [Twitter](https://twitter.com/search?q=%23repodb) - for the latest news.
- [Gitter Chat](https://gitter.im/RepoDb/community) - for direct and live Q&A.

## Dependencies

- [Npgsql](https://www.nuget.org/packages/Npgsql/) - the data provider used for *PostgreSql*.
- [RepoDb](https://www.nuget.org/packages/RepoDb.SqLite/) - the core library of *RepoDb*.

## License

[Apache-2.0](http://apache.org/licenses/LICENSE-2.0.html) - Copyright © 2019 - Michael Camara Pendon

--------

## Installation

At the *Package Manager Console*, write the command below.

```csharp
> Install-Package RepoDb.PostgreSql
```

## Getting Started

First, the bootstrapper must be initialized.

```csharp
RepoDb.PostgreSqlBootstrap.Initialize();
```

**Note:** The call must be done once.

After the bootstrap initialization, any library operation can then be called.

### Query

```csharp
using (var connection = new NpgsqlConnection(ConnectionString))
{
	var customer = connection.Query<Customer>(c => c.Id == 10045);
}
```

### Insert

```csharp
var customer = new Customer
{
	FirstName = "John",
	LastName = "Doe",
	IsActive = true
};
using (var connection = new NpgsqlConnection(ConnectionString))
{
	var id = connection.Insert<Customer>(customer);
}
```

### Update

```csharp
using (var connection = new NpgsqlConnection(ConnectionString))
{
	var customer = connection.Query<Customer>(10045);
	customer.FirstName = "John";
	customer.LastUpdatedUtc = DateTime.UtcNow;
	var affectedRows = connection.Update<Customer>(customer);
}
```

### Delete

```csharp
using (var connection = new NpgsqlConnection(ConnectionString))
{
	var customer = connection.Query<Customer>(10045);
	var deletedCount = connection.Delete<Customer>(customer);
}
```

To learn more, please visit our [reference implementations](https://github.com/mikependon/RepoDb/blob/master/RepoDb.Docs/Reference%20Implementations.md) page.
