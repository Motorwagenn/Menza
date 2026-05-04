using Aspire.Hosting;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<SqlServerServerResource> sql;
IResourceBuilder<SqlServerDatabaseResource> database;

if (builder.Environment.IsEnvironment("Testing"))
{
    sql = builder.AddSqlServer("sql-testing")
                 .WithContainerName("sql-testing");

    database = sql.AddDatabase("database");
}
else
{
    sql = builder.AddSqlServer("sql")
                 .WithDataVolume()
                 .WithLifetime(ContainerLifetime.Persistent);

    database = sql.AddDatabase("database");

    builder.AddProject<Projects.UTB_Minute_DbManager>("dbmanager")
        .WithReference(database)
        .WithHttpCommand("reset-db", "Reset")
        .WaitFor(database);
}

var webapi = builder.AddProject<Projects.UTB_Minute_WebApi>("utb-minute-webapi")
                .WithReference(database)
                .WaitFor(database);

var adminClient = builder.AddProject<Projects.UTB_Minute_AdminClient>("adminclient")
    .WithReference(webapi)
    .WaitFor(webapi);

var canteenClient = builder.AddProject<Projects.UTB_Minute_CanteenClient>("canteenclient")
    .WithReference(webapi)
    .WaitFor(webapi);

builder.Build().Run();