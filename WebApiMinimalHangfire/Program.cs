using Hangfire;
using Serilog;
using WebApiMinimalHangfire.Jobs;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog(); // serilog

    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // app services
    builder.Services.AddTransient<SimpleJob>();

    // Add hangfire services.
    builder.Services.AddHangfire(configuration =>
        configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSerilogLogProvider()
            .UseColouredConsoleLogProvider()
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

    // endpoints
    app.MapPost(
            "/enqueue-background-job",
            (string parameter = "test") =>
            {
                BackgroundJob.Enqueue<SimpleJob>(job => job.Execute("Background", parameter));
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
                    job => job.Execute("Scheduled", parameter),
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
                    job => job.Execute("Recurring", parameter),
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
