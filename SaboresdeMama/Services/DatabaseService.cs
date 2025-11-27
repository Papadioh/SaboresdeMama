using SaboresdeMama.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;

namespace SaboresdeMama.Services
{
    public class DatabaseService
    {
        private bool _initialized = false;
        private static readonly List<Producto> _productosLocales = new();

        public DatabaseService()
        {
        }

        private async Task InitializeAsync()
        {
            if (_initialized)
                return;

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                System.Diagnostics.Debug.WriteLine("Firebase no está configurado. Usando modo local.");
                _initialized = true;
                return;
            }

            try
            {
                await InitializeDefaultUsersAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando Firebase: {ex.Message}");
            }

            _initialized = true;
        }

        public async Task<string> UploadImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                System.Diagnostics.Debug.WriteLine("Firebase no configurado. Subida de imagen cancelada.");
                return string.Empty;
            }

            try
            {
                await Task.Delay(1000);
                return $"https://saboresdemama.com/storage/{Path.GetFileName(imagePath)}?t={DateTime.Now.Ticks}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error subiendo imagen: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task AddUsuarioAsync(Usuario usuario)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                System.Diagnostics.Debug.WriteLine("Firebase no configurado. No se puede registrar usuario remotamente.");
                usuario.Id = Guid.NewGuid().ToString();
                return;
            }

            try
            {
                var dict = new Dictionary<string, object>
                {
                    { "Nombre", usuario.Nombre ?? "" },
                    { "Username", usuario.Username ?? "" },
                    { "Password", usuario.Password ?? "" },
                    { "TipoUsuario", usuario.TipoUsuario ?? "Cliente" },
                    { "Telefono", usuario.Telefono ?? "" },
                    { "Direccion", usuario.Direccion ?? "" },
                    { "FechaRegistro", usuario.FechaRegistro == default ? DateTime.Now : usuario.FechaRegistro }
                };

                usuario.Id = await FirebaseService.AddDocumentAsync("usuarios", dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registrando usuario: {ex.Message}");
                throw;
            }
        }

        public async Task<Usuario> GetUsuarioByUsernameAsync(string username)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return null;
            }

            try
            {
                var usuarios = await FirebaseService.GetCollectionAsync<Usuario>("usuarios");
                return usuarios.FirstOrDefault(u => u.Username == username);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo usuario de Firebase: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateUsuarioAsync(Usuario usuario)
        {
            await InitializeAsync();

            if (usuario == null || string.IsNullOrEmpty(usuario.Id))
            {
                System.Diagnostics.Debug.WriteLine("UpdateUsuarioAsync: usuario inválido.");
                return;
            }

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                System.Diagnostics.Debug.WriteLine("Firebase no configurado. No se puede actualizar usuario remotamente.");
                return;
            }

            try
            {
                var dict = new Dictionary<string, object>
                {
                    { "Nombre", usuario.Nombre ?? "" },
                    { "Username", usuario.Username ?? "" },
                    { "Password", usuario.Password ?? "" },
                    { "TipoUsuario", usuario.TipoUsuario ?? "Cliente" },
                    { "Telefono", usuario.Telefono ?? "" },
                    { "Direccion", usuario.Direccion ?? "" },
                    { "FechaRegistro", usuario.FechaRegistro == default ? DateTime.Now : usuario.FechaRegistro }
                };

                await FirebaseService.UpdateDocumentAsync("usuarios", usuario.Id, dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando usuario: {ex.Message}");
                throw;
            }
        }

        private async Task InitializeDefaultUsersAsync()
        {
            try
            {
                var usuarios = await FirebaseService.GetCollectionAsync<Usuario>("usuarios");

                if (usuarios.Count == 0)
                {
                    var adminDict = new Dictionary<string, object>
                    {
                        { "Username", "admin" },
                        { "Password", "admin123" },
                        { "TipoUsuario", "Admin" },
                        { "Nombre", "Administrador" }
                    };
                    await FirebaseService.AddDocumentAsync("usuarios", adminDict);

                    var clienteDict = new Dictionary<string, object>
                    {
                        { "Username", "cliente" },
                        { "Password", "cliente123" },
                        { "TipoUsuario", "Cliente" },
                        { "Nombre", "Cliente" }
                    };
                    await FirebaseService.AddDocumentAsync("usuarios", clienteDict);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando usuarios por defecto: {ex.Message}");
            }
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            await InitializeAsync();
            return await FirebaseService.GetCollectionAsync<Usuario>("usuarios");
        }

        public async Task<List<Pedido>> GetPedidosAsync()
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return new List<Pedido>();
            }

            try
            {
                var allPedidos = await FirebaseService.GetCollectionAsync<Pedido>("pedidos");
                return allPedidos.OrderByDescending(p => p.FechaCreacion).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo pedidos: {ex.Message}");
                return new List<Pedido>();
            }
        }

        public async Task<List<Pedido>> GetAllPedidosAsync()
        {
            return await GetPedidosAsync();
        }

        public async Task<Pedido> GetPedidoByIdAsync(string id)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return null;
            }

            try
            {
                return await FirebaseService.GetDocumentAsync<Pedido>("pedidos", id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo pedido por ID: {ex.Message}");
                return null;
            }
        }

        public async Task UpdatePedidoAsync(Pedido pedido)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return;
            }

            try
            {
                var dict = new Dictionary<string, object>
                {
                    { "DescripcionProducto", pedido.DescripcionProducto ?? "" },
                    { "ClienteNombre", pedido.ClienteNombre ?? "" },
                    { "ClienteId", pedido.ClienteId ?? "" },
                    { "ClienteTelefono", pedido.ClienteTelefono ?? "" },
                    { "ClienteDireccion", pedido.ClienteDireccion ?? "" },
                    { "FechaEntrega", pedido.FechaEntrega },
                    { "FechaCreacion", pedido.FechaCreacion },
                    { "Estado", pedido.Estado ?? "" },
                    { "Total", pedido.Total },
                    { "DetallesPedido", pedido.DetallesPedido ?? "" },
                    { "RecetaId", pedido.RecetaId ?? "" }
                };
                await FirebaseService.UpdateDocumentAsync("pedidos", pedido.Id, dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando pedido: {ex.Message}");
                throw;
            }
        }

        public async Task AddPedidoAsync(Pedido pedido)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                pedido.Id = Guid.NewGuid().ToString();
                return;
            }

            try
            {
                var dict = new Dictionary<string, object>
                {
                    { "DescripcionProducto", pedido.DescripcionProducto ?? "" },
                    { "ClienteNombre", pedido.ClienteNombre ?? "" },
                    { "ClienteId", pedido.ClienteId ?? "" },
                    { "ClienteTelefono", pedido.ClienteTelefono ?? "" },
                    { "ClienteDireccion", pedido.ClienteDireccion ?? "" },
                    { "FechaEntrega", pedido.FechaEntrega },
                    { "FechaCreacion", pedido.FechaCreacion },
                    { "Estado", pedido.Estado ?? "" },
                    { "Total", pedido.Total },
                    { "DetallesPedido", pedido.DetallesPedido ?? "" },
                    { "RecetaId", pedido.RecetaId ?? "" }
                };
                pedido.Id = await FirebaseService.AddDocumentAsync("pedidos", dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando pedido: {ex.Message}");
                throw;
            }
        }

        public async Task DeletePedidoAsync(Pedido pedido)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return;
            }

            try
            {
                await FirebaseService.DeleteDocumentAsync("pedidos", pedido.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando pedido: {ex.Message}");
                throw;
            }
        }

        public async Task AddProductoAsync(Producto producto)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                producto.Id = Guid.NewGuid().ToString();
                producto.FechaCreacion = DateTime.Now;
                _productosLocales.Add(producto);
                return;
            }

            try
            {
                producto.FechaCreacion = DateTime.Now;
                var dict = new Dictionary<string, object>
                {
                    { "Nombre", producto.Nombre ?? "" },
                    { "Descripcion", producto.Descripcion ?? "" },
                    { "Precio", producto.Precio },
                    { "Stock", producto.Stock },
                    { "InformacionExtra", producto.InformacionExtra ?? "" },
                    { "Categoria", producto.Categoria ?? "" },
                    { "FechaCreacion", producto.FechaCreacion },
                    { "ImageURL", producto.ImageURL ?? "" } // <-- Guardar URL
                };
                producto.Id = await FirebaseService.AddDocumentAsync("productos", dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando producto en Firebase: {ex.Message}");
                if (string.IsNullOrEmpty(producto.Id))
                {
                    producto.Id = Guid.NewGuid().ToString();
                }
                if (producto.FechaCreacion == default)
                {
                    producto.FechaCreacion = DateTime.Now;
                }
                _productosLocales.Add(producto);
            }
        }

        public async Task<List<Producto>> GetProductosAsync()
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                foreach (var producto in _productosLocales)
                {
                    if (string.IsNullOrEmpty(producto.Id))
                    {
                        producto.Id = Guid.NewGuid().ToString();
                        System.Diagnostics.Debug.WriteLine($"ID generado para producto local: {producto.Nombre} -> {producto.Id}");
                    }
                }
                return _productosLocales.OrderBy(p => p.Nombre).ToList();
            }

            try
            {
                var productos = await FirebaseService.GetCollectionAsync<Producto>("productos");
                foreach (var producto in productos)
                {
                    if (string.IsNullOrEmpty(producto.Id))
                    {
                        System.Diagnostics.Debug.WriteLine($"ADVERTENCIA: Producto de Firebase sin ID: {producto.Nombre}");
                    }
                }
                return productos.OrderBy(p => p.Nombre).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo productos: {ex.Message}");
                // Asegurar que todos los productos locales tengan ID
                foreach (var producto in _productosLocales)
                {
                    if (string.IsNullOrEmpty(producto.Id))
                    {
                        producto.Id = Guid.NewGuid().ToString();
                    }
                }
                return _productosLocales.OrderBy(p => p.Nombre).ToList();
            }
        }

        public async Task<List<Producto>> GetProductosDisponiblesAsync()
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                foreach (var producto in _productosLocales)
                {
                    if (string.IsNullOrEmpty(producto.Id))
                    {
                        producto.Id = Guid.NewGuid().ToString();
                        System.Diagnostics.Debug.WriteLine($"ID generado para producto local: {producto.Nombre} -> {producto.Id}");
                    }
                }
                return _productosLocales
                    .Where(p => p.Stock > 0)
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }

            try
            {
                var productos = await FirebaseService.GetCollectionAsync<Producto>("productos");
                foreach (var producto in productos)
                {
                    if (string.IsNullOrEmpty(producto.Id))
                    {
                        System.Diagnostics.Debug.WriteLine($"ADVERTENCIA: Producto de Firebase sin ID: {producto.Nombre}");
                    }
                }
                return productos
                    .Where(p => p.Stock > 0)
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo productos disponibles: {ex.Message}");
                // Asegurar que todos los productos locales tengan ID
                foreach (var producto in _productosLocales)
                {
                    if (string.IsNullOrEmpty(producto.Id))
                    {
                        producto.Id = Guid.NewGuid().ToString();
                    }
                }
                return _productosLocales
                    .Where(p => p.Stock > 0)
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }
        }

        public async Task<List<Producto>> SearchProductosAsync(string searchTerm)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                var query = _productosLocales.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lower = searchTerm.ToLower();
                    query = query.Where(p =>
                        (p.Nombre ?? string.Empty).ToLower().Contains(lower) ||
                        (p.Descripcion ?? string.Empty).ToLower().Contains(lower));
                }
                return query.OrderBy(p => p.Nombre).ToList();
            }

            try
            {
                var allProductos = await FirebaseService.GetCollectionAsync<Producto>("productos");

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return allProductos.OrderBy(p => p.Nombre).ToList();
                }

                var lowerSearchTerm = searchTerm.ToLower();
                return allProductos
                    .Where(p => p.Nombre?.ToLower().Contains(lowerSearchTerm) == true ||
                               p.Descripcion?.ToLower().Contains(lowerSearchTerm) == true)
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando productos: {ex.Message}");
                var query = _productosLocales.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lower = searchTerm.ToLower();
                    query = query.Where(p =>
                        (p.Nombre ?? string.Empty).ToLower().Contains(lower) ||
                        (p.Descripcion ?? string.Empty).ToLower().Contains(lower));
                }
                return query.OrderBy(p => p.Nombre).ToList();
            }
        }

        public async Task<Producto> GetProductoByIdAsync(string id)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return _productosLocales.FirstOrDefault(p => p.Id == id);
            }

            try
            {
                return await FirebaseService.GetDocumentAsync<Producto>("productos", id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo producto por ID: {ex.Message}");
                return _productosLocales.FirstOrDefault(p => p.Id == id);
            }
        }

        public async Task UpdateProductoAsync(Producto producto)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                var existente = _productosLocales.FirstOrDefault(p => p.Id == producto.Id);
                if (existente != null)
                {
                    existente.Nombre = producto.Nombre;
                    existente.Descripcion = producto.Descripcion;
                    existente.Precio = producto.Precio;
                    existente.Stock = producto.Stock;
                    existente.InformacionExtra = producto.InformacionExtra;
                    existente.Categoria = producto.Categoria;
                    existente.FechaCreacion = producto.FechaCreacion;
                    existente.ImageURL = producto.ImageURL;
                }
                return;
            }

            try
            {
                var dict = new Dictionary<string, object>
                {
                    { "Nombre", producto.Nombre ?? "" },
                    { "Descripcion", producto.Descripcion ?? "" },
                    { "Precio", producto.Precio },
                    { "Stock", producto.Stock },
                    { "InformacionExtra", producto.InformacionExtra ?? "" },
                    { "Categoria", producto.Categoria ?? "" },
                    { "FechaCreacion", producto.FechaCreacion },
                    { "ImageURL", producto.ImageURL ?? "" } // <-- Guardar URL
                };
                await FirebaseService.UpdateDocumentAsync("productos", producto.Id, dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando producto: {ex.Message}");
                var existente = _productosLocales.FirstOrDefault(p => p.Id == producto.Id);
                if (existente != null)
                {
                    existente.Nombre = producto.Nombre;
                    existente.Descripcion = producto.Descripcion;
                    existente.Precio = producto.Precio;
                    existente.Stock = producto.Stock;
                    existente.InformacionExtra = producto.InformacionExtra;
                    existente.Categoria = producto.Categoria;
                    existente.FechaCreacion = producto.FechaCreacion;
                    existente.ImageURL = producto.ImageURL;
                }
            }
        }

        public async Task DeleteProductoAsync(Producto producto)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                _productosLocales.RemoveAll(p => p.Id == producto.Id);
                return;
            }

            try
            {
                await FirebaseService.DeleteDocumentAsync("productos", producto.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando producto: {ex.Message}");
                _productosLocales.RemoveAll(p => p.Id == producto.Id);
            }
        }

        public async Task<bool> ActualizarStockAsync(string productoId, int cantidad)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(productoId))
            {
                System.Diagnostics.Debug.WriteLine("ActualizarStockAsync: productoId es nulo o vacío");
                return false;
            }

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                var local = _productosLocales.FirstOrDefault(p => p.Id == productoId);
                if (local != null)
                {
                    var stockAnterior = local.Stock;
                    local.Stock += cantidad;
                    if (local.Stock < 0) local.Stock = 0;
                    System.Diagnostics.Debug.WriteLine($"Stock actualizado (local): {local.Nombre} - Anterior: {stockAnterior}, Cambio: {cantidad}, Nuevo: {local.Stock}");
                    return true;
                }
                System.Diagnostics.Debug.WriteLine($"ActualizarStockAsync: No se encontró producto local con ID: {productoId}");
                return false;
            }

            try
            {
                var producto = await GetProductoByIdAsync(productoId);
                if (producto != null)
                {
                    var stockAnterior = producto.Stock;
                    producto.Stock += cantidad;
                    if (producto.Stock < 0) producto.Stock = 0;
                    
                    System.Diagnostics.Debug.WriteLine($"Stock actualizado (Firebase): {producto.Nombre} - Anterior: {stockAnterior}, Cambio: {cantidad}, Nuevo: {producto.Stock}");
                    
                    await UpdateProductoAsync(producto);
                    return true;
                }
                System.Diagnostics.Debug.WriteLine($"ActualizarStockAsync: No se encontró producto en Firebase con ID: {productoId}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ActualizarStockAsync: {ex.Message}");
                return false;
            }
        }

        public async Task AddRecetaAsync(Receta receta)
        {
            await InitializeAsync();
            var dict = new Dictionary<string, object>
            {
                { "Nombre", receta.Nombre ?? "" },
                { "Ingredientes", receta.Ingredientes ?? "" },
                { "Procedimiento", receta.Procedimiento ?? "" },
                { "InsumosNecesariosJson", receta.InsumosNecesariosJson ?? "" }
            };
            receta.Id = await FirebaseService.AddDocumentAsync("recetas", dict);
        }

        public async Task<List<Receta>> GetRecetasAsync()
        {
            await InitializeAsync();
            var recetas = await FirebaseService.GetCollectionAsync<Receta>("recetas");
            return recetas.OrderBy(r => r.Nombre).ToList();
        }

        public async Task<List<Receta>> SearchRecetasAsync(string searchTerm)
        {
            await InitializeAsync();
            var allRecetas = await FirebaseService.GetCollectionAsync<Receta>("recetas");

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return allRecetas.OrderBy(r => r.Nombre).ToList();
            }

            var lowerSearchTerm = searchTerm.ToLower();
            return allRecetas
                .Where(r => r.Nombre?.ToLower().Contains(lowerSearchTerm) == true)
                .OrderBy(r => r.Nombre)
                .ToList();
        }

        public async Task DeleteRecetaAsync(Receta receta)
        {
            await InitializeAsync();
            await FirebaseService.DeleteDocumentAsync("recetas", receta.Id);
        }

        public async Task UpdateRecetaAsync(Receta receta)
        {
            await InitializeAsync();
            var dict = new Dictionary<string, object>
            {
                { "Nombre", receta.Nombre ?? "" },
                { "Ingredientes", receta.Ingredientes ?? "" },
                { "Procedimiento", receta.Procedimiento ?? "" },
                { "InsumosNecesariosJson", receta.InsumosNecesariosJson ?? "" }
            };
            await FirebaseService.UpdateDocumentAsync("recetas", receta.Id, dict);
        }

        public async Task AddVentaAsync(Venta venta)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                venta.Id = Guid.NewGuid().ToString();
                return;
            }

            try
            {
                var dict = new Dictionary<string, object>
                {
                    { "PedidoId", venta.PedidoId ?? string.Empty },
                    { "DescripcionProducto", venta.DescripcionProducto ?? "" },
                    { "ClienteNombre", venta.ClienteNombre ?? "" },
                    { "ClienteTelefono", venta.ClienteTelefono ?? "" },
                    { "ClienteDireccion", venta.ClienteDireccion ?? "" },
                    { "Total", venta.Total },
                    { "FechaCompletado", venta.FechaCompletado }
                };
                venta.Id = await FirebaseService.AddDocumentAsync("ventas", dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando venta: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Venta>> GetVentasAsync()
        {
            await InitializeAsync();
            var ventas = await FirebaseService.GetCollectionAsync<Venta>("ventas");
            return ventas.OrderByDescending(v => v.FechaCompletado).ToList();
        }

        public async Task<Venta> GetVentaByPedidoIdAsync(string pedidoId)
        {
            await InitializeAsync();
            if (string.IsNullOrEmpty(pedidoId))
                return null;

            var ventas = await FirebaseService.GetCollectionAsync<Venta>("ventas");
            return ventas.FirstOrDefault(v => v.PedidoId == pedidoId);
        }

        public async Task DeleteVentaAsync(Venta venta)
        {
            await InitializeAsync();
            await FirebaseService.DeleteDocumentAsync("ventas", venta.Id);
        }

        // ======================================================
        // ===== MÉTODOS PARA GESTIÓN DE INSUMOS =====
        // ======================================================

        public async Task AddInsumoAsync(Insumo insumo)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                insumo.Id = Guid.NewGuid().ToString();
                insumo.FechaCreacion = DateTime.Now;
                return;
            }

            try
            {
                insumo.FechaCreacion = DateTime.Now;
                var dict = new Dictionary<string, object>
                {
                    { "Nombre", insumo.Nombre ?? "" },
                    { "Cantidad", insumo.Cantidad },
                    { "Unidad", insumo.Unidad ?? "" },
                    { "FechaCreacion", insumo.FechaCreacion }
                };
                insumo.Id = await FirebaseService.AddDocumentAsync("insumos", dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando insumo en Firebase: {ex.Message}");
                if (string.IsNullOrEmpty(insumo.Id))
                {
                    insumo.Id = Guid.NewGuid().ToString();
                }
                if (insumo.FechaCreacion == default)
                {
                    insumo.FechaCreacion = DateTime.Now;
                }
            }
        }

        public async Task<List<Insumo>> GetInsumosAsync()
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return new List<Insumo>();
            }

            try
            {
                var insumos = await FirebaseService.GetCollectionAsync<Insumo>("insumos");
                return insumos.OrderBy(i => i.Nombre).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo insumos: {ex.Message}");
                return new List<Insumo>();
            }
        }

        public async Task<Insumo> GetInsumoByIdAsync(string id)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return null;
            }

            try
            {
                return await FirebaseService.GetDocumentAsync<Insumo>("insumos", id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo insumo por ID: {ex.Message}");
                return null;
            }
        }

        public async Task<Insumo> GetInsumoByNombreAsync(string nombre)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return null;
            }

            try
            {
                var insumos = await FirebaseService.GetCollectionAsync<Insumo>("insumos");
                return insumos.FirstOrDefault(i => i.Nombre?.ToLower() == nombre?.ToLower());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo insumo por nombre: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateInsumoAsync(Insumo insumo)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return;
            }

            try
            {
                var dict = new Dictionary<string, object>
                {
                    { "Nombre", insumo.Nombre ?? "" },
                    { "Cantidad", insumo.Cantidad },
                    { "Unidad", insumo.Unidad ?? "" },
                    { "FechaCreacion", insumo.FechaCreacion }
                };
                await FirebaseService.UpdateDocumentAsync("insumos", insumo.Id, dict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando insumo: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteInsumoAsync(Insumo insumo)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return;
            }

            try
            {
                await FirebaseService.DeleteDocumentAsync("insumos", insumo.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando insumo: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ActualizarCantidadInsumoAsync(string insumoId, double cantidad)
        {
            await InitializeAsync();

            if (string.IsNullOrEmpty(insumoId))
            {
                System.Diagnostics.Debug.WriteLine("ActualizarCantidadInsumoAsync: insumoId es nulo o vacío");
                return false;
            }

            if (string.IsNullOrEmpty(FirebaseService.ProjectId) || FirebaseService.ProjectId == "TU_PROJECT_ID")
            {
                return false;
            }

            try
            {
                var insumo = await GetInsumoByIdAsync(insumoId);
                if (insumo != null)
                {
                    var cantidadAnterior = insumo.Cantidad;
                    insumo.Cantidad += cantidad;
                    if (insumo.Cantidad < 0) insumo.Cantidad = 0;
                    
                    System.Diagnostics.Debug.WriteLine($"Cantidad de insumo actualizada: {insumo.Nombre} - Anterior: {cantidadAnterior}, Cambio: {cantidad}, Nuevo: {insumo.Cantidad}");
                    
                    await UpdateInsumoAsync(insumo);
                    return true;
                }
                System.Diagnostics.Debug.WriteLine($"ActualizarCantidadInsumoAsync: No se encontró insumo con ID: {insumoId}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ActualizarCantidadInsumoAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string Message)> CompletarPedidoConVentaAsync(Pedido pedido)
        {
            await InitializeAsync();

            if (pedido == null)
                return (false, "Pedido inválido.");

            if (pedido.Estado == "Completado")
                return (false, "El pedido ya está marcado como completado.");

            Receta recetaAsociada = null;
            try
            {
                var recetas = await GetRecetasAsync();
                if (!string.IsNullOrEmpty(pedido.RecetaId))
                {
                    recetaAsociada = recetas.FirstOrDefault(r => r.Id == pedido.RecetaId);
                }

                if (recetaAsociada == null && !string.IsNullOrEmpty(pedido.DescripcionProducto))
                {
                    var desc = pedido.DescripcionProducto.ToLower();
                    recetaAsociada = recetas.FirstOrDefault(r =>
                        r.Nombre?.ToLower().Contains(desc) == true ||
                        desc.Contains(r.Nombre?.ToLower() ?? string.Empty));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error buscando receta para pedido: {ex.Message}");
            }

            if (recetaAsociada != null && recetaAsociada.InsumosNecesarios != null && recetaAsociada.InsumosNecesarios.Count > 0)
            {
                var (disponible, faltantes) = await VerificarDisponibilidadInsumosAsync(recetaAsociada);
                if (!disponible)
                {
                    var faltantesTexto = string.Join("\n", faltantes);
                    return (false, $"No se puede completar el pedido. Faltan insumos:\n{faltantesTexto}");
                }

                var restado = await RestarInsumosDeRecetaAsync(recetaAsociada);
                if (!restado)
                {
                    return (false, "No se pudieron restar los insumos del inventario.");
                }
            }

            pedido.Estado = "Completado";
            await UpdatePedidoAsync(pedido);

            var venta = new Venta
            {
                PedidoId = pedido.Id,
                DescripcionProducto = pedido.DescripcionProducto,
                ClienteNombre = pedido.ClienteNombre,
                ClienteTelefono = pedido.ClienteTelefono,
                ClienteDireccion = pedido.ClienteDireccion,
                Total = pedido.Total,
                FechaCompletado = DateTime.Now
            };
            await AddVentaAsync(venta);

            return (true, "Pedido completado y registrado en ventas.");
        }

        public async Task<(bool Success, string Message)> RevertirPedidoAAceptadoAsync(Pedido pedido)
        {
            await InitializeAsync();

            if (pedido == null)
                return (false, "Pedido inválido.");

            if (pedido.Estado != "Completado")
                return (false, "Solo los pedidos completados pueden marcarse como no completados.");

            pedido.Estado = "Aceptado";
            await UpdatePedidoAsync(pedido);

            var venta = await GetVentaByPedidoIdAsync(pedido.Id);
            if (venta != null)
            {
                await DeleteVentaAsync(venta);
            }

            return (true, "El pedido volvió al estado 'Aceptado'.");
        }

        // Verificar si hay suficientes insumos para una receta
        public async Task<(bool disponible, List<string> insumosFaltantes)> VerificarDisponibilidadInsumosAsync(Receta receta)
        {
            await InitializeAsync();

            if (receta == null || receta.InsumosNecesarios == null || receta.InsumosNecesarios.Count == 0)
            {
                return (true, new List<string>()); // Si no tiene insumos definidos, se considera disponible
            }

            var todosInsumos = await GetInsumosAsync();
            var insumosFaltantes = new List<string>();

            foreach (var insumoReceta in receta.InsumosNecesarios)
            {
                var insumo = todosInsumos.FirstOrDefault(i => i.Id == insumoReceta.InsumoId || i.Nombre == insumoReceta.InsumoNombre);
                
                if (insumo == null)
                {
                    insumosFaltantes.Add($"{insumoReceta.InsumoNombre} (no existe en inventario)");
                }
                else if (insumo.Cantidad < insumoReceta.CantidadNecesaria)
                {
                    insumosFaltantes.Add($"{insumoReceta.InsumoNombre} (necesita {insumoReceta.CantidadNecesaria} {insumoReceta.Unidad}, hay {insumo.Cantidad} {insumo.Unidad})");
                }
            }

            return (insumosFaltantes.Count == 0, insumosFaltantes);
        }

        // Restar insumos del inventario cuando se completa un pedido
        public async Task<bool> RestarInsumosDeRecetaAsync(Receta receta)
        {
            await InitializeAsync();

            if (receta == null || receta.InsumosNecesarios == null || receta.InsumosNecesarios.Count == 0)
            {
                return true; // Si no tiene insumos, no hay nada que restar
            }

            var todosInsumos = await GetInsumosAsync();
            var errores = new List<string>();

            foreach (var insumoReceta in receta.InsumosNecesarios)
            {
                var insumo = todosInsumos.FirstOrDefault(i => i.Id == insumoReceta.InsumoId || i.Nombre == insumoReceta.InsumoNombre);
                
                if (insumo == null)
                {
                    errores.Add($"Insumo '{insumoReceta.InsumoNombre}' no encontrado");
                    continue;
                }

                if (insumo.Cantidad < insumoReceta.CantidadNecesaria)
                {
                    errores.Add($"Insumo '{insumoReceta.InsumoNombre}' no tiene suficiente cantidad");
                    continue;
                }

                // Restar la cantidad
                await ActualizarCantidadInsumoAsync(insumo.Id, -insumoReceta.CantidadNecesaria);
            }

            if (errores.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Errores al restar insumos: {string.Join(", ", errores)}");
                return false;
            }

            return true;
        }
    }
}