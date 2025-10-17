using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sieg.Application.Interfaces;
using Sieg.Application.Services;
using Sieg.Domain.Interfaces.Repositories;
using Sieg.Domain.Interfaces.Services;
using Sieg.Domain.Interfaces.UnitOfWork;
using Sieg.Infrastructure.Contexts;
using Sieg.Infrastructure.Repositories;
using Sieg.Infrastructure.Services;
using Sieg.Infrastructure.UnitOfWork;

namespace Sieg.IoC;

public static class IoCConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServerConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var assembly = AppDomain.CurrentDomain.Load("Sieg.Application");
        services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDocumentoService, DocumentoService>();

        return services;
    }

    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDocumentoFiscalRepository, DocumentoFiscalRepository>();
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDetectorDocumentoFiscalService, DetectorDocumentoFiscalService>();
        services.AddScoped<IExtratorDocumentoFiscalService, ExtratorDocumentoFiscalService>();

        var awsOptions = new Amazon.Extensions.NETCore.Setup.AWSOptions
        {
            Credentials = new Amazon.Runtime.BasicAWSCredentials(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"]
            ),
            Region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
        };

        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();

        var bucketName = configuration["AWS:BucketName"] ?? "xml-fiscais";
        services.AddSingleton<IArmazenamentoService>(sp =>
            new ArmazenamentoService(sp.GetRequiredService<IAmazonS3>(), bucketName)
        );


        return services;
    }
}
