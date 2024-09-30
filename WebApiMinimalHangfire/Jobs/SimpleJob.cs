namespace WebApiMinimalHangfire.Jobs;

public class SimpleJob(ILogger<SimpleJob> logger)
{
    public Task Execute(string jobType, string parameter = "test")
    {
        logger.LogInformation(
            "{jobType} job with parameter: {parameter} scheduled",
            jobType,
            parameter
        );

        return Task.CompletedTask;
    }
}
