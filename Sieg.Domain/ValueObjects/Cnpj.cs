using Sieg.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieg.Domain.ValueObjects;

public sealed record Cnpj
{
    public string Value { get; }

    public Cnpj(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DominioException("CNPJ não pode ser vazio.");

        var onlyDigits = new string(value.Where(char.IsDigit).ToArray());

        if (!Validar(onlyDigits))
            throw new DominioException("CNPJ inválido.");

        Value = onlyDigits; 
    }

    private bool Validar(string cnpj)
    {
        if (cnpj.Length != 14)
            return false;

        if (new string(cnpj[0], 14) == cnpj)
            return false;

        int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string cnpjSemDigitos = cnpj.Substring(0, 12);
        int soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(cnpjSemDigitos[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        cnpjSemDigitos += digito1;
        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += int.Parse(cnpjSemDigitos[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        string digitosInformados = cnpj.Substring(12, 2);
        string digitosCalculados = $"{digito1}{digito2}";

        return digitosInformados == digitosCalculados;
    }

    public string Formatado()
    {
        if (Value.Length != 14)
            return Value;

        return Convert.ToUInt64(Value).ToString(@"00\.000\.000\/0000\-00");
    }

    public override string ToString() => Formatado();
}

