using Sieg.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Interfaces.Repositories;

public interface IDocumentoRepository
{
    void Atualizar(Documento documento);
    Task<Documento?> ObterPorIdAsync(Guid documentId, CancellationToken cancellationToken);
}
