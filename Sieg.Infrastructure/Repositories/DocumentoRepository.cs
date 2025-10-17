using Microsoft.EntityFrameworkCore;
using Sieg.Domain.Entities;
using Sieg.Domain.Interfaces.Repositories;
using Sieg.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Infrastructure.Repositories;

public class DocumentoRepository : IDocumentoRepository
{
    private readonly DatabaseContext _context;

    public DocumentoRepository(DatabaseContext context) => _context = context;

    public void Atualizar(Documento documento)
        => _context.Documentos.Update(documento);

    public async Task<Documento?> ObterPorIdAsync(Guid documentoId, CancellationToken cancellationToken)
        => await _context.Documentos.FirstOrDefaultAsync(df => df.Id == documentoId, cancellationToken);
   
}
