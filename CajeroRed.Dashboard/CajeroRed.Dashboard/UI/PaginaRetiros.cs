using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>D-08. Consulta de retiros, rechazos, motivos y consumo de límite diario.</summary>
public class PaginaRetiros : PaginaBase
{
    private static readonly Font FuenteNegrita = Tema.Fuente(9.5f, FontStyle.Bold);

    private readonly DataGridView _grid = new();
    private readonly DateTimePicker _dtpInicio = new();
    private readonly DateTimePicker _dtpFin = new();
    private readonly ComboBox _cmbCajero = new();
    private readonly ComboBox _cmbEstado = new(); // Todos, Aprobados, Rechazados
    private readonly TextBox _txtUsuario = new();
    private readonly Button _btnFiltrar = new();

    // Etiquetas de resumen
    private readonly Label _lblTotalRetirado = new();
    private readonly Label _lblRechazosCount = new();

    private List<Transaccion> _retiros = new();

    public override string Titulo => "Control de Retiros y Rechazos";
    public override string Subtitulo => "Consulta de retiros, códigos de respuesta, motivos de rechazo y consumo de límite diario (D-08)";

    public PaginaRetiros()
    {
        ConfigurarGrid();

        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Tema.Fondo,
            Margin = Padding.Empty
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); // Panel de filtros
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 75));  // Tabla
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Panel de totales

        var panelFiltros = CrearPanelFiltros();
        var tarjetaTabla = new TarjetaTitulada("Listado de retiros y operaciones de la red", _grid)
        { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12) };
        var panelResumen = CrearPanelResumen();

        raiz.Controls.Add(panelFiltros, 0, 0);
        raiz.Controls.Add(tarjetaTabla, 0, 1);
        raiz.Controls.Add(panelResumen, 0, 2);

        Controls.Add(raiz);
    }

    private Control CrearPanelFiltros()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.White,
            Padding = new Padding(12, 10, 12, 10)
        };

        var lblDesde = new Label { Text = "Desde:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _dtpInicio.Format = DateTimePickerFormat.Short;
        _dtpInicio.Value = DateTime.Now.AddDays(-7);
        _dtpInicio.Width = 95;
        _dtpInicio.Margin = new Padding(0, 3, 10, 0);

        var lblHasta = new Label { Text = "Hasta:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _dtpFin.Format = DateTimePickerFormat.Short;
        _dtpFin.Value = DateTime.Now;
        _dtpFin.Width = 95;
        _dtpFin.Margin = new Padding(0, 3, 15, 0);

        var lblCajero = new Label { Text = "Cajero:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _cmbCajero.Width = 95;
        _cmbCajero.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCajero.Items.Add("Todos");
        _cmbCajero.Items.Add("ATM-01");
        _cmbCajero.Items.Add("ATM-02");
        _cmbCajero.Items.Add("ATM-03");
        _cmbCajero.SelectedIndex = 0;
        _cmbCajero.Margin = new Padding(0, 3, 15, 0);

        var lblEstado = new Label { Text = "Resultado:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _cmbEstado.Width = 110;
        _cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbEstado.Items.Add("Todos");
        _cmbEstado.Items.Add("Aprobados");
        _cmbEstado.Items.Add("Rechazados");
        _cmbEstado.SelectedIndex = 0;
        _cmbEstado.Margin = new Padding(0, 3, 15, 0);

        var lblUsuario = new Label { Text = "Usuario:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _txtUsuario.Width = 120;
        _txtUsuario.Margin = new Padding(0, 3, 15, 0);

        _btnFiltrar.Text = "Filtrar";
        _btnFiltrar.BackColor = Color.FromArgb(37, 99, 235);
        _btnFiltrar.ForeColor = Color.White;
        _btnFiltrar.FlatStyle = FlatStyle.Flat;
        _btnFiltrar.Font = FuenteNegrita;
        _btnFiltrar.Width = 90;
        _btnFiltrar.Height = 30;
        _btnFiltrar.Cursor = Cursors.Hand;
        _btnFiltrar.FlatAppearance.BorderSize = 0;
        _btnFiltrar.Click += async (s, e) => await ActualizarAsync();

        panel.Controls.Add(lblDesde);
        panel.Controls.Add(_dtpInicio);
        panel.Controls.Add(lblHasta);
        panel.Controls.Add(_dtpFin);
        panel.Controls.Add(lblCajero);
        panel.Controls.Add(_cmbCajero);
        panel.Controls.Add(lblEstado);
        panel.Controls.Add(_cmbEstado);
        panel.Controls.Add(lblUsuario);
        panel.Controls.Add(_txtUsuario);
        panel.Controls.Add(_btnFiltrar);

        return panel;
    }

    private Control CrearPanelResumen()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White,
            Margin = Padding.Empty,
            Padding = new Padding(15, 10, 15, 10)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        EstilizarEtiquetaResumen(_lblTotalRetirado, "Total Retirado Exitosamente: Q 0.00");
        EstilizarEtiquetaResumen(_lblRechazosCount, "Operaciones Rechazadas: 0");

        panel.Controls.Add(_lblTotalRetirado, 0, 0);
        panel.Controls.Add(_lblRechazosCount, 1, 0);

        return panel;
    }

    private void EstilizarEtiquetaResumen(Label lbl, string textoInicial)
    {
        lbl.Text = textoInicial;
        lbl.Dock = DockStyle.Fill;
        lbl.Font = Tema.Fuente(10f, FontStyle.Bold);
        lbl.ForeColor = Tema.Texto;
        lbl.TextAlign = ContentAlignment.MiddleLeft;
    }

    private void ConfigurarGrid()
    {
        EstiloGrid.Aplicar(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        AgregarColumna("Id", "ID", 50);
        AgregarColumna("Fecha", "Fecha y hora", 130);
        AgregarColumna("Cajero", "Cajero", 80);
        AgregarColumna("Usuario", "Usuario", 140);
        AgregarColumna("Tarjeta", "Tarjeta", 100);
        AgregarColumna("Monto", "Monto (Q)", 100, true);
        AgregarColumna("Resultado", "Estado", 110);
        AgregarColumna("Motivo", "Motivo de rechazo / Detalle", 180);
    }

    private void AgregarColumna(string nombre, string encabezado, float peso, bool derecha = false)
    {
        var columna = new DataGridViewTextBoxColumn
        {
            Name = nombre,
            HeaderText = encabezado,
            FillWeight = peso,
            MinimumWidth = 50,
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
        string? cajeroFiltro = _cmbCajero.SelectedItem?.ToString();
        if (cajeroFiltro == "Todos") cajeroFiltro = null;

        bool? soloRechazados = null;
        string estadoFiltro = _cmbEstado.SelectedItem?.ToString() ?? "Todos";
        if (estadoFiltro == "Rechazados") soloRechazados = true;
        else if (estadoFiltro == "Aprobados") soloRechazados = false; // lo filtramos abajo

        _retiros = await Contexto.Datos.ObtenerRetirosAsync(cajeroFiltro, soloRechazados == true ? true : null);

        // Rango de fechas
        DateTime fechaInicio = _dtpInicio.Value.Date;
        DateTime fechaFin = _dtpFin.Value.Date.AddDays(1).AddTicks(-1);
        _retiros = _retiros.Where(r => r.Fecha >= fechaInicio && r.Fecha <= fechaFin).ToList();

        // Filtro específico para Aprobados si se seleccionó en el combobox
        if (estadoFiltro == "Aprobados")
        {
            _retiros = _retiros.Where(r => r.Aprobada).ToList();
        }

        // Filtro por usuario/tarjeta
        string textoUsuario = _txtUsuario.Text.Trim();
        if (!string.IsNullOrEmpty(textoUsuario))
        {
            _retiros = _retiros.Where(r =>
                r.NombreUsuario.Contains(textoUsuario, StringComparison.OrdinalIgnoreCase) ||
                r.TarjetaEnmascarada.Contains(textoUsuario)).ToList();
        }

        PintarFilas();
        ActualizarMetricas();
    }

    private void PintarFilas()
    {
        _grid.Rows.Clear();
        foreach (var t in _retiros)
        {
            int indice = _grid.Rows.Add();
            var fila = _grid.Rows[indice];
            fila.Tag = t.IdTransaccion;

            fila.Cells["Id"].Value = t.IdTransaccion;
            fila.Cells["Fecha"].Value = t.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
            fila.Cells["Cajero"].Value = t.CodigoCajero;
            fila.Cells["Usuario"].Value = t.NombreUsuario;
            fila.Cells["Tarjeta"].Value = t.TarjetaEnmascarada;
            fila.Cells["Monto"].Value = Formato.Quetzales(t.Monto);

            var celdaEstado = fila.Cells["Resultado"];
            if (t.Aprobada)
            {
                celdaEstado.Value = "Aprobado (00)";
                celdaEstado.Style.ForeColor = Tema.Verde;
                fila.Cells["Motivo"].Value = "Transacción exitosa";
            }
            else
            {
                celdaEstado.Value = $"Rechazado ({t.CodigoRespuesta})";
                celdaEstado.Style.ForeColor = Tema.Rojo;
                fila.Cells["Motivo"].Value = t.MotivoRechazo ?? "Límite diario excedido o error de saldo";
            }
            celdaEstado.Style.Font = FuenteNegrita;
        }
    }

    private void ActualizarMetricas()
    {
        decimal totalExitoso = _retiros.Where(r => r.Aprobada).Sum(r => r.Monto);
        int totalRechazos = _retiros.Count(r => !r.Aprobada);

        _lblTotalRetirado.Text = $"Total Retirado Exitosamente: {Formato.Quetzales(totalExitoso)}";
        _lblRechazosCount.Text = $"Operaciones Rechazadas: {totalRechazos}";
    }
}
