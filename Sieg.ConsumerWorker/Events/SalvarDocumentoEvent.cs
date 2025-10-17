using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.ConsumerWorker.Models;

public sealed record SalvarDocumentoEvent(Guid EventId, Guid DocumentoId);
