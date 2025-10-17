using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Sieg.Domain.Interfaces.Services;

namespace Sieg.Infrastructure.Services;

public sealed class ArmazenamentoService : IArmazenamentoService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public ArmazenamentoService(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    public async Task<string> SalvarAsync(MemoryStream arquivo)
    {
        var nomeArquivo = $"{Guid.NewGuid()}.xml";

        arquivo.Position = 0;
        var key = $"{nomeArquivo}";

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(arquivo, _bucketName, key);

        return key; 
    }

    public async Task<string> ObterArquivoComoStringAsync(string caminho)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = caminho
        };

        using var response = await _s3Client.GetObjectAsync(request);
        using var reader = new StreamReader(response.ResponseStream);
        return await reader.ReadToEndAsync();
    }
}
