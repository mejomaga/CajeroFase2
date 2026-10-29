using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>D-04 y D-03 (distribución por denominación). Monitor de cajeros.
/// Incluye gestión de botones: Inicializar, Cargar Efectivo, Estado Remoto, Editar (IP/Nombre) y Baja Lógica.</summary>
public class PaginaCajeros : PaginaBase
{
    private static readonly int[] Denominaciones = { 200, 100, 50, 20, 10, 5, 1 };
    private static readonly Font FuenteNegrita = Tema.Fuente(9.5f, FontStyle.Bold);

    private readonly DataGridView _grid = new();
    private readonly GraficaBarras _grafica = new();
    private readonly TarjetaTitulada _tarjetaGrafica;

    // Botones de gestión
    private readonly Button _btnInicializar = new();
    private readonly Button _btnCargarEfectivo = new();
    private readonly Button _btnEstadoRemoto = new();
    private readonly Button _btnNuevoCajero = new();
    private readonly Button _btnEditarCajero = new();
    private readonly Button _btnEliminarCajero = new();

    private List<Cajero> _cajeros = new();
    private string? _codigoSeleccionado;

    public override string Titulo => "Monitor y Gestión de Cajeros";
    public override string Subtitulo => "Estado, efectivo, billetes por denominación y control administrativo (D-04 / D-05)";

