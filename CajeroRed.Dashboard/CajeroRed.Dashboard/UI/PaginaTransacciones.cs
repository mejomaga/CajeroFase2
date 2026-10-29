using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>D-06. Módulo de transacciones en tiempo real de toda la red.</summary>
public class PaginaTransacciones : PaginaBase
{
    private static readonly Font FuenteNegrita = Tema.Fuente(9.5f, FontStyle.Bold);

    private readonly DataGridView _grid = new();
    private List<Transaccion> _transacciones = new();

    public override string Titulo => "Transacciones en Tiempo Real";
    public override string Subtitulo => "Registro general de operaciones, retiros, depósitos y rechazos en la red (D-06)";

    public PaginaTransacciones()
    {
        ConfigurarGrid();

        var tarjetaTabla = new TarjetaTitulada("Historial de transacciones de la red", _grid)
        { Dock = DockStyle.Fill, Margin = Padding.Empty };

        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = Tema.Fondo,
            Padding = new Padding(0)
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.Controls.Add(tarjetaTabla, 0, 0);

        Controls.Add(raiz);
    }

    private void ConfigurarGrid()
    {
        EstiloGrid.Aplicar(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        AgregarColumna("Id", "ID", 60);
        AgregarColumna("Fecha", "Fecha y hora", 130);
        AgregarColumna("Cajero", "Cajero", 80);
        AgregarColumna("Tipo", "Tipo", 100);
        AgregarColumna("Usuario", "Usuario", 140);
        AgregarColumna("Tarjeta", "Tarjeta", 100);
        AgregarColumna("Monto", "Monto (Q)", 110, true);
        AgregarColumna("Estado", "Resultado", 110);
        AgregarColumna("Motivo", "Motivo de rechazo", 160);
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
        _transacciones = await Contexto.Datos.ObtenerTransaccionesAsync();
        PintarFilas();
    }

    private void PintarFilas()
    {
        _grid.Rows.Clear();
        foreach (var t in _transacciones)
        {
            int indice = _grid.Rows.Add();
            var fila = _grid.Rows[indice];
            fila.Tag = t.IdTransaccion;

            fila.Cells["Id"].Value = t.IdTransaccion;
            fila.Cells["Fecha"].Value = t.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
            fila.Cells["Cajero"].Value = t.CodigoCajero;
            fila.Cells["Tipo"].Value = t.Tipo;
            fila.Cells["Usuario"].Value = t.NombreUsuario;
            fila.Cells["Tarjeta"].Value = t.TarjetaEnmascarada;
            fila.Cells["Monto"].Value = Formato.Quetzales(t.Monto);

            var celdaEstado = fila.Cells["Estado"];
            if (t.Aprobada)
            {
                celdaEstado.Value = "Aprobada (00)";
                celdaEstado.Style.ForeColor = Tema.Verde;
            }
            else
            {
                celdaEstado.Value = $"Rechazada ({t.CodigoRespuesta})";
                celdaEstado.Style.ForeColor = Tema.Rojo;
            }
            celdaEstado.Style.Font = FuenteNegrita;

            fila.Cells["Motivo"].Value = t.MotivoRechazo ?? "—";
        }
    }
}