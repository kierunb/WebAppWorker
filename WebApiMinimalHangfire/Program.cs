using Hangfire;
using Newtonsoft.Json;
using Serilog;
using WebApiMinimalHangfire.Jobs;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog();

    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // app services
    builder.Services.AddTransient<SimpleJob>();

    // Hangfire
    // Add Hangfire services.
    builder.Services.AddHangfire(configuration =>
        configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireDB"))
    );
    builder.Services.AddHangfireServer();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseHangfireDashboard(
        "/hangfire", // dashboard url
        new DashboardOptions
        {
            //custom authorization filter
            //Authorization = new[] { new MyAuthorizationFilter() }
        }
    );

    // Endpoints
    app.MapPost(
            "/enqueue-background-job",
            (string parameter = "test") =>
            {
                BackgroundJob.Enqueue<SimpleJob>(
                    job => job.Execute("Background", parameter));
                return Results.Created();
            }
        )
        .WithName("Enqueue Background Job")
        .WithOpenApi();

    app.MapPost(
            "/enqueue-delayed-job",
            (string parameter = "test") =>
            {
                var delay = TimeSpan.FromSeconds(10);

                BackgroundJob.Schedule<SimpleJob>(
                    job => job.Execute("Background", parameter),
                    delay
                );

                return Results.Created();
            }
        )
        .WithName("Enqueue Delayed Job")
        .WithOpenApi();

    app.MapPost(
            "/enqueue-recurring-job",
            (string parameter = "test") =>
            {
                string recurringJobId = Guid.NewGuid().ToString();
                string cronExpression = Cron.Minutely();

                RecurringJob.AddOrUpdate<SimpleJob>(
                    recurringJobId,
                    job => job.Execute("Background", parameter),
                    cronExpression
                );
                return Results.Created();
            }
        )
        .WithName("Enqueue Recurring Job")
        .WithOpenApi();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
