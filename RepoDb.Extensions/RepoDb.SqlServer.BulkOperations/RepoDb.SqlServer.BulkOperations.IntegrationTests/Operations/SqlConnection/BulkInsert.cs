﻿using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepoDb.Extensions;
using RepoDb.IntegrationTests.Setup;
using RepoDb.SqlServer.BulkOperations.IntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace RepoDb.SqlServer.BulkOperations.IntegrationTests.Operations
{
    [TestClass]
    public class SqlConnectionOperationsTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Database.Initialize();
            Cleanup();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Database.Cleanup();
        }

        #region BulkInsert<TEntity>

        [TestMethod]
        public void TestSqlConnectionBulkInsertForEntities()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsert(tables);

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForEntitiesWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsert(tables);

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForEntitiesIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                connection.BulkInsert(tables, mappings);
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForEntitiesDbDataReader()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsert<BulkInsertIdentityTable>((DbDataReader)reader);

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);

                        // Act
                        var result = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                        // Assert
                        Assert.AreEqual(tables.Count * 2, result.Count());
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForEntitiesDbDataReaderWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.RowGuid), nameof(BulkInsertIdentityTable.RowGuid)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsert<BulkInsertIdentityTable>((DbDataReader)reader, mappings);

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);

                        // Act
                        var result = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                        // Assert
                        Assert.AreEqual(tables.Count * 2, result.Count());
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForEntitiesDbDataReaderIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        destinationConnection.BulkInsert<BulkInsertIdentityTable>((DbDataReader)reader, mappings);
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForEntitiesDataTable()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsert<BulkInsertIdentityTable>(table);

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);

                            // Act
                            var result = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                            // Assert
                            Assert.AreEqual(tables.Count * 2, result.Count());
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForEntitiesDataTableWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.RowGuid), nameof(BulkInsertIdentityTable.RowGuid)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsert<BulkInsertIdentityTable>(table, DataRowState.Unchanged, mappings);

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);

                            // Act
                            var result = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                            // Assert
                            Assert.AreEqual(tables.Count * 2, result.Count());
                        }
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForEntitiesDataTableIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            destinationConnection.BulkInsert<BulkInsertIdentityTable>(table, DataRowState.Unchanged, mappings);
                        }
                    }
                }
            }
        }

        #endregion

        #region BulkInsert<TEntity>(Extra Fields)

        [TestMethod]
        public void TestSqlConnectionBulkInsertForEntitiesWithExtraFields()
        {
            // Setup
            var tables = Helper.CreateWithExtraFieldsBulkInsertIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsert(tables);

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForEntitiesWithExtraFieldsWithMappings()
        {
            // Setup
            var tables = Helper.CreateWithExtraFieldsBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsert(tables);

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        #endregion

        #region BulkInsert(TableName)

        [TestMethod]
        public void TestSqlConnectionBulkInsertForTableNameDataEntities()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsert(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), tables);

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForTableNameDbDataReader()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsert(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), (DbDataReader)reader);

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);

                        // Act
                        var result = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                        // Assert
                        Assert.AreEqual(tables.Count * 2, result.Count());
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForTableNameDbDataReaderWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.RowGuid), nameof(BulkInsertIdentityTable.RowGuid)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsert(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), (DbDataReader)reader, mappings);

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);

                        // Act
                        var result = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                        // Assert
                        Assert.AreEqual(tables.Count * 2, result.Count());
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForTableNameDbDataReaderIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsert(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), (DbDataReader)reader, mappings);

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForTableNameDbDataReaderIfTheTableNameIsNotValid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        destinationConnection.BulkInsert("InvalidTable", (DbDataReader)reader);
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForTableNameDbDataReaderIfTheTableNameIsMissing()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        destinationConnection.BulkInsert("MissingTable", (DbDataReader)reader);
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForTableNameDbDataTable()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsert(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), table);

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);

                            // Act
                            var result = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                            // Assert
                            Assert.AreEqual(tables.Count * 2, result.Count());
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertForTableNameDbDataTableWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.RowGuid), nameof(BulkInsertIdentityTable.RowGuid)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsert(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), table, DataRowState.Unchanged, mappings);

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);

                            // Act
                            var result = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                            // Assert
                            Assert.AreEqual(tables.Count * 2, result.Count());
                        }
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForTableNameDbDataTableIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsert(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), table, DataRowState.Unchanged, mappings);

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);
                        }
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForTableNameDbDataTableIfTheTableNameIsNotValid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            destinationConnection.BulkInsert("InvalidTable", table, DataRowState.Unchanged);
                        }
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertForTableNameDbDataTableIfTheTableNameIsMissing()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            destinationConnection.BulkInsert("MissingTable", table, DataRowState.Unchanged);
                        }
                    }
                }
            }
        }

        #endregion

        #region BulkInsertAsync<TEntity>

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForEntities()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsertAsync(tables).Result;

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForEntitiesWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsertAsync(tables).Result;

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForEntitiesIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsertAsync(tables, mappings);

                // Trigger
                var result = bulkInsertResult.Result;
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForEntitiesDbDataReader()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsertAsync<BulkInsertIdentityTable>((DbDataReader)reader).Result;

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);

                        // Act
                        var queryResult = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                        // Assert
                        Assert.AreEqual(tables.Count * 2, queryResult.Count());
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForEntitiesDbDataReaderWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.RowGuid), nameof(BulkInsertIdentityTable.RowGuid)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsertAsync<BulkInsertIdentityTable>((DbDataReader)reader, mappings).Result;

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);

                        // Act
                        var queryResult = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                        // Assert
                        Assert.AreEqual(tables.Count * 2, queryResult.Count());
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForEntitiesDbDataReaderIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsertAsync<BulkInsertIdentityTable>((DbDataReader)reader, mappings);

                        // Trigger
                        var result = bulkInsertResult.Result;
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForEntitiesDataTable()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsertAsync<BulkInsertIdentityTable>(table).Result;

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);

                            // Act
                            var queryResult = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                            // Assert
                            Assert.AreEqual(tables.Count * 2, queryResult.Count());
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForEntitiesDataTableWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.RowGuid), nameof(BulkInsertIdentityTable.RowGuid)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsertAsync<BulkInsertIdentityTable>(table, DataRowState.Unchanged, mappings).Result;

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);

                            // Act
                            var queryResult = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                            // Assert
                            Assert.AreEqual(tables.Count * 2, queryResult.Count());
                        }
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForEntitiesDataTableIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsertAsync<BulkInsertIdentityTable>(table, DataRowState.Unchanged, mappings);

                            // Trigger
                            var result = bulkInsertResult.Result;
                        }
                    }
                }
            }
        }

        #endregion

        #region BulkInsertAsync<TEntity>(Extra Fields)

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForEntitiesWithExtraFields()
        {
            // Setup
            var tables = Helper.CreateWithExtraFieldsBulkInsertIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsertAsync(tables).Result;

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForEntitiesWithExtraFieldsWithMappings()
        {
            // Setup
            var tables = Helper.CreateWithExtraFieldsBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsertAsync(tables).Result;

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        #endregion

        #region BulkInsertAsync(TableName)

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForTableNameDataEntities()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Act
                var bulkInsertResult = connection.BulkInsertAsync(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), tables).Result;

                // Assert
                Assert.AreEqual(tables.Count, bulkInsertResult);

                // Act
                var queryResult = connection.QueryAll<BulkInsertIdentityTable>();

                // Assert
                Assert.AreEqual(tables.Count, queryResult.Count());
                tables.AsList().ForEach(t =>
                {
                    Helper.AssertPropertiesEquality(t, queryResult.ElementAt(tables.IndexOf(t)));
                });
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForTableNameDbDataReader()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsertAsync(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), (DbDataReader)reader).Result;

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);

                        // Act
                        var queryResult = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                        // Assert
                        Assert.AreEqual(tables.Count * 2, queryResult.Count());
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForTableNameDbDataReaderWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.RowGuid), nameof(BulkInsertIdentityTable.RowGuid)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsertAsync(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), (DbDataReader)reader, mappings).Result;

                        // Assert
                        Assert.AreEqual(tables.Count, bulkInsertResult);

                        // Act
                        var queryResult = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                        // Assert
                        Assert.AreEqual(tables.Count * 2, queryResult.Count());
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForTableNameDbDataReaderIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsertAsync(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), (DbDataReader)reader, mappings);

                        // Trigger
                        var result = bulkInsertResult.Result;
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForTableNameDbDataReaderIfTheTableNameIsNotValid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsertAsync("InvalidTable", (DbDataReader)reader);

                        // Trigger
                        var result = bulkInsertResult.Result;
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForTableNameDbDataReaderIfTheTableNameIsMissing()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    // Open the destination connection
                    using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                    {
                        // Act
                        var bulkInsertResult = destinationConnection.BulkInsertAsync("MissingTable", (DbDataReader)reader);

                        // Trigger
                        var result = bulkInsertResult.Result;
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForTableNameDataTable()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsertAsync(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), table).Result;

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);

                            // Act
                            var queryResult = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                            // Assert
                            Assert.AreEqual(tables.Count * 2, queryResult.Count());
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlConnectionBulkInsertAsyncForTableNameDataTableWithMappings()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add the mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.RowGuid), nameof(BulkInsertIdentityTable.RowGuid)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnInt)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnNVarChar)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsertAsync(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), table, DataRowState.Unchanged, mappings).Result;

                            // Assert
                            Assert.AreEqual(tables.Count, bulkInsertResult);

                            // Act
                            var queryResult = destinationConnection.QueryAll<BulkInsertIdentityTable>();

                            // Assert
                            Assert.AreEqual(tables.Count * 2, queryResult.Count());
                        }
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForTableNameDataTableIfTheMappingsAreInvalid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);
            var mappings = new List<BulkInsertMapItem>();

            // Add invalid mappings
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnBit), nameof(BulkInsertIdentityTable.ColumnBit)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime), nameof(BulkInsertIdentityTable.ColumnDateTime)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDateTime2), nameof(BulkInsertIdentityTable.ColumnDateTime2)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnDecimal), nameof(BulkInsertIdentityTable.ColumnDecimal)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnFloat), nameof(BulkInsertIdentityTable.ColumnFloat)));

            // Switched
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnInt), nameof(BulkInsertIdentityTable.ColumnNVarChar)));
            mappings.Add(new BulkInsertMapItem(nameof(BulkInsertIdentityTable.ColumnNVarChar), nameof(BulkInsertIdentityTable.ColumnInt)));

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsertAsync(ClassMappedNameCache.Get<BulkInsertIdentityTable>(), table, DataRowState.Unchanged, mappings);

                            // Trigger
                            var result = bulkInsertResult.Result;
                        }
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForTableNameDataTableIfTheTableNameIsNotValid()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsertAsync("InvalidTable", table);

                            // Trigger
                            var result = bulkInsertResult.Result;
                        }
                    }
                }
            }
        }

        [TestMethod, ExpectedException(typeof(AggregateException))]
        public void ThrowExceptionOnSqlConnectionBulkInsertAsyncForTableNameDataTableIfTheTableNameIsMissing()
        {
            // Setup
            var tables = Helper.CreateBulkInsertIdentityTables(10);

            // Insert the records first
            using (var connection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                connection.InsertAll(tables);
            }

            // Open the source connection
            using (var sourceConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
            {
                // Read the data from source connection
                using (var reader = sourceConnection.ExecuteReader("SELECT * FROM [dbo].[BulkInsertIdentityTable];"))
                {
                    using (var table = new DataTable())
                    {
                        table.Load(reader);

                        // Open the destination connection
                        using (var destinationConnection = new SqlConnection(Database.ConnectionStringForRepoDb))
                        {
                            // Act
                            var bulkInsertResult = destinationConnection.BulkInsertAsync("MissingTable", table, DataRowState.Unchanged);

                            // Trigger
                            var result = bulkInsertResult.Result;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
