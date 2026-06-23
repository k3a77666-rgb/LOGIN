using Microsoft.EntityFrameworkCore;
using LOGIN.Models;

namespace LOGIN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet de todas las tablas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔥 CONFIGURACIÓN GLOBAL: TODAS LAS FECHAS EN UTC
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetColumnType("timestamp with time zone");
                    }
                }
            }

            // ... resto de configuraciones (Usuario, Producto, etc.)
        }


        // ============================================
        // USUARIOS
        // ============================================
        modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).HasColumnName("password").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Edad).HasColumnName("edad").IsRequired();
                entity.Property(e => e.Ciudad).HasColumnName("ciudad").IsRequired().HasMaxLength(100);
                entity.Property(e => e.FechaRegistro).HasColumnName("fechaagregistro");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // ============================================
            // PRODUCTOS
            // ============================================
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("productos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
                entity.Property(e => e.Cantidad).HasColumnName("cantidad").IsRequired();
                entity.Property(e => e.Precio).HasColumnName("precio").IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.FechaRegistro).HasColumnName("fechaagregistro");
            });

            // ============================================
            // CARRITOITEMS
            // ============================================
            modelBuilder.Entity<CarritoItem>(entity =>
            {
                entity.ToTable("carritoitems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
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
            // PEDIDOS
            // ============================================
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("pedidos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UsuarioId).HasColumnName("usuarioid");
                entity.Property(e => e.FechaPedido).HasColumnName("fechapedido");
                entity.Property(e => e.Total).HasColumnName("total").HasColumnType("decimal(18,2)");
                entity.Property(e => e.Estado).HasColumnName("estado");
                entity.Property(e => e.DireccionEnvio).HasColumnName("direccionenvio").HasMaxLength(200);
                entity.Property(e => e.MetodoPago).HasColumnName("metodopago").HasMaxLength(50);
                entity.Property(e => e.NumeroReferencia).HasColumnName("numeroreferencia").HasMaxLength(20);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Estado)
                    .HasConversion<int>();
            });

            // ============================================
            // PEDIDODETALLES
            // ============================================
            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.ToTable("pedidodetalles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PedidoId).HasColumnName("pedidoid");
                entity.Property(e => e.ProductoId).HasColumnName("productoid");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.PrecioUnitario).HasColumnName("preciounitario").HasColumnType("decimal(18,2)");

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
}