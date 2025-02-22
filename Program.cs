using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Pedido
{
    public List<Producto> Productos { get; set; }
}

public class Producto
{
    public string Nombre { get; set; }
    public int Cantidad { get; set; }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        Pedido pedido = new Pedido
        {
            Productos = new List<Producto>
            {
                new Producto { Nombre = "Camiseta", Cantidad = 2 },
                new Producto { Nombre = "Pantalón", Cantidad = 1 },
                new Producto { Nombre = "Zapatos", Cantidad = 1 }
            }
        };

        await ProcesarPedidoAsync(pedido);
    }

    public static async Task ProcesarPedidoAsync(Pedido pedido)
    {
        List<Task> tareas = new List<Task>();

        foreach (Producto producto in pedido.Productos)
        {
            var tareaPadre = Task.Factory.StartNew(() =>
            {
                var tareaHija = Task.Factory.StartNew(async () =>
                {
                    await VerificarStockAsync(producto);
                    decimal precio = await CalcularPrecioAsync(producto);
                    await EnviarALogisticaAsync(producto, precio);
                }, TaskCreationOptions.AttachedToParent).Unwrap();

                tareaHija.ContinueWith(t =>
                {
                    Console.WriteLine($"Producto {producto.Nombre} procesado correctamente.");
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

                tareaHija.ContinueWith(t =>
                {
                    Console.WriteLine($"Error al procesar {producto.Nombre}: {t.Exception?.InnerException?.Message}");
                }, TaskContinuationOptions.OnlyOnCanceled);

                return tareaHija;
            }).Unwrap();

            tareas.Add(tareaPadre);
        }

        await Task.WhenAny(Task.WhenAll(tareas), Task.Delay(5000));
        Console.WriteLine("Procesamiento de pedido finalizado.");
    }

    public static async Task VerificarStockAsync(Producto producto)
    {
        await Task.Delay(new Random().Next(500, 1500));
        Console.WriteLine($"Stock verificado para {producto.Nombre}");
    }

    public static async Task<decimal> CalcularPrecioAsync(Producto producto)
    {
        await Task.Delay(new Random().Next(500, 1500));
        decimal precio = new Random().Next(10, 100);
        Console.WriteLine($"Precio calculado para {producto.Nombre}: ${precio}");
        return precio;
    }

    public static async Task EnviarALogisticaAsync(Producto producto, decimal precio)
    {
        await Task.Delay(new Random().Next(500, 1500));
        Console.WriteLine($"Producto {producto.Nombre} enviado a logística con precio ${precio}");
    }
}
