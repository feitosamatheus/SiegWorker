using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Exceptions;

public abstract class PersonalizadaException : Exception
{
    public PersonalizadaException() : base() { }
    public PersonalizadaException(string mensagem) : base(mensagem) { }
    public PersonalizadaException(string mensagem, Exception excecao) : base(mensagem, excecao) { }
}
