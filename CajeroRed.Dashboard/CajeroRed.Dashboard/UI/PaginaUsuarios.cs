using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>D-10 y D-11. Gestión y consulta de usuarios (crear, modificar, cambiar tarjeta conservando límites, bloquear/desbloquear).</summary>
public class PaginaUsuarios : PaginaBase
{
    private static readonly Font FuenteNegrita = Tema.Fuente(9.5f, FontStyle.Bold);

    private readonly DataGridView _grid = new();
    private readonly Button _btnNuevoUsuario = new();
    private readonly Button _btnModificar = new();
    private readonly Button _btnCambiarTarjeta = new();
    private readonly Button _btnBloquearDesbloquear = new();

    private List<Usuario> _usuarios = new();
    private int? _idUsuarioSeleccionado;

    public override string Titulo => "Gestión de Usuarios";
    public override string Subtitulo => "Creación, control de tarjetas, límites diarios, saldos y bloqueos (D-10)";

    public PaginaUsuarios()
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
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Barra superior de acciones
        var barraAcciones = CrearBarraAcciones();

        // Contenedor de la tabla
        var tarjetaTabla = new TarjetaTitulada("Listado de usuarios de la red", _grid)
        { Dock = DockStyle.Fill, Margin = Padding.Empty };

        raiz.Controls.Add(barraAcciones, 0, 0);
        raiz.Controls.Add(tarjetaTabla, 0, 1);
        Controls.Add(raiz);

        _grid.SelectionChanged += (s, e) => AlCambiarSeleccion();
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

        EstilizarBoton(_btnNuevoUsuario, "＋ Crear Usuario", Color.FromArgb(37, 99, 235));
        EstilizarBoton(_btnModificar, "✏ Modificar Datos", Color.FromArgb(13, 148, 136));
        EstilizarBoton(_btnCambiarTarjeta, "💳 Cambiar Tarjeta", Color.FromArgb(217, 119, 6));
        EstilizarBoton(_btnBloquearDesbloquear, "🔒 Bloquear / Desbloquear", Color.FromArgb(185, 28, 28));

        _btnNuevoUsuario.Click += BtnNuevoUsuario_Click;
        _btnModificar.Click += BtnModificar_Click;
        _btnCambiarTarjeta.Click += BtnCambiarTarjeta_Click;
        _btnBloquearDesbloquear.Click += BtnBloquearDesbloquear_Click;

        panel.Controls.Add(_btnNuevoUsuario);
        panel.Controls.Add(_btnModificar);
        panel.Controls.Add(_btnCambiarTarjeta);
        panel.Controls.Add(_btnBloquearDesbloquear);

