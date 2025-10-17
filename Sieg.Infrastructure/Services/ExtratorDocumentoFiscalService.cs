using Sieg.Domain.Entities;
using Sieg.Domain.Enums;
using Sieg.Domain.Exceptions;
using Sieg.Domain.Interfaces.Services;
using Sieg.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sieg.Infrastructure.Services;

public class ExtratorDocumentoFiscalService : IExtratorDocumentoFiscalService
{
    public DocumentoFiscal ExtrairCampos(XmlDocument xml, ETipoDocumentoFiscal tipo, Guid documentoId)
    {
        try
        {
            return tipo switch
            {
                ETipoDocumentoFiscal.NFe => ExtrairCamposNFe(xml, documentoId),
                ETipoDocumentoFiscal.CTe => ExtrairCamposCTe(xml, documentoId),
                ETipoDocumentoFiscal.NFSe => ExtrairCamposNFSe(xml, documentoId),
                _ => throw new DominioException("Tipo de documento desconhecido.")
            };
        }
        catch (XmlException ex)
        {
            throw new ExtracaoException("Erro ao processar o XML do documento fiscal.", ex);
        }
        catch (FormatException ex)
        {
            throw new ExtracaoException("Formato inválido encontrado durante a extração.", ex);
        }
        catch (DominioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ExtracaoException("Erro inesperado durante a extração do documento fiscal.", ex);
        }
    }

    private static DocumentoFiscal ExtrairCamposNFe(XmlDocument xml, Guid documentoId)
    {
        var ns = new XmlNamespaceManager(xml.NameTable);
        ns.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");

        var cnpj = ExtrairValorXml(xml, ns,
            "/nfe:nfeProc/nfe:NFe/nfe:infNFe/nfe:emit/nfe:CNPJ",
            "/nfe:NFe/nfe:infNFe/nfe:emit/nfe:CNPJ");

        var uf = ExtrairValorXml(xml, ns,
            "/nfe:nfeProc/nfe:NFe/nfe:infNFe/nfe:emit/nfe:enderEmit/nfe:UF",
            "/nfe:NFe/nfe:infNFe/nfe:emit/nfe:enderEmit/nfe:UF");

        var dataHoraEmissao = ExtrairValorXml(xml, ns,
            "/nfe:nfeProc/nfe:NFe/nfe:infNFe/nfe:ide/nfe:dhEmi",
            "/nfe:NFe/nfe:infNFe/nfe:ide/nfe:dhEmi");

        var dataEmissao = ExtrairValorXml(xml, ns,
            "/nfe:nfeProc/nfe:NFe/nfe:infNFe/nfe:ide/nfe:dEmi",
            "/nfe:NFe/nfe:infNFe/nfe:ide/nfe:dEmi");

        var valorTotal = ExtrairValorXml(xml, ns,
            "/nfe:nfeProc/nfe:NFe/nfe:infNFe/nfe:total/nfe:ICMSTot/nfe:vNF",
            "/nfe:NFe/nfe:infNFe/nfe:total/nfe:ICMSTot/nfe:vNF");

        var data = TentarParseEmissao(dataHoraEmissao, dataEmissao, out var dto) ? dto : DateTimeOffset.UtcNow;
        var total = ParseDecimalInvariante(valorTotal);
        var cnpjVo = new Cnpj(cnpj);
        var ufVo = new Uf(uf);

        return new DocumentoFiscal(documentoId, ETipoDocumentoFiscal.NFe, cnpjVo, data, ufVo, total);
    }

