using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Interfaces.Services;

public interface IArmazenamentoService
{
    Task<string> SalvarAsync(MemoryStream memoria);

    Task<string> ObterArquivoComoStringAsync(string caminho);
}
