using Sieg.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.ValueObjects;

public sealed record Uf
{
    private static readonly HashSet<string> ValidStates = new(StringComparer.Ordinal)
    {
        "AC","AL","AM","AP","BA","CE","DF","ES","GO","MA","MG","MS","MT",
        "PA","PB","PE","PI","PR","RJ","RN","RO","RR","RS","SC","SE","SP","TO"
    };
    public string Value { get; }

    public Uf(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DominioException("UF não pode ser vazia.");

        var code = value.Trim().ToUpperInvariant();

        if (code.Length != 2 || !ValidStates.Contains(code))
            throw new DominioException($"UF inválida: {value}");

        Value = code;
    }
}


