using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LOGIN.Data;
using LOGIN.Models;
using Microsoft.Extensions.Caching.Memory;

namespace LOGIN.Controllers
{
    public class TiendaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public TiendaController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private bool UsuarioLogueado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        private int GetUsuarioId()
        {
            return int.Parse(HttpContext.Session.GetString("UsuarioId") ?? "0");
        }

        // GET: Tienda (vitrina de productos con paginación y caché)
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
        {
            if (!UsuarioLogueado())
                return RedirectToAction("Login", "Account");

            try
            {
                // 🔥 CACHÉ PARA PRODUCTOS (5 minutos)
                var cacheKey = "productos_lista";
                List<Producto> productos;
                int totalProductos;

                if (!_cache.TryGetValue(cacheKey, out productos))
                {
                    productos = await _context.Productos
                        .AsNoTracking()
                        .OrderBy(p => p.Nombre)
                        .ToListAsync();

                    _cache.Set(cacheKey, productos, TimeSpan.FromMinutes(5));
                }

                totalProductos = productos.Count;

                // Paginación manual
                var productosPaginados = productos
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // 🔥 CACHÉ PARA CARRITO (30 segundos)
                var carritoCountKey = $"carrito_count_{GetUsuarioId()}";
                int carritoCount;

                if (!_cache.TryGetValue(carritoCountKey, out carritoCount))
                {
                    carritoCount = await _context.CarritoItems
                        .Where(c => c.UsuarioId == GetUsuarioId())
                        .SumAsync(c => c.Cantidad);

                    _cache.Set(carritoCountKey, carritoCount, TimeSpan.FromSeconds(30));
                }

                ViewBag.CarritoCount = carritoCount;
                ViewBag.TotalProductos = totalProductos;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalProductos / pageSize);

                return View(productosPaginados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Tienda/Index: {ex.Message}");
                TempData["Error"] = "Error al cargar los productos. Intenta de nuevo.";
                return RedirectToAction("Carrito");
            }
        }

