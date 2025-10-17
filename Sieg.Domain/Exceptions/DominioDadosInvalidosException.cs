using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Exceptions;

public class DominioDadosInvalidosException : DominioException
{
    public DominioDadosInvalidosException() : base() { }
    public DominioDadosInvalidosException(string mensagem) : base(mensagem) { }
    public DominioDadosInvalidosException(string mensagem, Exception excecaoInterna) : base(mensagem, excecaoInterna) { }
}
