using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LOGIN.Models
{
    public enum EstadoPedido
    {
        Pendiente,
        Confirmado,
        Enviado,
        Entregado,
        Cancelado
    }

    [Table("pedidos")]
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        // 🔥 CAMBIO: UtcNow
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        public decimal Total { get; set; }

        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [StringLength(200)]
        public string? DireccionEnvio { get; set; }

        [StringLength(50)]
        public string? MetodoPago { get; set; }

        [StringLength(20)]
        public string? NumeroReferencia { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<PedidoDetalle>? Detalles { get; set; }
    }
}