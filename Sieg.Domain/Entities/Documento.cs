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
    public string XmlHash { get; private set; } = string.Empty;

    public ICollection<DocumentoFiscal> DocumentosFiscais { get; private set; } = new List<DocumentoFiscal>();

    public Documento(string nomeOriginalArquivo, string caminhoXml, long tamanho, string xmlConteudo)
    {
        NomeOriginalArquivo = nomeOriginalArquivo;
        CaminhoXml = caminhoXml;
        Tamanho = tamanho;
        DefinirHash(xmlConteudo);

        Validar();
    }

    public void AtualizarDocumento(string nomeOriginalArquivo, string caminhoXml)
    {
        NomeOriginalArquivo = nomeOriginalArquivo;
        CaminhoXml = caminhoXml;

        Validar();
        Atualizar();
    }

    public void AlterarCaminhoXml(string caminhoXml)
    {
        if (string.IsNullOrWhiteSpace(caminhoXml))
            throw new DominioException("Caminho do arquivo XML não pode ser vazio.");

        CaminhoXml = caminhoXml;
        Atualizar();
    }

    public void AlterarNomeOriginalArquivo(string nomeOriginalArquivo)
    {
        if (string.IsNullOrWhiteSpace(nomeOriginalArquivo))
            throw new DominioException("Nome original do arquivo não pode ser vazio.");

        NomeOriginalArquivo = nomeOriginalArquivo;
        Atualizar();
    }

    public void ProcessarDocumento()
    {
        Processado = true;
    }

    private void DefinirHash(string xmlConteudo)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(xmlConteudo);
        var hashBytes = sha256.ComputeHash(bytes);
        XmlHash = Convert.ToHexString(hashBytes);
    }

    protected override void Validar()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(NomeOriginalArquivo))
            erros.Add("Nome original do arquivo não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(CaminhoXml))
            erros.Add("Caminho do arquivo XML não pode ser vazio.");

        if (Tamanho <= 0)
            erros.Add("O tamanho do arquivo deve ser maior que zero.");

        if (!CaminhoXml.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            erros.Add("O arquivo deve ser do tipo XML.");

        if (string.IsNullOrWhiteSpace(XmlHash))
            erros.Add("O hash do XML não pode ser vazio. Certifique-se de definir o hash do conteúdo.");

        if (erros.Any())
            throw new DominioException(string.Join("; ", erros));
    }
}
