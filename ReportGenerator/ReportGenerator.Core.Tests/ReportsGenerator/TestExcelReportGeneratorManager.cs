﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportGenerator.Core.Database;
using ReportGenerator.Core.Database.Factories;
using ReportGenerator.Core.Database.Managers;
using ReportGenerator.Core.Database.Utils;
using ReportGenerator.Core.Helpers;
using ReportGenerator.Core.ReportsGenerator;
using ReportGenerator.Core.Tests.TestUtils;
using Xunit;

namespace ReportGenerator.Core.Tests.ReportsGenerator
{
    public class TestExcelReportGeneratorManager
    {
        [Fact]
        public void TestGenerateReportSqlServer()
        {
            _testSqlServerDbName = TestSqlServerDatabasePattern + "_" + DateTime.Now.Millisecond.ToString();
            SetUpSqlServerTestData();
            // executing extraction ...
            object[] parameters = ExcelReportGeneratorHelper.CreateParameters(1, 2, 3);
            ILoggerFactory loggerFactory = new LoggerFactory();
            IReportGeneratorManager manager = new ExcelReportGeneratorManager(loggerFactory, DbEngine.SqlServer, 
                                                                              TestSqlServerHost, _testSqlServerDbName);
            Task<bool> result = manager.GenerateAsync(TestExcelTemplate, SqlServerDataExecutionConfig, ReportFile, parameters);
            result.Wait();
            Assert.True(result.Result);
            TearDownSqlServerTestData();
        }

        [Fact]
        public void TestGenerateReportSqlLite()
        {
            SetUpSqLiteTestData();
            object[] parameters = ExcelReportGeneratorHelper.CreateParameters(1, 2, 3);
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            IReportGeneratorManager manager = new ExcelReportGeneratorManager(loggerFactory, DbEngine.SqLite, 
                                                                              $"Data Source={TestSqLiteDatabase};Version={3}");
            Task<bool> result = manager.GenerateAsync(TestExcelTemplate, SqLiteDataExecutionConfig, ReportFile, parameters);
            result.Wait();
            Assert.True(result.Result);
            TearDownSqLiteTestData();
        }
        
        public void TestGenerateReportPostgres()
        {
            
        }
        
        public void TestGenerateReportMySql()
        {
            
        }

        private void SetUpSqlServerTestData()
        {
            // TestSqlServerDatabaseManager.CreateDatabase(TestSqlServerHost, _testSqlServerDbName);
            _dbManager = new CommonDbManager(DbEngine.SqlServer, _loggerFactory.CreateLogger<CommonDbManager>());
            IDictionary<string, string> connectionStringParams = new Dictionary<string, string>()
            {
                {DbParametersKeys.HostKey, TestSqlServerHost},
                {DbParametersKeys.DatabaseKey, _testSqlServerDbName},
                {DbParametersKeys.UseIntegratedSecurityKey, "true"},
                {DbParametersKeys.UseTrustedConnectionKey, "true"}
            };
            _connectionString = ConnectionStringBuilder.Build(DbEngine.SqlServer, connectionStringParams);
            _dbManager.CreateDatabase(_connectionString, true);
            // 
            string createDatabaseStatement = File.ReadAllText(Path.GetFullPath(SqlServerCreateDatabaseScript));
            string insertDataStatement = File.ReadAllText(Path.GetFullPath(SqlServerInsertDataScript));
            // TestSqlServerDatabaseManager.ExecuteSql(TestSqlServerHost, _testSqlServerDbName, createDatabaseStatement);
            // TestSqlServerDatabaseManager.ExecuteSql(TestSqlServerHost, _testSqlServerDbName, insertDataStatement);
            _dbManager.ExecuteNonQueryAsync(_connectionString, createDatabaseStatement).Wait();
            _dbManager.ExecuteNonQueryAsync(_connectionString, insertDataStatement).Wait();
        }

        private void TearDownSqlServerTestData()
        {
            //TestSqlServerDatabaseManager.DropDatabase(Server, _testDbName);
            _dbManager.DropDatabase(_connectionString);
        }

        private void SetUpSqLiteTestData()
        {
            TestSqLiteDatabaseManager.CreateDatabase(TestSqLiteDatabase);
            string createDatabaseStatement = File.ReadAllText(Path.GetFullPath(SqLiteCreateDatabaseScript));
            string insertDataStatement = File.ReadAllText(Path.GetFullPath(SqLiteInsertDataScript));
            TestSqLiteDatabaseManager.ExecuteSql(TestSqLiteDatabase, createDatabaseStatement);
            TestSqLiteDatabaseManager.ExecuteSql(TestSqLiteDatabase, insertDataStatement);
        }

        private void TearDownSqLiteTestData()
        {
            TestSqLiteDatabaseManager.DropDatabase(TestSqLiteDatabase);
        }
        
        private void SetUpMySqlTestData()
        {
            //TestSqLiteDatabaseManager.CreateDatabase(TestSqLiteDatabase);
            //string createDatabaseStatement = File.ReadAllText(Path.GetFullPath(SqLiteCreateDatabaseScript));
            //string insertDataStatement = File.ReadAllText(Path.GetFullPath(SqLiteInsertDataScript));
            //TestSqLiteDatabaseManager.ExecuteSql(TestSqLiteDatabase, createDatabaseStatement);
            //TestSqLiteDatabaseManager.ExecuteSql(TestSqLiteDatabase, insertDataStatement);
        }

        private void TearDownMySqlTestData()
        {
            //TestSqLiteDatabaseManager.DropDatabase(TestSqLiteDatabase);
        }

        private const string TestExcelTemplate = @"..\..\..\TestExcelTemplates\CitizensTemplate.xlsx";
        private const string ReportFile = @".\Report.xlsx";
        private const string SqlServerDataExecutionConfig = @"..\..\..\ExampleConfig\sqlServerDataExtractionParams.xml";
        private const string SqLiteDataExecutionConfig = @"..\..\..\ExampleConfig\sqLiteDataExtractionParams.xml";

        private const string TestSqlServerHost = @"(localdb)\mssqllocaldb";
        private const string TestSqlServerDatabasePattern = "ReportGeneratorTestDb";
        private const string TestSqLiteDatabase = "ReportGeneratorTestDb.sqlite";

        private const string SqlServerCreateDatabaseScript = @"..\..\..\DbScripts\SqlServerCreateDb.sql";
        private const string SqlServerInsertDataScript = @"..\..\..\DbScripts\SqlServerCreateData.sql";
        private const string SqLiteCreateDatabaseScript = @"..\..\..\DbScripts\SqLiteCreateDb.sql";
        private const string SqLiteInsertDataScript = @"..\..\..\DbScripts\SqLiteCreateData.sql";
        private const string MySqlCreateDatabaseScript = @"..\..\..\DbScripts\MySqlCreateDb.sql";
        private const string MySqlInsertDataScript = @"..\..\..\DbScripts\MySqlCreateData.sql";

        private string _testSqlServerDbName;
        private string _connectionString;
        private readonly ILoggerFactory _loggerFactory = new LoggerFactory();
        private IDbManager _dbManager;
    }
}
