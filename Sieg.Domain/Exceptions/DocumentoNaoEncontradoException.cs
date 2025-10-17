using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Exceptions;

public class DocumentoNaoEncontradoException : DominioException
{
    public DocumentoNaoEncontradoException() : base() { }
    public DocumentoNaoEncontradoException(string mensagem) : base(mensagem) { }
    public DocumentoNaoEncontradoException(string mensagem, Exception excecao) : base(mensagem, excecao) { }
}
