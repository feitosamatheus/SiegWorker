using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Exceptions;

public class DominioException : PersonalizadaException
{
    public DominioException() : base() { }
    public DominioException(string mensagem) : base(mensagem) { }
    public DominioException(string mensagem, Exception excecao) : base(mensagem, excecao) { }
}
