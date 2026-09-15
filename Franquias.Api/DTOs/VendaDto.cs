using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Franquias.Api.DTOs
{
    public class ItemVendaDto
    {
        [Required]
        public int ProdutoId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }
    }

    public class CriarVendaDto
    {
        [Required]
        public int UnidadeId { get; set; }

        [Required]
        public List<ItemVendaDto> Itens { get; set; } = new();
    }

    public class VendaDto
    {
        public int Id { get; set; }
        public int UnidadeId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime DataVenda { get; set; }
        public decimal Total { get; set; }
        public List<ItemVendaDto> Itens { get; set; } = new();
    }
}
