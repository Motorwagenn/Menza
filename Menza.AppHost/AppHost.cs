var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                 .WithDataVolume()
                 .WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("database");

builder.AddProject<Projects.UTB_Minute_DbManager>("dbmanager")
    .WithReference(database)
    .WithHttpCommand("reset-db", "Reset")
    .WaitFor(database);

builder.AddProject<Projects.UTB_Minute_WebApi>("utb-minute-webapi");

builder.Build().Run();