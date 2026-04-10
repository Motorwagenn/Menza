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

builder.AddProject<Projects.UTB_Minute_WebApi>("utb-minute-webapi")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();