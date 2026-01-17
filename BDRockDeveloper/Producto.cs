namespace BDRockDeveloper
{
    public class Producto
    {
        public int? Id { get; set; }
        public string? Codigo { get; set; }

        public string? Nombre { get; set; }
        public double? Precio { get; set; }
        public Proveedor? Proveedor { get; set; }
        public string? Transaccion { get; set; }

    }
}