        return panel;
    }

    private void EstilizarBoton(Button btn, string texto, Color colorFondo)
    {
        btn.Text = texto;
        btn.BackColor = colorFondo;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.Font = Tema.Fuente(9f, FontStyle.Bold);
        btn.Width = 180;
        btn.Height = 32;
        btn.Cursor = Cursors.Hand;
        btn.Margin = new Padding(0, 0, 10, 0);
        btn.FlatAppearance.BorderSize = 0;
    }

    private void ConfigurarGrid()
    {
        EstiloGrid.Aplicar(_grid);
        _grid.Dock = DockStyle.Fill;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        AgregarColumna("Id", "ID", 50);
        AgregarColumna("Nombre", "Nombre completo", 160);
        AgregarColumna("Tarjeta", "Tarjeta", 100);
        AgregarColumna("Saldo", "Saldo actual", 110, true);
        AgregarColumna("Limite", "Límite diario", 110, true);
        AgregarColumna("Retirado", "Retirado hoy", 110, true);
        AgregarColumna("Disponible", "Disponible hoy", 110, true);
        AgregarColumna("Estado", "Estado", 90);
        AgregarColumna("UltimoAcceso", "Último acceso", 120);
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
        _usuarios = await Contexto.Datos.ObtenerUsuariosAsync();
        PintarFilas();
    }

    private void PintarFilas()
    {
        _grid.Rows.Clear();
        foreach (var u in _usuarios)
        {
            int indice = _grid.Rows.Add();
            var fila = _grid.Rows[indice];
            fila.Tag = u.IdUsuario;

            fila.Cells["Id"].Value = u.IdUsuario;
            fila.Cells["Nombre"].Value = u.Nombre;
            fila.Cells["Tarjeta"].Value = u.TarjetaEnmascarada;
            fila.Cells["Saldo"].Value = Formato.Quetzales(u.Saldo);
            fila.Cells["Limite"].Value = Formato.Quetzales(u.LimiteDiario);
            fila.Cells["Retirado"].Value = Formato.Quetzales(u.RetiradoHoy);
            fila.Cells["Disponible"].Value = Formato.Quetzales(u.DisponibleHoy);

            var celdaEstado = fila.Cells["Estado"];
            celdaEstado.Value = u.Estado;
            celdaEstado.Style.ForeColor = u.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? Tema.Verde : Tema.Rojo;
            celdaEstado.Style.Font = FuenteNegrita;

            fila.Cells["UltimoAcceso"].Value = u.UltimoAcceso.HasValue ? Formato.Hora(u.UltimoAcceso.Value) : "Sin accesos";
        }
    }

    private void AlCambiarSeleccion()
    {
        if (_grid.CurrentRow?.Tag is int id)
        {
            _idUsuarioSeleccionado = id;
        }
    }

    private async void BtnNuevoUsuario_Click(object sender, EventArgs e)
    {
        // Ventana rápida de captura para cumplir estrictamente con D-10 (Nombre, Tarjeta 16 dígitos, PIN 4 dígitos, Saldo, Límite)
        string nombre = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el nombre completo del usuario:", "Nuevo Usuario", "Cliente Nuevo");
        if (string.IsNullOrWhiteSpace(nombre)) return;

        string tarjeta = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el número de tarjeta (16 dígitos):", "Nuevo Usuario", "4532118890123456");
        if (tarjeta.Length != 16)
        {
            MessageBox.Show("La tarjeta debe tener exactamente 16 dígitos.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string pin = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el PIN (4 dígitos):", "Nuevo Usuario", "1234");
        if (pin.Length != 4)
        {
            MessageBox.Show("El PIN debe tener exactamente 4 dígitos.", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        string saldoStr = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el saldo inicial (Q):", "Nuevo Usuario", "5000.00");
        if (!decimal.TryParse(saldoStr, out decimal saldo)) saldo = 5000.00m;

        string limiteStr = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el límite diario global (Q):", "Nuevo Usuario", "2000.00");
        if (!decimal.TryParse(limiteStr, out decimal limite)) limite = 2000.00m;

        bool exito = await Contexto.Datos.CrearUsuarioAsync(nombre, tarjeta, pin, saldo, limite);
        if (exito)
        {
            MessageBox.Show("Usuario creado exitosamente en la red.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ActualizarAsync();
        }
    }

    private async void BtnModificar_Click(object sender, EventArgs e)
    {
        if (!_idUsuarioSeleccionado.HasValue)
        {
            MessageBox.Show("Seleccione un usuario de la tabla.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var usuarioActual = _usuarios.FirstOrDefault(u => u.IdUsuario == _idUsuarioSeleccionado.Value);
        string nombreActual = usuarioActual?.Nombre ?? "";

        string nuevoNombre = Microsoft.VisualBasic.Interaction.InputBox("Modificar nombre del usuario:", "Modificar Datos", nombreActual);
        if (string.IsNullOrWhiteSpace(nuevoNombre)) return;

        string nuevoLimiteStr = Microsoft.VisualBasic.Interaction.InputBox("Modificar límite diario global (Q):", "Modificar Límite", usuarioActual?.LimiteDiario.ToString() ?? "2000");
        if (!decimal.TryParse(nuevoLimiteStr, out decimal nuevoLimite)) nuevoLimite = 2000.00m;

        bool exito = await Contexto.Datos.ModificarUsuarioAsync(_idUsuarioSeleccionado.Value, nuevoNombre, usuarioActual?.Saldo ?? 0, nuevoLimite);
        if (exito)
        {
            MessageBox.Show("Datos y límites del usuario actualizados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ActualizarAsync();
        }
    }

    private async void BtnCambiarTarjeta_Click(object sender, EventArgs e)
    {
        if (!_idUsuarioSeleccionado.HasValue)
        {
            MessageBox.Show("Seleccione un usuario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string nuevaTarjeta = Microsoft.VisualBasic.Interaction.InputBox("Ingrese el nuevo número de tarjeta (16 dígitos):", "Cambio de Tarjeta (D-10 / RN-04)", "9988776655443322");
        if (nuevaTarjeta.Length != 16)
        {
            MessageBox.Show("La tarjeta debe tener 16 dígitos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Regla RN-04 / D-10: Al cambiar tarjeta se conserva el historial y lo retirado en el día
        bool exito = await Contexto.Datos.CambiarTarjetaUsuarioAsync(_idUsuarioSeleccionado.Value, nuevaTarjeta);
        if (exito)
        {
            MessageBox.Show("Tarjeta reemplazada con éxito. El límite consumido de hoy se ha conservado para la red (RN-04).", "Cambio de Tarjeta", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ActualizarAsync();
        }
    }

    private async void BtnBloquearDesbloquear_Click(object sender, EventArgs e)
    {
        if (!_idUsuarioSeleccionado.HasValue)
        {
            MessageBox.Show("Seleccione un usuario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var usuario = _usuarios.FirstOrDefault(x => x.IdUsuario == _idUsuarioSeleccionado.Value);
        if (usuario == null) return;

        string nuevoEstado = usuario.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? "Bloqueado" : "Activo";

        bool exito = await Contexto.Datos.CambiarEstadoUsuarioAsync(_idUsuarioSeleccionado.Value, nuevoEstado);
        if (exito)
        {
            MessageBox.Show($"El usuario ha sido cambiado a estado: {nuevoEstado}.", "Control de Acceso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ActualizarAsync();
        }
    }
}