using Sieg.Domain.Entities;
using Sieg.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Interfaces.Repositories;

public interface IDocumentoFiscalRepository
{
    Task AdicionarAsync(DocumentoFiscal documentoFiscal, CancellationToken cancellationToken);
}
