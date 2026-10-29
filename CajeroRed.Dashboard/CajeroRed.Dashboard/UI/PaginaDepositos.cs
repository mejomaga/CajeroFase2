using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>D-07. Consulta de depósitos con filtros (fecha inicio/fin, cajero, usuario), totales, promedio y desglose por denominación.</summary>
public class PaginaDepositos : PaginaBase
{
    private static readonly Font FuenteNegrita = Tema.Fuente(9.5f, FontStyle.Bold);

    private readonly DataGridView _grid = new();
    private readonly DateTimePicker _dtpInicio = new();
    private readonly DateTimePicker _dtpFin = new();
    private readonly ComboBox _cmbCajero = new();
    private readonly TextBox _txtUsuario = new();
    private readonly Button _btnFiltrar = new();

    // Etiquetas de resumen exigidas (Totales, Promedio, Desglose)
    private readonly Label _lblTotalGeneral = new();
    private readonly Label _lblPromedio = new();
    private readonly Label _lblDesglose = new();

    private List<Transaccion> _depositos = new();

    public override string Titulo => "Consulta y Gestión de Depósitos";
    public override string Subtitulo => "Filtros por fecha, cajero y usuario, con totales, promedio y desglose por denominación (D-07)";

    public PaginaDepositos()
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
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 70));  // Tabla
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));  // Panel de totales y desglose

        var panelFiltros = CrearPanelFiltros();
        var tarjetaTabla = new TarjetaTitulada("Listado de depósitos filtrados", _grid)
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

        // Filtros de fecha (Desde / Hasta)
        var lblDesde = new Label { Text = "Desde:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _dtpInicio.Format = DateTimePickerFormat.Short;
        _dtpInicio.Value = DateTime.Now.AddDays(-7);
        _dtpInicio.Width = 100;
        _dtpInicio.Margin = new Padding(0, 3, 10, 0);

        var lblHasta = new Label { Text = "Hasta:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _dtpFin.Format = DateTimePickerFormat.Short;
        _dtpFin.Value = DateTime.Now;
        _dtpFin.Width = 100;
        _dtpFin.Margin = new Padding(0, 3, 15, 0);

        // Filtro de Cajero
        var lblCajero = new Label { Text = "Cajero:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _cmbCajero.Width = 100;
        _cmbCajero.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCajero.Items.Add("Todos");
        _cmbCajero.Items.Add("ATM-01");
        _cmbCajero.Items.Add("ATM-02");
        _cmbCajero.Items.Add("ATM-03");
        _cmbCajero.SelectedIndex = 0;
        _cmbCajero.Margin = new Padding(0, 3, 15, 0);

        // Filtro de Usuario
        var lblUsuario = new Label { Text = "Usuario / Tarjeta:", AutoSize = true, Font = FuenteNegrita, Margin = new Padding(0, 6, 5, 0) };
        _txtUsuario.Width = 130;
        _txtUsuario.Margin = new Padding(0, 3, 15, 0);

        _btnFiltrar.Text = "Filtrar Resultados";
        _btnFiltrar.BackColor = Color.FromArgb(37, 99, 235);
        _btnFiltrar.ForeColor = Color.White;
        _btnFiltrar.FlatStyle = FlatStyle.Flat;
        _btnFiltrar.Font = FuenteNegrita;
        _btnFiltrar.Width = 130;
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
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.White,
            Margin = Padding.Empty,
            Padding = new Padding(10)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        EstilitarEtiquetaResumen(_lblTotalGeneral, "Total Depositado: Q 0.00");
        EstilitarEtiquetaResumen(_lblPromedio, "Promedio por Depósito: Q 0.00");
        EstilitarEtiquetaResumen(_lblDesglose, "Desglose por denominación: —");

        panel.Controls.Add(_lblTotalGeneral, 0, 0);
        panel.Controls.Add(_lblPromedio, 1, 0);
        panel.Controls.Add(_lblDesglose, 2, 0);

        return panel;
    }

    private void EstilitarEtiquetaResumen(Label lbl, string textoInicial)
    {
        lbl.Text = textoInicial;
        lbl.Dock = DockStyle.Fill;
        lbl.Font = Tema.Fuente(9.5f);
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
        AgregarColumna("Desglose", "Desglose Billetes", 200);
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
    }
}