    private static DocumentoFiscal ExtrairCamposCTe(XmlDocument xml, Guid documentoId)
    {
        var ns = new XmlNamespaceManager(xml.NameTable);
        ns.AddNamespace("cte", "http://www.portalfiscal.inf.br/cte");

        var cnpj = ExtrairValorXml(xml, ns,
            "/cte:cteProc/cte:CTe/cte:infCte/cte:emit/cte:CNPJ",
            "/cte:CTe/cte:infCte/cte:emit/cte:CNPJ");

        var uf = ExtrairValorXml(xml, ns,
            "/cte:cteProc/cte:CTe/cte:infCte/cte:emit/cte:enderEmit/cte:UF",
            "/cte:CTe/cte:infCte/cte:emit/cte:enderEmit/cte:UF");

        var dataHoraEmissao = ExtrairValorXml(xml, ns,
            "/cte:cteProc/cte:CTe/cte:infCte/cte:ide/cte:dhEmi",
            "/cte:CTe/cte:infCte/cte:ide/cte:dhEmi");

        var valorTotal = ExtrairValorXml(xml, ns,
            "/cte:cteProc/cte:CTe/cte:infCte/cte:vPrest/cte:vTPrest",
            "/cte:CTe/cte:infCte/cte:vPrest/cte:vTPrest");

        var data = TentarParseEmissao(dataHoraEmissao, null, out var dto) ? dto : DateTimeOffset.UtcNow;
        var total = ParseDecimalInvariante(valorTotal);
        var cnpjVo = new Cnpj(cnpj);
        var ufVo = new Uf(uf);

        return new DocumentoFiscal(documentoId, ETipoDocumentoFiscal.NFe, cnpjVo, data, ufVo, total);
    }

    private static DocumentoFiscal ExtrairCamposNFSe(XmlDocument xml, Guid documentoId)
    {
        string cnpj =
            xml.GetElementsByTagName("CNPJ").Cast<XmlNode>().FirstOrDefault()?.InnerText ??
            xml.SelectSingleNode("//*[contains(local-name(),'Emit') or contains(local-name(),'Prest')]/CNPJ")?.InnerText ??
            "";

        string uf =
            xml.SelectSingleNode("//*[contains(local-name(),'ender') or contains(local-name(),'Endereco')]/UF")?.InnerText ??
            xml.GetElementsByTagName("UF").Cast<XmlNode>().FirstOrDefault()?.InnerText ??
            "";

        string? dataEmissao =
            xml.GetElementsByTagName("dhEmi").Cast<XmlNode>().FirstOrDefault()?.InnerText ??
            xml.GetElementsByTagName("dEmi").Cast<XmlNode>().FirstOrDefault()?.InnerText ??
            xml.GetElementsByTagName("DataEmissao").Cast<XmlNode>().FirstOrDefault()?.InnerText;

        string valorTotal =
            xml.GetElementsByTagName("vNF").Cast<XmlNode>().FirstOrDefault()?.InnerText ??
            xml.GetElementsByTagName("vTPrest").Cast<XmlNode>().FirstOrDefault()?.InnerText ??
            xml.GetElementsByTagName("ValorServicos").Cast<XmlNode>().FirstOrDefault()?.InnerText ?? "0";

        var data = TentarParseEmissao(dataEmissao, null, out var dto) ? dto : DateTimeOffset.UtcNow;
        var total = ParseDecimalInvariante(valorTotal);
        var cnpjVo = new Cnpj(cnpj);
        var ufVo = new Uf(uf);

        return new DocumentoFiscal(documentoId, ETipoDocumentoFiscal.NFe, cnpjVo, data, ufVo, total);
    }

    // Métodos auxiliares

    private static string ExtrairValorXml(XmlDocument doc, XmlNamespaceManager ns, params string[] xpaths)
    {
        foreach (var path in xpaths)
        {
            var node = doc.SelectSingleNode(path, ns);
            if (node != null) return node.InnerText;
        }

        return string.Empty;
    }

    private static bool TentarParseEmissao(string? isoDate, string? legacyDate, out DateTimeOffset result)
    {
        if (!string.IsNullOrWhiteSpace(isoDate) &&
            DateTimeOffset.TryParse(isoDate, null, DateTimeStyles.AssumeUniversal, out result))
            return true;

        if (!string.IsNullOrWhiteSpace(legacyDate) &&
            DateTimeOffset.TryParseExact(legacyDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out result))
            return true;

        result = default;
        return false;
    }

    private static decimal ParseDecimalInvariante(string? valor)
    {
        return decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : 0;
    }
}
