using Sieg.Domain.Commons;
using Sieg.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Entities;

public sealed class Documento : EntidadeBase
{
    public string NomeOriginalArquivo { get; private set; }
    public string CaminhoXml { get; private set; }
    public long Tamanho { get; private set; }
    public bool Processado { get; private set; } = false;

    public ICollection<DocumentoFiscal> DocumentosFiscais { get; private set; } = new List<DocumentoFiscal>();

    public Documento(string nomeOriginalArquivo, string caminhoXml, long tamanho, bool processado)
    {
        NomeOriginalArquivo = nomeOriginalArquivo;
        CaminhoXml = caminhoXml;
        Tamanho = tamanho;
        Processado = processado;
    }

    public void ProcessarDocumento()
    {
        Processado = true;
    }

    protected override void Validar()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(NomeOriginalArquivo))
            erros.Add("O nome do arquivo original não pode ser nulo ou vazio.");

        if (string.IsNullOrWhiteSpace(CaminhoXml))
            erros.Add("O caminho do XML não pode ser nulo ou vazio.");

        if (Tamanho <= 0)
            erros.Add("O tamanho do arquivo deve ser maior que zero.");

        if (erros.Any())
            throw new DominioException(string.Join("; ", erros));
    }
}
