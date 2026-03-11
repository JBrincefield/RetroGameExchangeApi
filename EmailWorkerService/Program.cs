using EmailWorkerService;
using EmailWorkerService.Services;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<Worker>();

var metricServer = new MetricServer(port: 9091);
metricServer.Start();

var host = builder.Build();
host.Run();