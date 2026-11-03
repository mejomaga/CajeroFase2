using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

public class PaginaCajeros : PaginaBase
{
    private static readonly int[] Denominaciones = { 200, 100, 50, 20, 10, 5, 1 };
    private static readonly Font FuenteNegrita = Tema.Fuente(9.5f, FontStyle.Bold);

    private readonly DataGridView _grid = new();
    private readonly GraficaBarras _grafica = new();
    private readonly TarjetaTitulada _tarjetaGrafica;

    private readonly Button _btnInicializar = new();
    private readonly Button _btnCargarEfectivo = new();
    private readonly Button _btnInactivar = new();
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

        var panelContenedorTabla = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
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
            WrapContents = true,
            AutoSize = true,
            BackColor = Color.White,
            Padding = new Padding(10, 8, 10, 8)
        };

        EstilizarBoton(_btnInicializar, "⚙ Inicializar (Máx 10k)", Color.FromArgb(13, 148, 136));
        EstilizarBoton(_btnCargarEfectivo, "💵 Cargar Lote (Máx 30k)", Color.FromArgb(217, 119, 6));
        EstilizarBoton(_btnInactivar, "✖ Inactivar Cajero", Color.FromArgb(100, 116, 139));
        EstilizarBoton(_btnNuevoCajero, "＋ Nuevo Cajero", Color.FromArgb(37, 99, 235));
        EstilizarBoton(_btnEditarCajero, "✏️ Editar IP/Nombre", Color.FromArgb(79, 70, 229));
        EstilizarBoton(_btnEliminarCajero, "🗑 Eliminar", Color.FromArgb(220, 38, 38));

        _btnInicializar.Click += BtnInicializar_Click;
        _btnCargarEfectivo.Click += BtnCargarEfectivo_Click;
        _btnInactivar.Click += BtnInactivar_Click;
        _btnNuevoCajero.Click += BtnNuevoCajero_Click;
        _btnEditarCajero.Click += BtnEditarCajero_Click;
        _btnEliminarCajero.Click += BtnEliminarCajero_Click;

        panel.Controls.Add(_btnInicializar);
        panel.Controls.Add(_btnCargarEfectivo);
        panel.Controls.Add(_btnInactivar);
        panel.Controls.Add(_btnNuevoCajero);
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
        btn.Width = 155;
        btn.Height = 34;
        btn.Cursor = Cursors.Hand;
        btn.Margin = new Padding(0, 0, 8, 5);
        btn.FlatAppearance.BorderSize = 0;
    }

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

        // --- TRADUCCIÓN EXACTA SOLICITADA ---
        // Si el estado del enumerador no es Desconectado, se asume Activo.
        bool esActivo = c.EstaActivo;
        string estadoVisual = esActivo ? "Activo" : "Inactivo";
        Color colorEstado = esActivo ? Tema.Verde : Tema.Rojo;

        var celdaEstado = fila.Cells["Estado"];
        celdaEstado.Value = "●  " + estadoVisual;
        celdaEstado.Style.ForeColor = colorEstado;
        celdaEstado.Style.SelectionForeColor = colorEstado;
        celdaEstado.Style.Font = FuenteNegrita;
        // ------------------------------------

        var celdaEfectivo = fila.Cells["Efectivo"];
        celdaEfectivo.Value = Formato.Quetzales(c.EfectivoTotal);
        celdaEfectivo.Style.ForeColor = c.Estado == EstadoCajero.EfectivoBajo ? Tema.Naranja : Color.Empty;

        foreach (int d in Denominaciones)
            fila.Cells["B" + d].Value = c.Inventario.TryGetValue(d, out int cantidad) ? cantidad : 0;

        var celdaLatido = fila.Cells["Heartbeat"];
        celdaLatido.Value = Formato.Hora(c.UltimoHeartbeat);

        fila.Cells["UltimaOp"].Value = Formato.Hora(c.UltimaTransaccion);
    }

    // ------------------------------------------------------------------ EVENTOS DE GESTIÓN

    private async void BtnInicializar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Seleccione un cajero de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var desglose = SolicitarDesgloseBilletes($"Inicializar Cajero {_codigoSeleccionado}");
        if (desglose == null || desglose.Count == 0) return;

        decimal total = desglose.Sum(x => x.Key * x.Value);
        if (total > 10000m)
        {
            MessageBox.Show($"El monto total (Q. {total:N2}) excede el límite máximo permitido de Q. 10,000.00 para inicializar.",
                            "Límite Excedido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Desea inicializar el cajero {_codigoSeleccionado} con Q. {total:N2} en total?",
                            "Confirmar Inicialización", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            bool exito = await Contexto.Datos.InicializarCajeroAsync(_codigoSeleccionado, desglose);
            if (exito)
            {
                // Pasamos a "Activo" en la BD
                await Contexto.Datos.CambiarEstadoCajeroAsync(_codigoSeleccionado, "Activo");

                MessageBox.Show("Cajero inicializado y activado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudo inicializar. Verifique la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void BtnCargarEfectivo_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Seleccione un cajero de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cajeroActual = _cajeros.FirstOrDefault(c => c.Codigo == _codigoSeleccionado);
        if (cajeroActual == null) return;

        // VALIDADOR CÓDIGO 91: El cajero debe estar activo y con inicialización previa (Efectivo > 0)
        bool esActivo = cajeroActual.EstaActivo;

        if (!esActivo || cajeroActual.EfectivoTotal == 0)
        {
            MessageBox.Show("CÓDIGO 91: Operación rechazada.\n\nEl cajero no ha sido inicializado previamente o se encuentra inactivo. Debe utilizar el botón 'Inicializar' primero para registrar un cuadre de inicio de día.",
                            "Cajero No Inicializado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var desglose = SolicitarDesgloseBilletes("Cargar Nuevo Lote de Efectivo");
        if (desglose == null || desglose.Count == 0) return;

        decimal montoAgregar = desglose.Sum(x => x.Key * x.Value);
        decimal totalResultante = cajeroActual.EfectivoTotal + montoAgregar;

        if (totalResultante > 30000m)
        {
            MessageBox.Show($"No se puede agregar el lote. La capacidad máxima del cajero es de Q. 30,000.00.\n\n" +
                            $"- Efectivo actual: Q. {cajeroActual.EfectivoTotal:N2}\n" +
                            $"- Intenta agregar: Q. {montoAgregar:N2}\n" +
                            $"- Total resultante: Q. {totalResultante:N2}",
                            "Capacidad Máxima Excedida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Desea agregar Q. {montoAgregar:N2} al cajero {_codigoSeleccionado}?",
                            "Carga de Efectivo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            bool exito = await Contexto.Datos.AgregarEfectivoCajeroAsync(_codigoSeleccionado, desglose);
            if (exito)
            {
                MessageBox.Show("Efectivo agregado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudo agregar el efectivo.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void BtnInactivar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Seleccione un cajero de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"¿Desea bloquear e inactivar el cajero {_codigoSeleccionado}?\n\nLa pantalla del cajero se bloqueará inmediatamente. Para volver a activarlo tendrá que realizar una nueva inicialización de dinero.",
                            "Inactivar Cajero", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            // Escribimos explícitamente "Inactivo" en la base de datos
            bool exito = await Contexto.Datos.CambiarEstadoCajeroAsync(_codigoSeleccionado, "Inactivo");

            if (exito)
            {
                MessageBox.Show($"Cajero {_codigoSeleccionado} bloqueado correctamente. Su estado ha cambiado a Inactivo.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudo inactivar el cajero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private Dictionary<int, int>? SolicitarDesgloseBilletes(string tituloVentana)
    {
        var desglose = new Dictionary<int, int>();
        foreach (int d in Denominaciones)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Ingrese la CANTIDAD DE BILLETES de Q {d}:",
                tituloVentana,
                "0");

            if (string.IsNullOrEmpty(input)) return null;

            if (int.TryParse(input, out int cantidad) && cantidad > 0)
            {
                desglose[d] = cantidad;
            }
        }
        return desglose;
    }
    private async void BtnHabilitar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado)) return;

        if (MessageBox.Show($"¿Habilitar el cajero {_codigoSeleccionado}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            // Usamos el estado exacto de tu SQL
            bool exito = await Contexto.Datos.CambiarEstadoCajeroAsync(_codigoSeleccionado, "En línea");
            if (exito)
            {
                MessageBox.Show("Cajero habilitado. La pantalla cambiará en unos segundos.");
                await ActualizarAsync();
            }
        }
    }

    private async void BtnInhabilitar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado)) return;

        if (MessageBox.Show($"¿Inhabilitar el cajero {_codigoSeleccionado}?", "Inhabilitar", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            bool exito = await Contexto.Datos.CambiarEstadoCajeroAsync(_codigoSeleccionado, "Fuera de servicio");
            if (exito)
            {
                MessageBox.Show("Cajero inhabilitado. Se bloqueará en unos segundos.");
                await ActualizarAsync();
            }
        }
    }
    private async void BtnNuevoCajero_Click(object? sender, EventArgs e)
    {
        string codigo = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el CÓDIGO único del nuevo cajero (ej. C04):", "Nuevo Cajero", "");
        if (string.IsNullOrWhiteSpace(codigo)) return;

        string nombre = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el NOMBRE descriptivo del cajero:", "Nuevo Cajero", "");
        if (string.IsNullOrWhiteSpace(nombre)) return;

        string ubicacion = Microsoft.VisualBasic.Interaction.InputBox("Ingrese la UBICACIÓN física:", "Nuevo Cajero", "");
        if (string.IsNullOrWhiteSpace(ubicacion)) return;

        string ip = Microsoft.VisualBasic.Interaction.InputBox("Ingrese la dirección IP del cajero:", "Nuevo Cajero", "127.0.0.1");
        if (string.IsNullOrWhiteSpace(ip)) return;

        try
        {
            bool exito = await Contexto.Datos.GuardarCajeroAsync(codigo.Trim(), nombre.Trim(), ubicacion.Trim(), ip.Trim());

            if (exito)
            {
                // Regla de Negocio: Todo cajero nace "Inactivo" obligatoriamente
                await Contexto.Datos.CambiarEstadoCajeroAsync(codigo.Trim(), "Inactivo");

                MessageBox.Show($"¡Cajero '{codigo.Trim()}' registrado con éxito! Nace Inactivo, por lo que debe inicializarlo para que empiece a operar.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudo registrar el nuevo cajero.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al crear el cajero: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnEditarCajero_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Por favor, selecciona un cajero de la tabla primero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cajeroActual = _cajeros.FirstOrDefault(c => c.Codigo == _codigoSeleccionado);

        string nuevoNombre = Microsoft.VisualBasic.Interaction.InputBox(
            $"Modifique el nombre para el cajero {_codigoSeleccionado}:",
            "Editar Nombre",
            cajeroActual?.Nombre ?? "");

        if (string.IsNullOrWhiteSpace(nuevoNombre)) return;

        string nuevaIp = Microsoft.VisualBasic.Interaction.InputBox(
            $"Modifique la dirección IP para el cajero {_codigoSeleccionado}:",
            "Editar IP",
            cajeroActual?.IP ?? "");

        if (string.IsNullOrWhiteSpace(nuevaIp)) return;

        try
        {
            bool exito = await Contexto.Datos.GuardarCajeroAsync(
                _codigoSeleccionado,
                nuevoNombre.Trim(),
                cajeroActual?.Ubicacion ?? "Central",
                nuevaIp.Trim());

            if (exito)
            {
                MessageBox.Show($"Cajero actualizado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudieron guardar los cambios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al actualizar: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnEliminarCajero_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_codigoSeleccionado))
        {
            MessageBox.Show("Por favor, selecciona un cajero de la lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmacion = MessageBox.Show(
            $"¿Estás seguro de ELIMINAR al cajero '{_codigoSeleccionado}'?\n\nEl cajero dejará de operar inmediatamente en la red.",
            "Confirmar Eliminación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmacion != DialogResult.Yes) return;

        try
        {
            bool exito = await Contexto.Datos.DarDeBajaCajeroAsync(_codigoSeleccionado);

            if (exito)
            {
                MessageBox.Show("Cajero eliminado de la red correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudo realizar la eliminación en la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error de conexión: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

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