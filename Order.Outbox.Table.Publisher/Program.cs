using MassTransit;
using Order.Outbox.Table.Publisher.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddQuartz(configurator =>
{
    JobKey jobKey = new JobKey("OrderOutboxPublishJob");
    configurator.AddJob<OrderOutboxPublishJob>(options => options.WithIdentity(jobKey));

    TriggerKey triggerKey = new TriggerKey("OrderOutboxPublishTrigger");
    configurator.AddTrigger(options => options.ForJob(jobKey)
    .WithIdentity(triggerKey)
    .StartAt(DateTime.UtcNow)
    .WithSimpleSchedule(_builder => _builder
        .WithIntervalInSeconds(5)
        .RepeatForever()));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});


var host = builder.Build();
host.Run();