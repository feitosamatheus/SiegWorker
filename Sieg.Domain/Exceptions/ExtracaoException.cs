using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Exceptions;

public class ExtracaoException : PersonalizadaException
{
    public ExtracaoException() { }
    public ExtracaoException(string mensagem) : base(mensagem) { }
    public ExtracaoException(string mensagem, Exception excecaoInterna) : base(mensagem, excecaoInterna) { }
}
