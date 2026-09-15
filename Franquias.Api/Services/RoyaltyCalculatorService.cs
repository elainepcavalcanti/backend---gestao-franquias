using System;

namespace Franquias.Api.Services
{
    public interface IRoyaltyCalculatorService
    {
        decimal Calcular(decimal faturamento, decimal percentual);
    }

    public class RoyaltyCalculatorService : IRoyaltyCalculatorService
    {
        public decimal Calcular(decimal faturamento, decimal percentual)
        {
            if (faturamento < 0)
            {
                throw new ArgumentException(
                    "O faturamento não pode ser negativo.");
            }

            if (percentual < 0 || percentual > 100)
            {
                throw new ArgumentException(
                    "O percentual deve estar entre 0 e 100.");
            }

            var valor = faturamento * (percentual / 100m);

            return Math.Round(valor, 2);
        }
    }
}