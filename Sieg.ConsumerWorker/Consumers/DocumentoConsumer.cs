using MassTransit;
using Newtonsoft.Json.Linq;
using Sieg.Application.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class DocumentoConsumer : IConsumer<JObject>
{
    private readonly IDocumentoService _documentoService;

    public DocumentoConsumer(IDocumentoService documentoService)
        =>  _documentoService = documentoService;

    public async Task Consume(ConsumeContext<JObject> context)
    {
        var payload = context.Message;
        var eventId = payload["EventId"]?.ToString();
        var documentId = payload["DocumentoId"]?.ToString();

        await _documentoService.ProcessarDocumentoAsync(Guid.Parse(documentId!), context.CancellationToken);
    }
}
