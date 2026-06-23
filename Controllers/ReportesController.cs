using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LOGIN.Data;
using LOGIN.Models;

namespace LOGIN.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reportes/Dashboard
        public async Task<IActionResult> Dashboard(DateTime? desde, DateTime? hasta)
        {
            // Si no hay fechas, usar el mes actual
            if (!desde.HasValue)
                desde = DateTime.Now.AddDays(-30);
            if (!hasta.HasValue)
                hasta = DateTime.Now;

            ViewBag.Desde = desde.Value.ToString("dd/MM/yyyy");
            ViewBag.Hasta = hasta.Value.ToString("dd/MM/yyyy");

            // Obtener pedidos en el rango
            var pedidos = await _context.Pedidos
                .Include(p => p.Detalles!)
                .ThenInclude(d => d.Producto)
                .Where(p => p.FechaPedido >= desde.Value && p.FechaPedido <= hasta.Value)
                .ToListAsync();

            // Estadísticas
            var totalIngresos = pedidos.Sum(p => p.Total);
            var totalOrdenes = pedidos.Count;
            var itemsVendidos = pedidos.Sum(p => p.Detalles!.Sum(d => d.Cantidad));
            var ordenesConfirmadas = pedidos.Where(p => p.Estado == EstadoPedido.Confirmado).Count();

            ViewBag.TotalIngresos = totalIngresos;
            ViewBag.TotalOrdenes = totalOrdenes;
            ViewBag.ItemsVendidos = itemsVendidos;
            ViewBag.OrdenesConfirmadas = ordenesConfirmadas;

            // Top 5 productos más vendidos
            var topProductos = pedidos
                .SelectMany(p => p.Detalles!)
                .GroupBy(d => new { d.ProductoId, d.Producto!.Nombre })
                .Select(g => new
                {
                    Producto = g.Key.Nombre,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Recaudado = g.Sum(d => d.Cantidad * d.PrecioUnitario)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            ViewBag.TopProductos = topProductos;

            // Órdenes recientes
            var ordenesRecientes = pedidos
                .OrderByDescending(p => p.FechaPedido)
                .Take(10)
                .ToList();

            return View(ordenesRecientes);
        }

        // GET: Reportes/Index (vista anterior)
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // GET: Reportes/Productos
        public async Task<IActionResult> Productos()
        {
            var productos = await _context.Productos
                .OrderByDescending(p => p.Cantidad)
                .ToListAsync();

            var totalProductos = productos.Count;
            var valorTotalInventario = productos.Sum(p => p.Cantidad * p.Precio);
            var productosBajoStock = productos.Where(p => p.Cantidad < 5).Count();

            ViewBag.TotalProductos = totalProductos;
            ViewBag.ValorTotalInventario = valorTotalInventario;
            ViewBag.ProductosBajoStock = productosBajoStock;

            return View(productos);
        }

        // GET: Reportes/Compras
        public async Task<IActionResult> Compras()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles!)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            var totalVentas = pedidos.Sum(p => p.Total);
            var totalPedidos = pedidos.Count;
            var pedidosPendientes = pedidos.Where(p => p.Estado == EstadoPedido.Pendiente).Count();

            ViewBag.TotalVentas = totalVentas;
            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.PedidosPendientes = pedidosPendientes;

            return View(pedidos);
        }

        // GET: Reportes/Usuarios
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return View(usuarios);
        }

        // GET: Reportes/StockBajo
        public async Task<IActionResult> StockBajo()
        {
            var productos = await _context.Productos
                .Where(p => p.Cantidad < 5)
                .OrderBy(p => p.Cantidad)
                .ToListAsync();

            return View(productos);
        }
    }
}