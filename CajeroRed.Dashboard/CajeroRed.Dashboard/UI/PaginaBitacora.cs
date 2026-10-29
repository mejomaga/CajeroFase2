using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>Etapa 5. Módulo de Bitácora de Auditoría (D-15) con acciones de admin y eventos del sistema.</summary>
public class PaginaBitacora : PaginaBase
{
    private readonly DataGridView _grid = new();
    private readonly DateTimePicker _dtpInicio = new();
    private readonly DateTimePicker _dtpFin = new();
    private readonly ComboBox _cmbTipo = new();
    private readonly Button _btnFiltrar = new();
    private readonly Label _lblEstado = new();

    public override string Titulo => "Bitácora de Auditoría del Sistema";
    public override string Subtitulo => "Registro detallado de acciones del administrador y eventos críticos de la red con filtros avanzados (D-15)";

    public PaginaBitacora()
    {
        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Tema.Fondo,
            Padding = new Padding(0)
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Panel de filtros superior
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Tabla de registros

        var panelFiltros = CrearPanelFiltros();
        var panelGrid = CrearPanelGrid();

        raiz.Controls.Add(panelFiltros, 0, 0);
        raiz.Controls.Add(panelGrid, 0, 1);

        Controls.Add(raiz);
    }

    private Control CrearPanelFiltros()
    {
        var panelContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };
        var tarjeta = new TarjetaTitulada("Filtros de Búsqueda y Auditoría", panelContenedor)
        {
            Dock = DockStyle.Top,
            Height = 110,
            Margin = new Padding(0)
        };

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(12, 10, 12, 10)
        };

        // Filtro Fecha Inicio
        var lblInicio = new Label { Text = "Desde:", AutoSize = true, Font = Tema.Fuente(9f, FontStyle.Bold), Margin = new Padding(0, 8, 5, 0) };
        _dtpInicio.Format = DateTimePickerFormat.Short;
        _dtpInicio.Value = DateTime.Now.AddDays(-7);
        _dtpInicio.Width = 120;
        _dtpInicio.Font = Tema.Fuente(9.5f);

        // Filtro Fecha Fin
        var lblFin = new Label { Text = "Hasta:", AutoSize = true, Font = Tema.Fuente(9f, FontStyle.Bold), Margin = new Padding(15, 8, 5, 0) };
        _dtpFin.Format = DateTimePickerFormat.Short;
        _dtpFin.Value = DateTime.Now;
        _dtpFin.Width = 120;
        _dtpFin.Font = Tema.Fuente(9.5f);

        // Categoría / Tipo de Evento
        var lblTipo = new Label { Text = "Categoría:", AutoSize = true, Font = Tema.Fuente(9f, FontStyle.Bold), Margin = new Padding(15, 8, 5, 0) };
        _cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbTipo.Font = Tema.Fuente(9.5f);
        _cmbTipo.Width = 160;
        _cmbTipo.Items.Add("Todas las categorías");
        _cmbTipo.Items.Add("Administración");
        _cmbTipo.Items.Add("Seguridad");
        _cmbTipo.Items.Add("Cajeros");
        _cmbTipo.Items.Add("Sistema");
        _cmbTipo.SelectedIndex = 0;

        // Botón Filtrar
        _btnFiltrar.Text = "🔍 Aplicar Filtros";
        _btnFiltrar.BackColor = Color.FromArgb(37, 99, 235); // Azul principal
        _btnFiltrar.ForeColor = Color.White;
        _btnFiltrar.FlatStyle = FlatStyle.Flat;
        _btnFiltrar.Font = Tema.Fuente(9.5f, FontStyle.Bold);
        _btnFiltrar.Width = 150;
        _btnFiltrar.Height = 35;
        _btnFiltrar.Cursor = Cursors.Hand;
        _btnFiltrar.FlatAppearance.BorderSize = 0;
        _btnFiltrar.Margin = new Padding(20, 3, 0, 0);
        _btnFiltrar.Click += (s, e) => CargarDatosBitacora();

        layout.Controls.Add(lblInicio);
        layout.Controls.Add(_dtpInicio);
        layout.Controls.Add(lblFin);
        layout.Controls.Add(_dtpFin);
        layout.Controls.Add(lblTipo);
        layout.Controls.Add(_cmbTipo);
        layout.Controls.Add(_btnFiltrar);

        panelContenedor.Controls.Add(layout);
        return tarjeta;
    }

    private Control CrearPanelGrid()
    {
        var panelContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };
        var tarjeta = new TarjetaTitulada("Historial de Eventos y Acciones Registradas", panelContenedor)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };

        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.Font = Tema.Fuente(9.5f);

        // Cargamos los datos reales al iniciar
        CargarDatosBitacora();

        panelContenedor.Controls.Add(_grid);
        return tarjeta;
    }

    private async void CargarDatosBitacora()
    {
        try
        {
            // Consultamos los datos reales a SQL Server
            var datosReales = await Contexto.Datos.ObtenerBitacoraAsync();

            // Asignamos los datos reales a la tabla
            _grid.DataSource = datosReales;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar la bitácora real: " + ex.Message);
        }
    }

    public override async Task ActualizarAsync()
    {
        // Llamamos a la carga real de datos para que se refresque
        CargarDatosBitacora();

        // Como el método es 'async', no necesitamos devolver un objeto Task.
        // Lo dejamos terminar o podemos esperar a que la carga asíncrona finalice:
        await Task.CompletedTask;
    }
}