﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepoDb.UnitTests.Setup;
using System;
using System.Linq;

namespace RepoDb.UnitTests
{
    public partial class QueryGroupTest
    {
        #region Name

        [TestMethod]
        public void TestQueryGroupParseExpressionWithNameAtLeft()
        {
            // Setup
            var parsed = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.PropertyInt == 1, Helper.DbSetting);

            // Act
            var actual = parsed.QueryFields.First().Field.Name;
            var expected = "[PropertyInt]";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestQueryGroupParseExpressionWhereNameHasMapping()
        {
            // Setup
            var parsed = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.MappedPropertyString == "ABC", Helper.DbSetting);

            // Act
            var actual = parsed.QueryFields.First().Field.Name;
            var expected = "[PropertyString]";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        public void ThrowExceptionOnParseExpressionWithNameAtRight()
        {
            // Act
            QueryGroup.Parse<QueryGroupTestExpressionClass>(e => 1 == e.PropertyInt, Helper.DbSetting);
        }

        #endregion

        #region Properties

        [TestMethod]
        public void TestQueryGroupParseExpressionWithDoubleSameFieldsForAnd()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.PropertyInt == 1 && e.PropertyInt == 2, Helper.DbSetting).GetString();
            var expected = "(([PropertyInt] = @PropertyInt) AND ([PropertyInt] = @PropertyInt_1))";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestQueryGroupParseExpressionWithDoubleSameFieldsForOr()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.PropertyInt == 1 || e.PropertyInt == 2, Helper.DbSetting).GetString();
            var expected = "(([PropertyInt] = @PropertyInt) OR ([PropertyInt] = @PropertyInt_1))";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Groupings

        [TestMethod]
        public void TestQueryGroupParseExpressionWithSingleGroupForAnd()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.PropertyInt == 1 && e.PropertyString == "A", Helper.DbSetting).GetString();
            var expected = "(([PropertyInt] = @PropertyInt) AND ([PropertyString] = @PropertyString))";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestQueryGroupParseExpressionWithSingleGroupForOr()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.PropertyInt == 1 || e.PropertyString == "A", Helper.DbSetting).GetString();
            var expected = "(([PropertyInt] = @PropertyInt) OR ([PropertyString] = @PropertyString))";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestQueryGroupParseExpressionWithSingleGroupForOrAndSingleFieldForAnd()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => (e.PropertyInt == 1 || e.PropertyDouble == 2) && e.PropertyString == "A", Helper.DbSetting).GetString();
            var expected = "((([PropertyInt] = @PropertyInt) OR ([PropertyDouble] = @PropertyDouble)) AND ([PropertyString] = @PropertyString))";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestQueryGroupParseExpressionWithSingleGroupForAndAndSingleFieldForOr()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => (e.PropertyInt == 1 && e.PropertyDouble == 2) || e.PropertyString == "A", Helper.DbSetting).GetString();
            var expected = "((([PropertyInt] = @PropertyInt) AND ([PropertyDouble] = @PropertyDouble)) OR ([PropertyString] = @PropertyString))";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestQueryGroupParseExpressionWithDoubleGroupForAnd()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => (e.PropertyInt == 1 && e.PropertyDouble == 2) && (e.PropertyString == "A" && e.PropertySingle == 1), Helper.DbSetting).GetString();
            var expected = "((([PropertyInt] = @PropertyInt) AND ([PropertyDouble] = @PropertyDouble)) AND (([PropertyString] = @PropertyString) AND ([PropertySingle] = @PropertySingle)))";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestQueryGroupParseExpressionWithDoubleGroupForOr()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => (e.PropertyInt == 1 || e.PropertyDouble == 2) || (e.PropertyString == "A" || e.PropertySingle == 1), Helper.DbSetting).GetString();
            var expected = "((([PropertyInt] = @PropertyInt) OR ([PropertyDouble] = @PropertyDouble)) OR (([PropertyString] = @PropertyString) OR ([PropertySingle] = @PropertySingle)))";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Mapped

        [TestMethod]
        public void TestQueryGroupParseExpressionForMappedProperty()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.MappedPropertyString == "A", Helper.DbSetting).GetString();
            var expected = "([PropertyString] = @PropertyString)";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Quoted

        [TestMethod]
        public void TestQueryGroupParseExpressionForQuotedProperty()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.QuotedPropertyString == "A", Helper.DbSetting).GetString();
            var expected = "([PropertyString] = @PropertyString)";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Unorganized

        [TestMethod]
        public void TestQueryGroupParseExpressionForUnorganizedProperty()
        {
            // Act
            var actual = QueryGroup.Parse<QueryGroupTestExpressionClass>(e => e.UnorganizedPropertyString == "A", Helper.DbSetting).GetString();
            var expected = "([Property / . String] = @Property_____String)";

            // Assert
            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
