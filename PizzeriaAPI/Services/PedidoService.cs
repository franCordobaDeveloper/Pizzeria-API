using Microsoft.EntityFrameworkCore;
using PizzeriaAPI.Data;
using PizzeriaAPI.DTOs.Pedidos;
using PizzeriaAPI.Models;
using PizzeriaAPI.Services.Interfaces;

namespace PizzeriaAPI.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly PizzeriaContext _pizzeriaContext;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(PizzeriaContext pizzeriaContext, ILogger<PedidoService> logger)
        {
            _pizzeriaContext  = pizzeriaContext;
            _logger = logger;
        }

        public async Task<PedidoResponseDto?> ActualizarPedidoAsync(int id, PedidoRequestDto request, int usuarioId)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            if(usuarioId <= 0 || id <= 0)
            {
                _logger.LogWarning("Intento de actualizar un pedido con ID o usuario no válido: PedidoID={id}, UsuarioID={usuarioId}", id, usuarioId);
                throw new ArgumentException("El ID del pedido y el ID del usuario deben ser mayores a cero");
            }

            // 1. Verificar que existe y está activo
            var verificarPedido = await _pizzeriaContext.Pedidos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado == "activo");

            if (verificarPedido == null)
            {
                _logger.LogWarning("Intento de actualizar un pedido con ID no válido o no activo: {id}", id);
                throw new Exception("No se encontró un pedido activo con el ID proporcionado");
            }

            // Calcular el pedido con el costo del delivery
            var config = await _pizzeriaContext.Configuraciones.FirstOrDefaultAsync();
            var costoDelivery = config?.CostoDelivery ?? 0m;

            // Cargar todas las pizzas necesarias en un solo viaje
            var pizzasIds = request.Pizzas
                .SelectMany(p => new[] { p.PizzaMitad1Id, p.PizzaMitad2Id ?? 0 })
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var pizzasDict = await _pizzeriaContext.Pizzas
                .Where(p => pizzasIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // validar que todas las pizzas existan
            foreach (var pizzaId in pizzasIds)
            {
                if (!pizzasDict.ContainsKey(pizzaId))
                {
                    _logger.LogWarning("Intento de crear un pedido con una pizza no válida para el usuario {usuarioId}", usuarioId);
                    throw new ArgumentException("Una o más pizzas en el pedido no son válidas");
                }
            }

            decimal total = 0;
            var detalles = new List<DetallePedido>();

            foreach (var pizzaReq in request.Pizzas)
            {
                var pizza1 = pizzasDict[pizzaReq.PizzaMitad1Id];
                decimal precioUnitario = 0;

                switch (pizzaReq.Tipo)
                {
                    case "entera":
                        var pizza2Ent = pizzasDict[pizzaReq.PizzaMitad2Id!.Value];
                        precioUnitario = pizza1.PrecioEntera;
                        break;
                    case "combo":
                        var pizza2Combo = pizzasDict[pizzaReq.PizzaMitad2Id!.Value];
                        precioUnitario = pizza1.PrecioMitad + pizza2Combo.PrecioMitad;
                        break;
                    case "media":
                        precioUnitario = pizza1.PrecioMitad;
                        break;
                    default:
                        throw new Exception($"Tipo de pizza inválido: {pizzaReq.Tipo}");
                }

                var subtotal = precioUnitario * pizzaReq.Cantidad;
                total += subtotal;

                detalles.Add(new DetallePedido
                {
                    PizzaMitad1Id = pizzaReq.PizzaMitad1Id,
                    PizzaMitad2Id = pizzaReq.PizzaMitad2Id,
                    Tipo = pizzaReq.Tipo,
                    PrecioUnitario = precioUnitario,
                    Cantidad = pizzaReq.Cantidad
                });
            }

            if (request.Tipo == "delivery")
                total += costoDelivery;

            using var transaction = await _pizzeriaContext.Database.BeginTransactionAsync();
            try
            {
                // Eliminar detalles anteriores
                await _pizzeriaContext.DetallesPedido
                    .Where(d => d.PedidoId == id)
                    .ExecuteDeleteAsync();

                // Actualizar el pedido
                await _pizzeriaContext.Pedidos
                    .Where(p => p.Id == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.Tipo, request.Tipo)
                        .SetProperty(p => p.MetodoPago, request.MetodoPago)
                        .SetProperty(p => p.CostoDelivery, request.Tipo == "delivery" ? costoDelivery : 0)
                        .SetProperty(p => p.Total, total)
                        .SetProperty(p => p.Notas, request.Notas ?? string.Empty)
                    );

                // Insertar nuevos detalles
                await _pizzeriaContext.DetallesPedido.AddRangeAsync(detalles);
                await _pizzeriaContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return await ObtenerPedidoPorIdAsync(id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task<bool> CancelarPedidoAsync(int id)
        {
            if ( id <= 0)
            {
                _logger.LogWarning("Intento de cancelar un pedido con ID no válido: {id}", id);
                return false;
            }

            try
            {
                var actualizarPedido = await _pizzeriaContext.Pedidos
                    .Where(p => p.Id == id && p.Estado == "activo")
                    .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Estado, "cancelado"));

                if( actualizarPedido == 0)
                {
                    _logger.LogWarning("No se encontró un pedido activo con el ID {id} para cancelar", id);
                    return false;
                }

                _logger.LogInformation("Pedido con el ID {id} cancelado exitosamente", id);

                return true;
            }
            catch (Exception ex) 
            { 
                _logger.LogError(ex, "Error al cancelar el pedido con ID {id}", id);
                throw new Exception("Ocurrió un error al cancelar el pedido", ex);
          
            }
        }

        public async Task<PedidoResponseDto> CrearPedidoAsync(PedidoRequestDto request, int usuarioId)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if( request.Pizzas == null || request.Pizzas.Count == 0)
            {
                _logger.LogWarning("Intento de crear un pedido sin pizzas para el usuario {usuarioId}", usuarioId);
                throw new ArgumentException("El pedido debe contener al menos una pizza");
            }

            // Calcular el pedido con el costo del delivery
            var config = await _pizzeriaContext.Configuraciones.FirstOrDefaultAsync();
            var costoDelivery = config?.CostoDelivery ?? 0m;

            // Cargar todas las pizzas necesarias en un solo viaje
            var pizzasIds = request.Pizzas
                .SelectMany(p => new[] { p.PizzaMitad1Id, p.PizzaMitad2Id ?? 0})
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var pizzasDict = await _pizzeriaContext.Pizzas
                .Where(p => pizzasIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // validar que todas las pizzas existan
            foreach (var pizzaId in pizzasIds)
            {
                if (!pizzasDict.ContainsKey(pizzaId))
                {
                    _logger.LogWarning("Intento de crear un pedido con una pizza no válida para el usuario {usuarioId}", usuarioId);
                    throw new ArgumentException("Una o más pizzas en el pedido no son válidas");
                }
            }

            decimal total = 0;
            var detalles = new List<DetallePedido>();

            foreach (var pizzaReq in request.Pizzas)
            {
                var pizza1 = pizzasDict[pizzaReq.PizzaMitad1Id];
                decimal precioUnitario = 0;

                switch (pizzaReq.Tipo)
                {
                    case "entera":
                        var pizza2Ent = pizzasDict[pizzaReq.PizzaMitad2Id!.Value];
                        precioUnitario = pizza1.PrecioEntera;
                        break;
                    case "combo":
                        var pizza2Combo = pizzasDict[pizzaReq.PizzaMitad2Id!.Value];
                        precioUnitario = pizza1.PrecioMitad + pizza2Combo.PrecioMitad;
                        break;
                    case "media":
                        precioUnitario = pizza1.PrecioMitad;
                        break;
                    default:
                        throw new Exception($"Tipo de pizza inválido: {pizzaReq.Tipo}");
                }

                var subtotal = precioUnitario * pizzaReq.Cantidad;
                total += subtotal;

                detalles.Add(new DetallePedido
                {
                    PizzaMitad1Id = pizzaReq.PizzaMitad1Id,
                    PizzaMitad2Id = pizzaReq.PizzaMitad2Id,
                    Tipo = pizzaReq.Tipo,
                    PrecioUnitario = precioUnitario,
                    Cantidad = pizzaReq.Cantidad
                });
            }

            if (request.Tipo == "delivery")
                total += costoDelivery;

            var pedido = new Pedido
            {
                ClienteId = request.ClienteId,
                UsuarioId = usuarioId,
                Tipo = request.Tipo,
                Estado = "activo",
                MetodoPago = request.MetodoPago,
                CostoDelivery = request.Tipo == "delivery" ? costoDelivery : 0,
                Total = total,
                Notas = request.Notas ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                DetallesPedido = detalles
            };

            using var transaction = await _pizzeriaContext.Database.BeginTransactionAsync();
            try
            {
                await _pizzeriaContext.Pedidos.AddAsync(pedido);
                await _pizzeriaContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Pedido creado exitosamente - ID: {PedidoId}, Total: {Total}", pedido.Id, pedido.Total);
                return await ObtenerPedidoPorIdAsync(pedido.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PedidoResponseDto?> ObtenerPedidoPorIdAsync(int id)
        {
            try
            {
                var pedidosPorId = await _pizzeriaContext.Pedidos
                    .AsNoTracking()
                    .Where(p => p.Id == id)
                    .Select(p => new PedidoResponseDto
                    {
                        Id = p.Id,
                        NombreCliente = p.Cliente.Nombre,
                        Tipo = p.Tipo,
                        Estado = p.Estado,
                        MetodoPago = p.MetodoPago,
                        CostoDelivery = p.CostoDelivery,
                        Total = p.Total,
                        Notas = p.Notas,
                        CreatedAt = p.CreatedAt,
                        Pizzas = p.DetallesPedido.Select(d => new DetallePedidoResponseDto
                        {
                            PizzaMitad1 = d.PizzaMitad1.Nombre,
                            PizzaMitad2 = d.PizzaMitad2 != null ? d.PizzaMitad2.Nombre : null,
                            Tipo = d.Tipo,                          
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Se obtuvo un pedido con el ID {id}", pedidosPorId?.Id);
                return pedidosPorId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos activos");
                throw new Exception("Ocurrió un error al obtener los pedidos activos", ex);
            }
        }
        

        public async Task<List<PedidoResponseDto>> ObtenerPedidosActivosAsync()
        {
            try
            {
                var pedidosActivos = await _pizzeriaContext.Pedidos
                    .AsNoTracking()
                    .Where(p => p.Estado == "activo")
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new PedidoResponseDto
                    {
                        Id = p.Id,
                        NombreCliente = p.Cliente.Nombre,
                        Tipo = p.Tipo,
                        Estado = p.Estado,
                        MetodoPago = p.MetodoPago,
                        CostoDelivery = p.CostoDelivery,
                        Total = p.Total,
                        Notas = p.Notas,
                        CreatedAt = p.CreatedAt,
                        Pizzas = p.DetallesPedido.Select(d => new DetallePedidoResponseDto
                        {
                            PizzaMitad1 = d.PizzaMitad1.Nombre,
                            PizzaMitad2 = d.PizzaMitad2 != null ? d.PizzaMitad2.Nombre : null,
                            Tipo = d.Tipo,                          
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Subtotal
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("Se obtuvieron {Cantidad} pedidos activos", pedidosActivos.Count);
                return pedidosActivos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos activos. Detalles: {Message}", ex.Message);
                if (ex.InnerException != null)
                    _logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
                throw; // o lanza una excepción personalizada con el mensaje original
            }
        }
    }
}
