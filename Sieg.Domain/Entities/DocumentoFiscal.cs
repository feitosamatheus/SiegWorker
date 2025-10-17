using Sieg.Domain.Commons;
using Sieg.Domain.Enums;
using Sieg.Domain.Exceptions;
using Sieg.Domain.ValueObjects;

namespace Sieg.Domain.Entities;

public sealed class DocumentoFiscal : EntidadeBase
{
    public Guid DocumentoId { get; init; }
    public Documento? Documento { get; init; }
    public ETipoDocumentoFiscal TipoDocumento { get; private set; }
    public Cnpj CnpjEmitente { get; private set; }
    public DateTimeOffset DataEmissao { get; private set; }
    public Uf UfEmitente { get; private set; }
    public decimal ValorTotal { get; private set; }
    
    public DocumentoFiscal(Guid documentoId, ETipoDocumentoFiscal tipoDocumento, Cnpj cnpjEmitente, DateTimeOffset dataEmissao, Uf ufEmitente, decimal valorTotal)
    {
        DocumentoId = documentoId;
        TipoDocumento = tipoDocumento;
        CnpjEmitente = cnpjEmitente;
        DataEmissao = dataEmissao;
        UfEmitente = ufEmitente;
        ValorTotal = valorTotal;
    }

    protected override void Validar()
    {
        var erros = new List<string>();

        if (DocumentoId == Guid.Empty)
            erros.Add("DocumentoId não pode ser vazio.");

        if (!Enum.IsDefined(typeof(ETipoDocumentoFiscal), TipoDocumento))
            erros.Add("Tipo de documento inválido.");

        if (CnpjEmitente == null)
            erros.Add("CNPJ do emitente não pode ser nulo.");

        if (UfEmitente == null)
            erros.Add("UF do emitente não pode ser nula.");

        if (DataEmissao > DateTimeOffset.UtcNow)
            erros.Add("Data de emissão não pode ser futura.");

        if (ValorTotal <= 0)
            erros.Add("Valor total deve ser maior que zero.");

        if (erros.Any())
            throw new DominioException(string.Join("; ", erros));
    }
}
