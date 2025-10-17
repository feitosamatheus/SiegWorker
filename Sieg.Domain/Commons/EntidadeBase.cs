using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.Commons;

public abstract class EntidadeBase
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTimeOffset DataCriacao { get; private set; } = DateTimeOffset.Now;
    public DateTimeOffset? DataAtualizacao { get; private set; }
    public bool Excluido { get; private set; } = false;
    public DateTimeOffset? DataExclusao { get; private set; }

    protected abstract void Validar();
    protected void Atualizar() => DataAtualizacao = DateTimeOffset.Now;
    public void Excluir()
    {
        Excluido = true;
        DataExclusao = DateTimeOffset.UtcNow;
    }
}
