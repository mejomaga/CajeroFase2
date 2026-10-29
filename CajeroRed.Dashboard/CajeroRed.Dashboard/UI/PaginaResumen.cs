using System.Drawing;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>D-02 y D-03. Resumen del día: indicadores y gráficas.</summary>
public class PaginaResumen : PaginaBase
{
    private readonly TarjetaIndicador _kLinea = new("Cajeros en línea", Tema.Acento);
    private readonly TarjetaIndicador _kEfectivo = new("Efectivo total en la red", Tema.Acento);
    private readonly TarjetaIndicador _kRetirado = new("Total retirado", Tema.Acento);
    private readonly TarjetaIndicador _kDepositado = new("Total depositado", Tema.Acento);
    private readonly TarjetaIndicador _kPromedio = new("Promedio depositado", Tema.Acento);
    private readonly TarjetaIndicador _kTransacciones = new("Transacciones del día", Tema.Acento);
    private readonly TarjetaIndicador _kPin = new("Cambios de PIN", Tema.Acento);
    private readonly TarjetaIndicador _kRechazadas = new("Operaciones rechazadas", Tema.Verde);
    private readonly TarjetaIndicador _kAlertas = new("Alertas activas", Tema.Verde);

    private readonly GraficaBarras _graficaHoras = new();
    private readonly GraficaBarras _graficaCajeros = new();

    public override string Titulo => "Resumen general — hoy";
    public override string Subtitulo => "Indicadores del día, actualizados automáticamente cada 5 segundos";

    public PaginaResumen()
    {
        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Tema.Fondo,
            Margin = Padding.Empty
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 196));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // ----- indicadores: 5 arriba y 4 abajo (el último ocupa 2 columnas) -----
        var kpis = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 2,
            BackColor = Tema.Fondo,
            Margin = Padding.Empty
        };
        for (int i = 0; i < 5; i++) kpis.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        kpis.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        kpis.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));

        Colocar(kpis, _kLinea, 0, 0);
        Colocar(kpis, _kEfectivo, 1, 0);
        Colocar(kpis, _kRetirado, 2, 0);
        Colocar(kpis, _kDepositado, 3, 0);
        Colocar(kpis, _kPromedio, 4, 0);
        Colocar(kpis, _kTransacciones, 0, 1);
        Colocar(kpis, _kPin, 1, 1);
        Colocar(kpis, _kRechazadas, 2, 1);
        Colocar(kpis, _kAlertas, 3, 1, 2);

        // ----- gráficas -----
        var graficas = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Tema.Fondo,
            Margin = new Padding(0, 16, 0, 0)
        };
        graficas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        graficas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        graficas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _graficaCajeros.MostrarValores = true;
        _graficaCajeros.FormatoEje = v => v >= 1000 ? (v / 1000).ToString("0.#") + "k" : v.ToString("0");

        graficas.Controls.Add(new TarjetaTitulada("Retiros vs depósitos por hora (Q)", _graficaHoras)
            { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) }, 0, 0);
        graficas.Controls.Add(new TarjetaTitulada("Efectivo disponible por cajero (Q)", _graficaCajeros)
            { Dock = DockStyle.Fill, Margin = Padding.Empty }, 1, 0);

        raiz.Controls.Add(kpis, 0, 0);
        raiz.Controls.Add(graficas, 0, 1);
        Controls.Add(raiz);
    }

    private static void Colocar(TableLayoutPanel tabla, Control control, int columna, int fila, int span = 1)
    {
        bool ultimaColumna = columna + span >= tabla.ColumnCount;
        bool ultimaFila = fila >= tabla.RowCount - 1;
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 0, ultimaColumna ? 0 : 12, ultimaFila ? 0 : 12);
        tabla.Controls.Add(control, columna, fila);
        if (span > 1) tabla.SetColumnSpan(control, span);
    }

    public override async Task ActualizarAsync()
    {
        var tareaResumen = Contexto.Datos.ObtenerResumenAsync();
        var tareaHoras = Contexto.Datos.ObtenerSerieHorariaAsync();
        var tareaCajeros = Contexto.Datos.ObtenerCajerosAsync();
        await Task.WhenAll(tareaResumen, tareaHoras, tareaCajeros);

        Mostrar(await tareaResumen, await tareaHoras, await tareaCajeros);
    }

    private void Mostrar(ResumenDia r, List<PuntoHora> horas, List<Cajero> cajeros)
    {
        // Indicadores. Colores: verde = correcto, naranja = advertencia, rojo = falla.
        _kLinea.Valor = $"{r.CajerosEnLinea} / {r.CajerosTotal}";
        _kLinea.Detalle = "conectados ahora";
        _kLinea.ColorAcento = r.CajerosEnLinea == r.CajerosTotal ? Tema.Verde
                            : r.CajerosEnLinea == 0 ? Tema.Rojo : Tema.Naranja;

        _kEfectivo.Valor = Formato.Quetzales(r.EfectivoTotalRed);
        _kEfectivo.Detalle = "suma de todos los cajeros";
        _kRetirado.Valor = Formato.Quetzales(r.TotalRetirado);
        _kRetirado.Detalle = "retiros aprobados hoy";
        _kDepositado.Valor = Formato.Quetzales(r.TotalDepositado);
        _kDepositado.Detalle = "depósitos aprobados hoy";
        _kPromedio.Valor = Formato.Quetzales(r.PromedioDepositadoPorUsuario);
        _kPromedio.Detalle = "por usuario";

        _kTransacciones.Valor = r.NumeroTransacciones.ToString();
        _kTransacciones.Detalle = "operaciones registradas";
        _kPin.Valor = r.CambiosPin.ToString();
        _kPin.Detalle = "realizados hoy";

        _kRechazadas.Valor = r.OperacionesRechazadas.ToString();
        _kRechazadas.Detalle = r.OperacionesRechazadas == 0 ? "sin rechazos" : "revisar en Retiros";
        _kRechazadas.ColorAcento = r.OperacionesRechazadas == 0 ? Tema.Verde : Tema.Naranja;

        _kAlertas.Valor = r.AlertasActivas.ToString();
        _kAlertas.Detalle = r.AlertasActivas == 0 ? "todo en orden" : "pendientes de atender";
        _kAlertas.ColorAcento = r.AlertasActivas == 0 ? Tema.Verde : Tema.Naranja;

        // Gráfica 1: retiros vs depósitos por hora
        _graficaHoras.Actualizar(
            horas.Select(p => $"{p.Hora}h").ToArray(),
            new[]
            {
                new SerieGrafica { Nombre = "Retiros", Color = Tema.Acento, Valores = horas.Select(p => (double)p.Retiros).ToArray() },
                new SerieGrafica { Nombre = "Depósitos", Color = ColorTranslator.FromHtml("#93C5FD"), Valores = horas.Select(p => (double)p.Depositos).ToArray() }
            });

        // Gráfica 2: efectivo por cajero
        _graficaCajeros.Actualizar(
            cajeros.Select(c => c.Codigo).ToArray(),
            new[]
            {
                new SerieGrafica { Nombre = "Efectivo", Color = Tema.Acento, Valores = cajeros.Select(c => (double)c.EfectivoTotal).ToArray() }
            });
    }
}
