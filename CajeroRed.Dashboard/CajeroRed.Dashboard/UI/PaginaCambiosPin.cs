using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>D-09. Registro histórico de cambios de PIN y función de restablecimiento de PIN temporal.</summary>
public class PaginaCambiosPin : PaginaBase
{
    private static readonly Font FuenteNegrita = Tema.Fuente(9.5f, FontStyle.Bold);

    private readonly DataGridView _grid = new();
    private readonly Button _btnRestablecerPin = new();
    private List<Transaccion> _cambiosPin = new();

    public override string Titulo => "Auditoría y Gestión de Cambios de PIN";
    public override string Subtitulo => "Registro de modificaciones, cajero de origen, estado y generación de PIN temporal (D-09)";

    public PaginaCambiosPin()
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
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Barra de acciones administrativa
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Tabla

        var barraAcciones = CrearBarraAcciones();
        var tarjetaTabla = new TarjetaTitulada("Historial de cambios de PIN en la red", _grid)
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

        _btnRestablecerPin.Text = "🔑 Restablecer PIN Temporal";
        _btnRestablecerPin.BackColor = Color.FromArgb(217, 119, 6); // Tono naranja administrativo
        _btnRestablecerPin.ForeColor = Color.White;
        _btnRestablecerPin.FlatStyle = FlatStyle.Flat;
        _btnRestablecerPin.Font = Tema.Fuente(9f, FontStyle.Bold);
        _btnRestablecerPin.Width = 210;
        _btnRestablecerPin.Height = 32;
        _btnRestablecerPin.Cursor = Cursors.Hand;
        _btnRestablecerPin.FlatAppearance.BorderSize = 0;
        _btnRestablecerPin.Click += BtnRestablecerPin_Click;

        panel.Controls.Add(_btnRestablecerPin);
        return panel;
    }

    private async void BtnRestablecerPin_Click(object sender, EventArgs e)
    {
        try
        {
            // Obtenemos el listado completo de usuarios registrados en el sistema para seleccionarlos
            var usuarios = await Contexto.Datos.ObtenerUsuariosAsync();
            if (usuarios == null || usuarios.Count == 0)
            {
                MessageBox.Show("No hay usuarios registrados en el sistema.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Creamos un diálogo emergente interactivo para seleccionar al usuario
            using var formSeleccion = new Form
            {
                Text = "Seleccionar Usuario para Restablecer PIN",
                Width = 400,
                Height = 220,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblInfo = new Label
            {
                Text = "Seleccione el usuario al que desea asignarle un PIN temporal:",
                Left = 20,
                Top = 20,
                Width = 340,
                Font = Tema.Fuente(9.5f)
            };

            var cmbUsuarios = new ComboBox
            {
                Left = 20,
                Top = 60,
                Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = usuarios,
                DisplayMember = "Nombre",
                ValueMember = "IdUsuario"
            };

            var btnAceptar = new Button
            {
                Text = "Generar PIN",
                Left = 180,
                Top = 120,
                Width = 90,
                Height = 30,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAceptar.Click += (s, ev) => { formSeleccion.DialogResult = DialogResult.OK; formSeleccion.Close(); };

            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 280,
                Top = 120,
                Width = 80,
                Height = 30,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.Click += (s, ev) => { formSeleccion.DialogResult = DialogResult.Cancel; formSeleccion.Close(); };

            formSeleccion.Controls.Add(lblInfo);
            formSeleccion.Controls.Add(cmbUsuarios);
            formSeleccion.Controls.Add(btnAceptar);
            formSeleccion.Controls.Add(btnCancelar);

            if (formSeleccion.ShowDialog(this) != DialogResult.OK) return;

            var usuarioSeleccionado = (Usuario)cmbUsuarios.SelectedItem;
            if (usuarioSeleccionado == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Desea generar un PIN temporal para el usuario '{usuarioSeleccionado.Nombre}'?\n\nEl sistema asignará una clave temporal que el usuario estará obligado a cambiar en su primer uso en el cajero (D-09).",
                "Restablecer PIN Temporal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes) return;

            // Llamada al servicio para generar el PIN temporal
            string pinTemporal = await Contexto.Datos.RestablecerPinTemporalAsync(usuarioSeleccionado.IdUsuario);

            if (!string.IsNullOrEmpty(pinTemporal))
            {
                MessageBox.Show(
                    $"¡PIN temporal generado con éxito!\n\nUsuario: {usuarioSeleccionado.Nombre}\nPIN Temporal: {pinTemporal}\n\n(Comuníquese con el cliente de forma segura. Este PIN caducará al primer cambio en el cajero).",
                    "PIN Temporal Asignado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await ActualizarAsync();
            }
            else
            {
                MessageBox.Show("No se pudo procesar el restablecimiento del PIN.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al restablecer el PIN: {ex.Message}", "Excepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarGrid()
    {
        EstiloGrid.Aplicar(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        AgregarColumna("Id", "ID", 60);
        AgregarColumna("Fecha", "Fecha y hora", 140);
        AgregarColumna("Cajero", "Cajero", 90);
        AgregarColumna("Usuario", "Usuario", 160);
        AgregarColumna("Tarjeta", "Tarjeta", 110);
        AgregarColumna("Resultado", "Estado de Operación", 130);
        AgregarColumna("Seguridad", "Nota de Seguridad", 180);
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
        try
        {
            _cambiosPin = await Contexto.Datos.ObtenerCambiosPinAsync();
            PintarFilas();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar el historial de cambios de PIN: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PintarFilas()
    {
        _grid.Rows.Clear();
        foreach (var t in _cambiosPin)
        {
            int indice = _grid.Rows.Add();
            var fila = _grid.Rows[indice];
            fila.Tag = t.IdTransaccion;

            fila.Cells["Id"].Value = t.IdTransaccion;
            fila.Cells["Fecha"].Value = t.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
            fila.Cells["Cajero"].Value = t.CodigoCajero;
            fila.Cells["Usuario"].Value = t.NombreUsuario;
            fila.Cells["Tarjeta"].Value = t.TarjetaEnmascarada;

            var celdaEstado = fila.Cells["Resultado"];
            if (t.Aprobada)
            {
                celdaEstado.Value = "Exitoso (00)";
                celdaEstado.Style.ForeColor = Tema.Verde;
            }
            else
            {
                celdaEstado.Value = $"Rechazado ({t.CodigoRespuesta})";
                celdaEstado.Style.ForeColor = Tema.Rojo;
            }
            celdaEstado.Style.Font = FuenteNegrita;

            fila.Cells["Seguridad"].Value = "PIN protegido con cifrado (********)";
        }
    }
}