var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                 .WithDataVolume()
                 .WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("database");

builder.AddProject<Projects.Menza_Meals_Manager>("dbmanager")
    .WithReference(database)
    .WithHttpCommand("reset-db", "Reset")
    .WaitFor(database);

builder.Build().Run();