    public PaginaCajeros()
    {
        ConfigurarGrid();

        _grafica.MostrarValores = true;
        _grafica.MensajeVacio = "Selecciona un cajero en la tabla";
        _grafica.FormatoValor = v => "Q " + Formato.Numero(v);

        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Tema.Fondo,
            Margin = Padding.Empty
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 44));

        // Panel contenedor para la tabla y los botones de acción superior
        var panelContenedorTabla = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };

        // Barra de botones de gestión superior con ajuste automático para que se vean todos
        var barraAcciones = CrearBarraAcciones();
        barraAcciones.Dock = DockStyle.Top;

        _grid.Dock = DockStyle.Fill;

        panelContenedorTabla.Controls.Add(_grid);
        panelContenedorTabla.Controls.Add(barraAcciones);

        var tarjetaTabla = new TarjetaTitulada("Cajeros de la red y administración", panelContenedorTabla)
        { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 16) };

        _tarjetaGrafica = new TarjetaTitulada("Distribución del efectivo por denominación", _grafica)
        { Dock = DockStyle.Fill, Margin = Padding.Empty };

        raiz.Controls.Add(tarjetaTabla, 0, 0);
        raiz.Controls.Add(_tarjetaGrafica, 0, 1);
        Controls.Add(raiz);

        _grid.SelectionChanged += (s, e) => AlCambiarSeleccion();
    }

    private FlowLayoutPanel CrearBarraAcciones()
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true, // Permite ajustar si la pantalla es pequeña
            AutoSize = true,
            BackColor = Color.White,
            Padding = new Padding(10, 8, 10, 8)
        };

        EstilizarBoton(_btnNuevoCajero, "＋ Nuevo Cajero", Color.FromArgb(37, 99, 235));
        EstilizarBoton(_btnInicializar, "⚙ Inicializar (Q10k)", Color.FromArgb(13, 148, 136));
        EstilizarBoton(_btnCargarEfectivo, "💵 Cargar Efectivo", Color.FromArgb(217, 119, 6));
        EstilizarBoton(_btnEstadoRemoto, "🔌 Estado Remoto", Color.FromArgb(100, 116, 139));
        EstilizarBoton(_btnEditarCajero, "✏️ Editar IP / Nombre", Color.FromArgb(79, 70, 229));
        EstilizarBoton(_btnEliminarCajero, "🗑 Dar de Baja", Color.FromArgb(220, 38, 38));

        _btnInicializar.Click += BtnInicializar_Click;
        _btnCargarEfectivo.Click += BtnCargarEfectivo_Click;
        _btnEstadoRemoto.Click += BtnEstadoRemoto_Click;
        _btnNuevoCajero.Click += BtnNuevoCajero_Click;
        _btnEditarCajero.Click += BtnEditarCajero_Click;
        _btnEliminarCajero.Click += BtnEliminarCajero_Click;

        panel.Controls.Add(_btnNuevoCajero);
        panel.Controls.Add(_btnInicializar);
        panel.Controls.Add(_btnCargarEfectivo);
        panel.Controls.Add(_btnEstadoRemoto);
        panel.Controls.Add(_btnEditarCajero);
        panel.Controls.Add(_btnEliminarCajero);

        return panel;
    }

    private void EstilizarBoton(Button btn, string texto, Color colorFondo)
    {
        btn.Text = texto;
        btn.BackColor = colorFondo;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.Font = Tema.Fuente(8.5f, FontStyle.Bold);
        btn.Width = 150;
        btn.Height = 34;
        btn.Cursor = Cursors.Hand;
        btn.Margin = new Padding(0, 0, 8, 5);
        btn.FlatAppearance.BorderSize = 0;
    }

    // ------------------------------------------------------------------ tabla

    private void ConfigurarGrid()
    {
        EstiloGrid.Aplicar(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        AgregarColumna("Codigo", "Cajero", 70);
        AgregarColumna("Nombre", "Nombre", 130);
        AgregarColumna("Estado", "Estado", 120);
        AgregarColumna("IP", "IP", 105);
        AgregarColumna("Efectivo", "Efectivo total", 105, true);
        foreach (int d in Denominaciones)
            AgregarColumna("B" + d, "Q" + d, 48, true);
        AgregarColumna("Heartbeat", "Heartbeat", 85);
        AgregarColumna("UltimaOp", "Última op.", 85);
    }

    private void AgregarColumna(string nombre, string encabezado, float peso, bool derecha = false)
    {
        var columna = new DataGridViewTextBoxColumn
        {
            Name = nombre,
            HeaderText = encabezado,
            FillWeight = peso,
            MinimumWidth = 44,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        int indice = _grid.Columns.Add(columna);
        if (derecha)
        {
            _grid.Columns[indice].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _grid.Columns[indice].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }

    public override async Task ActualizarAsync()
    {
        _cajeros = await Contexto.Datos.ObtenerCajerosAsync();
        PintarFilas();
        ActualizarGrafica();
    }

    private void PintarFilas()
    {
        var existentes = _grid.Rows.Cast<DataGridViewRow>().ToDictionary(r => (string)r.Tag!);

        foreach (var cajero in _cajeros)
        {
            if (!existentes.TryGetValue(cajero.Codigo, out var fila))
            {
                int indice = _grid.Rows.Add();
                fila = _grid.Rows[indice];
                fila.Tag = cajero.Codigo;
            }
            LlenarFila(fila, cajero);
        }

        var codigos = _cajeros.Select(c => c.Codigo).ToHashSet();
        foreach (var fila in _grid.Rows.Cast<DataGridViewRow>().Where(r => !codigos.Contains((string)r.Tag!)).ToList())
            _grid.Rows.Remove(fila);
    }

    private static void LlenarFila(DataGridViewRow fila, Cajero c)
    {
        fila.Cells["Codigo"].Value = c.Codigo;
        fila.Cells["Nombre"].Value = c.Nombre;
        fila.Cells["IP"].Value = c.IP;

        var celdaEstado = fila.Cells["Estado"];
        celdaEstado.Value = "●  " + Cajero.TextoDeEstado(c.Estado);
        celdaEstado.Style.ForeColor = ColorEstado(c.Estado);
        celdaEstado.Style.SelectionForeColor = ColorEstado(c.Estado);
        celdaEstado.Style.Font = FuenteNegrita;

        var celdaEfectivo = fila.Cells["Efectivo"];
        celdaEfectivo.Value = Formato.Quetzales(c.EfectivoTotal);
        celdaEfectivo.Style.ForeColor = c.Estado == EstadoCajero.EfectivoBajo ? Tema.Naranja : Color.Empty;

        foreach (int d in Denominaciones)
            fila.Cells["B" + d].Value = c.Inventario.TryGetValue(d, out int cantidad) ? cantidad : 0;

        var celdaLatido = fila.Cells["Heartbeat"];
        celdaLatido.Value = Formato.Hora(c.UltimoHeartbeat);
        celdaLatido.Style.ForeColor = c.Estado == EstadoCajero.Desconectado ? Tema.Rojo : Color.Empty;

        fila.Cells["UltimaOp"].Value = Formato.Hora(c.UltimaTransaccion);
    }

    private static Color ColorEstado(EstadoCajero estado) => estado switch
    {
        EstadoCajero.EnLinea => Tema.Verde,
        EstadoCajero.Desconectado => Tema.Rojo,
        _ => Tema.Naranja
    };

    // ------------------------------------------------------------------ eventos de gestión (Etapa 3)

    private async void BtnInicializar_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Seleccione un cajero de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var desgloseDemo = new Dictionary<int, int> { { 200, 30 }, { 100, 40 } };

        if (MessageBox.Show($"¿Desea inicializar el cajero {_codigoSeleccionado} con el monto límite de Q 10,000.00?", "Confirmar Inicialización", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            bool exito = await Contexto.Datos.InicializarCajeroAsync(_codigoSeleccionado, desgloseDemo);
            if (exito)
            {
                MessageBox.Show("Cajero inicializado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudo inicializar. Verifique que no exceda Q 10,000.00.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void BtnCargarEfectivo_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Seleccione un cajero de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var desgloseCarga = new Dictionary<int, int> { { 100, 50 } };

        if (MessageBox.Show($"¿Desea agregar efectivo al cajero {_codigoSeleccionado}?", "Carga de Efectivo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            bool exito = await Contexto.Datos.AgregarEfectivoCajeroAsync(_codigoSeleccionado, desgloseCarga);
            if (exito)
            {
                MessageBox.Show("Efectivo agregado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("Operación rechazada. El cajero debe estar inicializado y el total no puede superar Q 30,000.00 (RN-03).", "Regla de Negocio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void BtnEstadoRemoto_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado)) return;

        bool exito = await Contexto.Datos.CambiarEstadoCajeroAsync(_codigoSeleccionado, "FueraDeServicio");
        if (exito)
        {
            MessageBox.Show($"Cajero {_codigoSeleccionado} puesto fuera de servicio.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ActualizarAsync();
        }
    }

    private void BtnNuevoCajero_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Utilice la base de datos o el módulo de registro para dar de alta nuevos cajeros.", "Nuevo Cajero", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void BtnEditarCajero_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Por favor, selecciona un cajero de la tabla primero para editarlo.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cajeroActual = _cajeros.FirstOrDefault(c => c.Codigo == _codigoSeleccionado);

        // 1. Pedir el nuevo nombre
        string nuevoNombre = Microsoft.VisualBasic.Interaction.InputBox(
            $"Modifique el nombre para el cajero {_codigoSeleccionado}:",
            "Editar Nombre del Cajero",
            cajeroActual?.Nombre ?? "Cajero");

        if (string.IsNullOrWhiteSpace(nuevoNombre)) return;

        // 2. Pedir la nueva IP para actualizarla en SQL Server
        string nuevaIp = Microsoft.VisualBasic.Interaction.InputBox(
            $"Modifique la dirección IP para el cajero {_codigoSeleccionado}:",
            "Editar IP del Cajero",
            cajeroActual?.IP ?? "127.0.0.1");

        if (string.IsNullOrWhiteSpace(nuevaIp)) return;

        try
        {
            // Guardar cambios reales en la Base de Datos / Repositorio
            bool exito = await Contexto.Datos.GuardarCajeroAsync(
                _codigoSeleccionado,
                nuevoNombre,
                cajeroActual?.Ubicacion ?? "Central",
                nuevaIp);

            if (exito)
            {
                MessageBox.Show($"¡Cajero {_codigoSeleccionado} actualizado con éxito!\nNueva IP: {nuevaIp}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync(); // Refresca la tabla automáticamente
            }
            else
            {
                MessageBox.Show("No se pudieron guardar los cambios en la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al actualizar: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnEliminarCajero_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Por favor, selecciona un cajero de la lista para dar de baja.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmacion = MessageBox.Show(
            $"¿Estás seguro de dar de baja lógicamente al cajero '{_codigoSeleccionado}'?\nEsto cumplirá con el registro histórico del sistema sin eliminar filas.",
            "Confirmar baja lógica",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmacion != DialogResult.Yes) return;

        try
        {
            bool exito = await Contexto.Datos.DarDeBajaCajeroAsync(_codigoSeleccionado);

            if (exito)
            {
                MessageBox.Show("Cajero dado de baja correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudo realizar la operación en la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error de conexión: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ------------------------------------------------------------------ gráfica

    private void AlCambiarSeleccion()
    {
        if (_grid.CurrentRow?.Tag is string codigo)
        {
            _codigoSeleccionado = codigo;
            ActualizarGrafica();
        }
    }

    private void ActualizarGrafica()
    {
        var c = _cajeros.FirstOrDefault(x => x.Codigo == _codigoSeleccionado);
        if (c == null)
        {
            _tarjetaGrafica.LblTitulo.Text = "Distribución del efectivo por denominación";
            _grafica.Actualizar(Array.Empty<string>(), Array.Empty<SerieGrafica>());
            return;
        }

        _tarjetaGrafica.LblTitulo.Text =
            $"Distribución del efectivo por denominación — {c.Codigo}  ({Formato.Quetzales(c.EfectivoTotal)})";

        var categorias = Denominaciones.Select(d => "Q" + d).ToArray();
        var valores = Denominaciones
            .Select(d => c.Inventario.TryGetValue(d, out int cantidad) ? (double)d * cantidad : 0.0)
            .ToArray();

        _grafica.Actualizar(categorias, new[]
        {
            new SerieGrafica { Nombre = "Efectivo", Color = Tema.Acento, Valores = valores }
        });
    }
}