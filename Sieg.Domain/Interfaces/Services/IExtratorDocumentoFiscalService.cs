using Sieg.Domain.Entities;
using Sieg.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sieg.Domain.Interfaces.Services;

public interface IExtratorDocumentoFiscalService
{
    DocumentoFiscal ExtrairCampos(XmlDocument xml, ETipoDocumentoFiscal tipoDocumento, Guid documentoId);
}
