using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Exceptions;

public class XmlInvalidoException : DominioException
{
    public XmlInvalidoException() : base() { }
    public XmlInvalidoException(string mensagem) : base(mensagem) { }
    public XmlInvalidoException(string mensagem, Exception excecao) : base(mensagem, excecao) { }
}
