using ApkaTestowa;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHealthChecks()
    .AddCheck<SampleHealthCheck>("Sample")
    .AddSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HangfireDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False",
            healthQuery: "SELECT 1;",
            name: "sql",
            failureStatus: HealthStatus.Degraded,
            tags: new string[] { "db", "sql", "sqlserver" })
    .AddUrlGroup(new Uri("https://www.nbp.pl"), "NBP");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


//app.MapHealthChecks("/healthz");

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).WithMetadata(new AllowAnonymousAttribute());

app.UseAuthorization();

app.MapControllers();

app.Run();
