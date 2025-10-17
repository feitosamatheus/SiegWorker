using MassTransit;
using Newtonsoft.Json.Linq;
using Serilog;
using Sieg.ConsumerWorker;
using Sieg.Domain.Exceptions;
using Sieg.IoC;
using System.ComponentModel.DataAnnotations;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console()
    .WriteTo.File("logs/siegWorker.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
    loggingBuilder.AddSerilog(dispose: true));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureRepositories(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);


builder.Services.AddMassTransit(x =>
{
    var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "b-ef391786-dc48-4b6f-b582-474e5f32078d.mq.us-east-2.on.aws";
    var rabbitPort = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5671");
    var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "sieg-database-desafio";
    var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "SiegDesafio123!";

    x.AddConsumer<DocumentoConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rabbitUri = $"amqps://{rabbitUser}:{rabbitPass}@{rabbitHost}:{rabbitPort}/";

        cfg.Host(new Uri(rabbitUri), h =>
        {
            h.UseSsl(s => s.Protocol = System.Security.Authentication.SslProtocols.Tls12);
        });

        cfg.UseNewtonsoftJsonSerializer();
        cfg.UseNewtonsoftJsonDeserializer();

        cfg.Message<JObject>(m => m.SetEntityName("events"));
        cfg.Publish<JObject>(p => p.ExchangeType = RabbitMQ.Client.ExchangeType.Topic);

        cfg.ReceiveEndpoint("documento-queue", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.Bind("events", b =>
            {
                b.ExchangeType = RabbitMQ.Client.ExchangeType.Topic;
                b.RoutingKey = "documento.*";    
            });
            e.PrefetchCount = 16;
            e.UseMessageRetry(r =>
            {
                r.Exponential(
                    retryLimit: 5,
                    minInterval: TimeSpan.FromSeconds(1),
                    maxInterval: TimeSpan.FromSeconds(30),
                    intervalDelta: TimeSpan.FromSeconds(5));

                r.Ignore<DominioException>();
                r.Ignore<ExtracaoException>();
                r.Ignore<DominioDadosInvalidosException>();
                r.Ignore<DocumentoNaoEncontradoException>();
                r.Ignore<XmlInvalidoException>();
                r.Ignore<ExtracaoException>();
                r.Ignore<ValidationException>();
                r.Ignore<ArgumentException>();
                r.Ignore<Newtonsoft.Json.JsonException>();
            });
            e.ConfigureConsumer<DocumentoConsumer>(ctx);
        });
    });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
