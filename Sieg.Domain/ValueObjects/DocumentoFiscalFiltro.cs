using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.ValueObjects;

public sealed record DocumentoFiscalFiltro(
    string? CnpjEmitente = null,
    string? UfEmitente = null,
    DateTimeOffset? DataInicio = null,
    DateTimeOffset? DataFim = null);
