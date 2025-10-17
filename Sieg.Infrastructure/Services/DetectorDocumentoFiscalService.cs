using Sieg.Domain.Enums;
using Sieg.Domain.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sieg.Infrastructure.Services;

public sealed class DetectorDocumentoFiscalService : IDetectorDocumentoFiscalService
{
    private const string NFE_NS = "http://www.portalfiscal.inf.br/nfe";
    private const string CTE_NS = "http://www.portalfiscal.inf.br/cte";

    public ETipoDocumentoFiscal Detectar(XmlDocument xml)
    {
        var root = xml.DocumentElement;
        if (root == null) return ETipoDocumentoFiscal.Desconhecido;

        var tipo = ObterTipoPorElemento(root.LocalName, root.NamespaceURI);
        if (tipo != ETipoDocumentoFiscal.Desconhecido) 
            return tipo;

        if (root.FirstChild is XmlElement primeiroFilho)
        {
            tipo = ObterTipoPorElemento(primeiroFilho.LocalName, primeiroFilho.NamespaceURI);
            if (tipo != ETipoDocumentoFiscal.Desconhecido) 
                return tipo;
        }

        foreach (XmlNode child in root.ChildNodes)
        {
            if (child is XmlElement elementoFilho)
            {
                if (ElementoSugereNFSe(elementoFilho.LocalName, elementoFilho.NamespaceURI))
                    return ETipoDocumentoFiscal.NFSe;
            }
        }

        if (ElementoSugereNFSe(root.LocalName, root.NamespaceURI))
            return ETipoDocumentoFiscal.NFSe;

        return ETipoDocumentoFiscal.Desconhecido;
    }

    private ETipoDocumentoFiscal ObterTipoPorElemento(string nome, string? uri)
    {
        uri ??= string.Empty;

        if ((uri == NFE_NS) && (nome == "NFe" || nome == "nfeProc"))
            return ETipoDocumentoFiscal.NFe;

        if ((uri == CTE_NS) && (nome == "CTe" || nome == "cteProc"))
            return ETipoDocumentoFiscal.CTe;

        return ETipoDocumentoFiscal.Desconhecido;
    }

    private bool ElementoSugereNFSe(string nome, string? uri)
    {
        nome = nome.ToLowerInvariant();
        uri = (uri ?? string.Empty).ToLowerInvariant();

        return nome.Contains("nfse") || nome.Contains("rps") || uri.Contains("nfse");
    }
}
