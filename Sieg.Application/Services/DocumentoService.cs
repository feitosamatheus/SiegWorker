using MediatR;
using Sieg.Application.Interfaces;
using Sieg.Application.UseCases.Documentos.ProcessarDocumento;
using System.Threading;

namespace Sieg.Application.Services;

public class DocumentoService : IDocumentoService
{
    private readonly IMediator _mediator;

    public DocumentoService(IMediator mediator)
        => _mediator = mediator;

    public async Task ProcessarDocumentoAsync(Guid documentId, CancellationToken cancellationToken)
        => await _mediator.Send(new ProcessarDocumentoCommand(documentId), cancellationToken);
}
