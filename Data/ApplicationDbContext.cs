using LOGIN.Models;
using Microsoft.EntityFrameworkCore;

namespace LOGIN.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<CarritoItem> CarritoItems { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoDetalle> PedidoDetalles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // CONFIGURACIÓN DE USUARIOS
        // ============================================
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);

            // Mapeo de columnas (C# → Supabase)
            entity.Property(e => e.Nombre).HasColumnName("nombre");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Edad).HasColumnName("edad");
            entity.Property(e => e.Ciudad).HasColumnName("ciudad");
            entity.Property(e => e.FechaRegistro).HasColumnName("fechaagregistro");
        });

        // ============================================
        // CONFIGURACIÓN DE PRODUCTOS
        // ============================================
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("productos");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombre).HasColumnName("nombre");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Precio).HasColumnName("precio");
            entity.Property(e => e.FechaRegistro).HasColumnName("fechaagregistro");
        });

        // ============================================
        // CONFIGURACIÓN DE CARRITOITEMS
        // ============================================
        modelBuilder.Entity<CarritoItem>(entity =>
        {
            entity.ToTable("carritoitems");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UsuarioId).HasColumnName("usuarioid");
            entity.Property(e => e.ProductoId).HasColumnName("productoid");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.FechaAgregado).HasColumnName("fechaagregado");

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================
        // CONFIGURACIÓN DE PEDIDOS
        // ============================================
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("pedidos");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UsuarioId).HasColumnName("usuarioid");
            entity.Property(e => e.FechaPedido).HasColumnName("fechapedido");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.DireccionEnvio).HasColumnName("direccionenvio");
            entity.Property(e => e.MetodoPago).HasColumnName("metodopago");
            entity.Property(e => e.NumeroReferencia).HasColumnName("numeroreferencia");

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Estado)
                .HasConversion<int>();
        });

        // ============================================
        // CONFIGURACIÓN DE PEDIDODETALLES
        // ============================================
        modelBuilder.Entity<PedidoDetalle>(entity =>
        {
            entity.ToTable("pedidodetalles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PedidoId).HasColumnName("pedidoid");
            entity.Property(e => e.ProductoId).HasColumnName("productoid");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PrecioUnitario).HasColumnName("preciounitario");

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                .WithMany()
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}