        // POST: Agregar al carrito
        [HttpPost]
        public async Task<IActionResult> AgregarAlCarrito(int productoId, int cantidad = 1)
        {
            if (!UsuarioLogueado())
                return Json(new { success = false, message = "Debes iniciar sesión" });

            var usuarioId = GetUsuarioId();
            var producto = await _context.Productos.FindAsync(productoId);

            if (producto == null)
                return Json(new { success = false, message = "Producto no encontrado" });

            if (producto.Cantidad < cantidad)
                return Json(new { success = false, message = "Stock insuficiente" });

            var itemExistente = await _context.CarritoItems
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.ProductoId == productoId);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                var carritoItem = new CarritoItem
                {
                    UsuarioId = usuarioId,
                    ProductoId = productoId,
                    Cantidad = cantidad
                };
                _context.CarritoItems.Add(carritoItem);
            }

            await _context.SaveChangesAsync();

            // 🔥 LIMPIAR CACHÉ DEL CARRITO
            _cache.Remove($"carrito_count_{usuarioId}");

            var totalItems = await _context.CarritoItems
                .Where(c => c.UsuarioId == usuarioId)
                .SumAsync(c => c.Cantidad);

            return Json(new { success = true, message = "Producto agregado", count = totalItems });
        }

        // GET: Carrito
        public async Task<IActionResult> Carrito()
        {
            if (!UsuarioLogueado())
                return RedirectToAction("Login", "Account");

            try
            {
                var usuarioId = GetUsuarioId();

                // 🔥 CACHÉ PARA EL CARRITO (30 segundos)
                var carritoKey = $"carrito_items_{usuarioId}";
                List<CarritoItem> carritoItems;

                if (!_cache.TryGetValue(carritoKey, out carritoItems))
                {
                    carritoItems = await _context.CarritoItems
                        .Include(c => c.Producto)
                        .Where(c => c.UsuarioId == usuarioId)
                        .AsNoTracking()
                        .ToListAsync();

                    _cache.Set(carritoKey, carritoItems, TimeSpan.FromSeconds(30));
                }

                var total = carritoItems.Sum(c => c.Cantidad * c.Producto!.Precio);
                ViewBag.Total = total;

                return View(carritoItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Tienda/Carrito: {ex.Message}");
                TempData["Error"] = "Error al cargar el carrito. Intenta de nuevo.";
                return RedirectToAction("Index");
            }
        }

        // POST: Actualizar cantidad
        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad(int itemId, int cantidad)
        {
            if (!UsuarioLogueado())
                return Json(new { success = false });

            var usuarioId = GetUsuarioId();
            var item = await _context.CarritoItems
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.Id == itemId && c.UsuarioId == usuarioId);

            if (item == null)
                return Json(new { success = false });

            if (cantidad <= 0)
            {
                _context.CarritoItems.Remove(item);
            }
            else
            {
                if (item.Producto != null && item.Producto.Cantidad < cantidad)
                    return Json(new { success = false, message = "Stock insuficiente" });

                item.Cantidad = cantidad;
            }

            await _context.SaveChangesAsync();

            // 🔥 LIMPIAR CACHÉ
            _cache.Remove($"carrito_items_{usuarioId}");
            _cache.Remove($"carrito_count_{usuarioId}");

            var carritoItems = await _context.CarritoItems
                .Include(c => c.Producto)
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            var total = carritoItems.Sum(c => c.Cantidad * c.Producto!.Precio);
            var totalItems = carritoItems.Sum(c => c.Cantidad);

            return Json(new
            {
                success = true,
                subtotal = item.Cantidad * (item.Producto?.Precio ?? 0),
                total = total,
                count = totalItems,
                itemRemoved = cantidad <= 0
            });
        }

        // GET: Checkout
        public async Task<IActionResult> Checkout()
        {
            if (!UsuarioLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = GetUsuarioId();

            // 🔥 USAR CACHÉ PARA EL CHECKOUT
            var carritoKey = $"carrito_items_{usuarioId}";
            List<CarritoItem> carritoItems;

            if (!_cache.TryGetValue(carritoKey, out carritoItems))
            {
                carritoItems = await _context.CarritoItems
                    .Include(c => c.Producto)
                    .Where(c => c.UsuarioId == usuarioId)
                    .AsNoTracking()
                    .ToListAsync();

                _cache.Set(carritoKey, carritoItems, TimeSpan.FromSeconds(30));
            }

            if (!carritoItems.Any())
                return RedirectToAction("Carrito");

            ViewBag.Total = carritoItems.Sum(c => c.Cantidad * c.Producto!.Precio);
            return View();
        }

        // POST: Confirmar pedido (OPTIMIZADO)
        [HttpPost]
        public async Task<IActionResult> ConfirmarPedido(string direccion, string metodoPago)
        {
            if (!UsuarioLogueado())
                return RedirectToAction("Login", "Account");

            var usuarioId = GetUsuarioId();

            // 🔥 OPTIMIZACIÓN: Obtener carrito con una sola consulta
            var carritoItems = await _context.CarritoItems
                .Include(c => c.Producto)
                .Where(c => c.UsuarioId == usuarioId)
                .AsNoTracking()
                .ToListAsync();

            if (!carritoItems.Any())
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction("Carrito");
            }

            // Verificar stock
            foreach (var item in carritoItems)
            {
                if (item.Producto == null || item.Producto.Cantidad < item.Cantidad)
                {
                    TempData["Error"] = $"Stock insuficiente para {item.Producto?.Nombre ?? "producto"}";
                    return RedirectToAction("Carrito");
                }
            }

            // Calcular total
            var total = carritoItems.Sum(c => c.Cantidad * c.Producto!.Precio);

            // Crear pedido
            var pedido = new Pedido
            {
                UsuarioId = usuarioId,
                Total = total,
                DireccionEnvio = direccion,
                MetodoPago = metodoPago,
                NumeroReferencia = "REF-" + DateTime.UtcNow.Ticks.ToString().Substring(0, 8),
                Estado = EstadoPedido.Confirmado
            };

            // 🔥 USAR TRANSACCIÓN PARA CONSISTENCIA
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                // Crear detalles y actualizar stock
                foreach (var item in carritoItems)
                {
                    var detalle = new PedidoDetalle
                    {
                        PedidoId = pedido.Id,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Producto!.Precio
                    };
                    _context.PedidoDetalles.Add(detalle);

                    // Actualizar stock
                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto != null)
                    {
                        producto.Cantidad -= item.Cantidad;
                        _context.Entry(producto).State = EntityState.Modified;
                    }
                }

                // Limpiar carrito
                _context.CarritoItems.RemoveRange(
                    _context.CarritoItems.Where(c => c.UsuarioId == usuarioId)
                );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 🔥 LIMPIAR CACHÉ
                _cache.Remove($"carrito_items_{usuarioId}");
                _cache.Remove($"carrito_count_{usuarioId}");

                TempData["Mensaje"] = "¡Pedido realizado con éxito!";
                return RedirectToAction("MisCompras");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error en ConfirmarPedido: {ex.Message}");
                TempData["Error"] = "Error al procesar el pedido. Intenta de nuevo.";
                return RedirectToAction("Carrito");
            }
        }

        // GET: Mis Compras
        public async Task<IActionResult> MisCompras()
        {
            if (!UsuarioLogueado())
                return RedirectToAction("Login", "Account");

            var pedidos = await _context.Pedidos
                .Include(p => p.Detalles!)
                .ThenInclude(d => d.Producto)
                .Where(p => p.UsuarioId == GetUsuarioId())
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return View(pedidos);
        }
    }
}