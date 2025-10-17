using Microsoft.EntityFrameworkCore;
using Sieg.Domain.Entities;
using Sieg.Domain.Interfaces.Repositories;
using Sieg.Domain.ValueObjects;
using Sieg.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sieg.Infrastructure.Repositories;

public sealed class DocumentoFiscalRepository : IDocumentoFiscalRepository
{
    private readonly DatabaseContext _context;

    public DocumentoFiscalRepository(DatabaseContext context) => _context = context;

    public async Task AdicionarAsync(DocumentoFiscal documentoFiscal, CancellationToken cancellationToken)
        => await _context.DocumentosFiscais.AddAsync(documentoFiscal, cancellationToken);

    public async Task<DocumentoFiscal?> ObterPorDocumentoIdAsync(Guid documentoId, CancellationToken cancellationToken)
        => await _context.DocumentosFiscais.FirstOrDefaultAsync(df => df.DocumentoId == documentoId, cancellationToken);
}
