using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>Etapa 5. Módulo de gestión y control de alertas activas y atendidas de la red.</summary>
public class PaginaAlertas : PaginaBase
{
    private static readonly Font FuenteNegrita = Tema.Fuente(9.5f, FontStyle.Bold);

    private readonly DataGridView _grid = new();
    private readonly Button _btnAtender = new();
    private List<AlertaRed> _alertas = new();

    public override string Titulo => "Centro de Alertas de la Red";
    public override string Subtitulo => "Monitoreo de incidencias críticas, bajo efectivo y estado de atención administrativa";

    public PaginaAlertas()
    {
        ConfigurarGrid();

        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Tema.Fondo,
            Margin = Padding.Empty
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Barra de acciones
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Tabla

        var barraAcciones = CrearBarraAcciones();
        var tarjetaTabla = new TarjetaTitulada("Listado de alertas registradas en el sistema", _grid)
        { Dock = DockStyle.Fill, Margin = Padding.Empty };

        raiz.Controls.Add(barraAcciones, 0, 0);
        raiz.Controls.Add(tarjetaTabla, 0, 1);

        Controls.Add(raiz);
    }

    private FlowLayoutPanel CrearBarraAcciones()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.White,
            Padding = new Padding(10, 8, 10, 8)
        };

        _btnAtender.Text = "✅ Marcar Alerta como Atendida";
        _btnAtender.BackColor = Color.FromArgb(16, 185, 129); // Verde éxito
        _btnAtender.ForeColor = Color.White;
        _btnAtender.FlatStyle = FlatStyle.Flat;
        _btnAtender.Font = Tema.Fuente(9f, FontStyle.Bold);
        _btnAtender.Width = 220;
        _btnAtender.Height = 32;
        _btnAtender.Cursor = Cursors.Hand;
        _btnAtender.FlatAppearance.BorderSize = 0;
        _btnAtender.Click += BtnAtender_Click;

        panel.Controls.Add(_btnAtender);
        return panel;
    }

    private async void BtnAtender_Click(object sender, EventArgs e)
    {
        if (_grid.CurrentRow?.Tag is not int idAlerta)
        {
            MessageBox.Show("Seleccione una alerta de la tabla para marcarla como atendida.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var alerta = _alertas.FirstOrDefault(a => a.IdAlerta == idAlerta);
        if (alerta == null) return;

        if (alerta.Atendida)
        {
            MessageBox.Show("Esta alerta ya se encuentra marcada como atendida.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        bool exito = await Contexto.Datos.AtenderAlertaAsync(idAlerta);
        if (exito)
        {
            MessageBox.Show("La alerta ha sido registrada como atendida exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ActualizarAsync();
        }
    }

    private void ConfigurarGrid()
    {
        EstiloGrid.Aplicar(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        AgregarColumna("Id", "ID", 50);
        AgregarColumna("Fecha", "Fecha y hora", 130);
        AgregarColumna("Cajero", "Cajero", 80);
        AgregarColumna("Tipo", "Tipo de Incidencia", 130);
        AgregarColumna("Descripcion", "Descripción del Problema", 240);
        AgregarColumna("Estado", "Estado", 100);
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
        _alertas = await Contexto.Datos.ObtenerAlertasAsync();
        PintarFilas();
    }

    private void PintarFilas()
    {
        _grid.Rows.Clear();
        foreach (var a in _alertas)
        {
            int indice = _grid.Rows.Add();
            var fila = _grid.Rows[indice];
            fila.Tag = a.IdAlerta;

            fila.Cells["Id"].Value = a.IdAlerta;
            fila.Cells["Fecha"].Value = a.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
            fila.Cells["Cajero"].Value = a.CodigoCajero;
            fila.Cells["Tipo"].Value = a.Tipo;
            fila.Cells["Descripcion"].Value = a.Descripcion;

            var celdaEstado = fila.Cells["Estado"];
            if (a.Atendida)
            {
                celdaEstado.Value = "Atendida";
                celdaEstado.Style.ForeColor = Tema.Verde;
            }
            else
            {
                celdaEstado.Value = "Activa";
                celdaEstado.Style.ForeColor = Tema.Naranja;
            }
            celdaEstado.Style.Font = FuenteNegrita;
        }
    }
}