namespace Sieg.Application.Interfaces;

public interface IDocumentoService
{
    Task ProcessarDocumentoAsync(Guid documentId, CancellationToken cancellation);
}
