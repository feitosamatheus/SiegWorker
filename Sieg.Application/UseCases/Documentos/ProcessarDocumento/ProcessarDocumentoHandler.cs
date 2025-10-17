using MediatR;
using Microsoft.Extensions.Logging;
using Sieg.Domain.Exceptions;
using Sieg.Domain.Interfaces.Repositories;
using Sieg.Domain.Interfaces.Services;
using Sieg.Domain.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sieg.Application.UseCases.Documentos.ProcessarDocumento;

public sealed class ProcessarDocumentoHandler : IRequestHandler<ProcessarDocumentoCommand, Unit>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IDocumentoFiscalRepository _documentoFiscalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IArmazenamentoService _armazenamentoService;
    private readonly IDetectorDocumentoFiscalService _detectorDocumentoFiscalService;
    private readonly IExtratorDocumentoFiscalService _extratorDocumentoFiscalService;
    private readonly ILogger<ProcessarDocumentoHandler> _logger;

    public ProcessarDocumentoHandler(IDocumentoRepository documentoRepository, IDocumentoFiscalRepository documentoFiscalRepository, IUnitOfWork unitOfWork, IArmazenamentoService armazenamentoService, IDetectorDocumentoFiscalService detectorDocumentoFiscalService, IExtratorDocumentoFiscalService extratorDocumentoFiscalService, ILogger<ProcessarDocumentoHandler> logger)
    {
        _documentoRepository = documentoRepository;
        _documentoFiscalRepository = documentoFiscalRepository;
        _unitOfWork = unitOfWork;
        _armazenamentoService = armazenamentoService;
        _detectorDocumentoFiscalService = detectorDocumentoFiscalService;
        _extratorDocumentoFiscalService = extratorDocumentoFiscalService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ProcessarDocumentoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var documento = await _documentoRepository.ObterPorIdAsync(request.DocumentoId, cancellationToken);
            if (documento == null)
                throw new DocumentoNaoEncontradoException("Documento não encontrado");

            if (documento.Processado)
            {
                _logger.LogInformation("Documento {DocumentoId} já processado anteriormente. Ignorando reprocessamento.", request.DocumentoId);
                return Unit.Value; 
            }

            var documentoFiscalExistente = await _documentoFiscalRepository.ObterPorDocumentoIdAsync(documento.Id, cancellationToken);
            if (documentoFiscalExistente != null)
            {
                _logger.LogInformation("Documento fiscal já existente para o documento {DocumentoId}. Ignorando reprocessamento.", request.DocumentoId);
                return Unit.Value;
            }

            var xmlConteudo = await _armazenamentoService.ObterArquivoComoStringAsync(documento.CaminhoXml);
            if(xmlConteudo == null)
                throw new XmlInvalidoException("Conteúdo do XML não encontrado");


            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlConteudo));
            var xmlDocument = await LerECriarXmlDocumentAsync(stream);
            var tipoDocumento = _detectorDocumentoFiscalService.Detectar(xmlDocument);
            var documentoFiscal = _extratorDocumentoFiscalService.ExtrairCampos(xmlDocument, tipoDocumento, documento.Id);

            documento.ProcessarDocumento();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _documentoFiscalRepository.AdicionarAsync(documentoFiscal, cancellationToken);
            _documentoRepository.Atualizar(documento);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Unit.Value;
        }
        catch (DocumentoNaoEncontradoException ex)
        {
            _logger.LogWarning(ex, "Documento não encontrado durante o processamento. DocumentoId: {DocumentoId}", request.DocumentoId);
            throw;
        }
        catch (XmlInvalidoException ex)
        {
            _logger.LogWarning(ex, "XML inválido ao processar documento {DocumentoId}: {Mensagem}", request.DocumentoId, ex.Message);
            throw;
        }
        catch (DominioDadosInvalidosException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos no XML do documento {DocumentoId}: {Mensagem}", request.DocumentoId, ex.Message);
            throw;
        }
        catch (ExtracaoException ex)
        {
            _logger.LogWarning(ex, "Erro ao extrair campos do documento fiscal {DocumentoId}: {Mensagem}", request.DocumentoId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar documento {DocumentoId}", request.DocumentoId);
            throw;
        }
    }

    private async Task<XmlDocument> LerECriarXmlDocumentAsync(Stream conteudoStream)
    {
        if (conteudoStream == null || !conteudoStream.CanRead)
            throw new DominioDadosInvalidosException("Stream inválida ou vazia");

        try
        {
            using var reader = new StreamReader(conteudoStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            var conteudoXml = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(conteudoXml))
                throw new DominioDadosInvalidosException("O conteúdo XML está vazio");

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            var xml = new XmlDocument { XmlResolver = null };
            using var stringReader = new StringReader(conteudoXml);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            xml.Load(xmlReader);

            return xml;
        }
        catch (XmlException)
        {
            throw new DominioDadosInvalidosException("O XML do documento fiscal está malformado");
        }
        catch (Exception ex)
        {
            throw new DominioDadosInvalidosException($"Falha ao processar XML: {ex.Message}");
        }
    }
}
