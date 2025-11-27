namespace SaboresdeMama.Models
{
    public class CarritoItem
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }

        public double Total => Cantidad * Producto.Precio;
    }
}

