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

    var keycloak = builder.AddKeycloak("keycloak", 8080)
                          .WithRealmImport("Realm")
                          .WithHttpsEndpoint(port: 8443, targetPort: 8080, name: "https")
                          .WithContainerName("utb-minute-keycloak-testing");
                      

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

    var keycloak = builder.AddKeycloak("keycloak", 8080)
                          .WithRealmImport("Realm")
                          .WithHttpsEndpoint(port: 8080, targetPort: 8080, name: "http")
                          .WithContainerName("utb-minute-keycloak")
                          .WithDataVolume("utb-minute-keycloak-data")
                          .WithLifetime(ContainerLifetime.Persistent);

    





var webapi = builder.AddProject<Projects.UTB_Minute_WebApi>("utb-minute-webapi")
                .WithReference(database)
                .WithReference(keycloak)
                .WaitFor(database)
                .WaitFor(keycloak); ;

var adminClient = builder.AddProject<Projects.UTB_Minute_AdminClient>("adminclient")
    .WithReference(webapi)
    .WithReference(keycloak)
    .WaitFor(webapi)
    .WaitFor(keycloak);

var canteenClient = builder.AddProject<Projects.UTB_Minute_CanteenClient>("canteenclient")
    .WithReference(webapi)
    .WithReference(keycloak)
    .WaitFor(webapi)
    .WaitFor(keycloak);
}
builder.Build().Run();