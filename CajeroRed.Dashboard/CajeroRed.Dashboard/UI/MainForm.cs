using System.Drawing;
using System.Windows.Forms;
using CajeroRed.Dashboard.Modelos;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>Ventana principal del dashboard: menú lateral, encabezado, contenido y barra de estado.
/// Se actualiza sola cada 5 segundos sin congelarse.</summary>
public class MainForm : Form
{
    private const int AnchoLateral = 240;
    private const int SegundosActualizacion = 5;

    // Módulos del menú: clave, texto, subtítulo y etapa en la que se construye.
    private static readonly (string Clave, string Texto, string Subtitulo, string Etapa)[] Modulos =
    {
        ("resumen",       "Resumen",        "Indicadores del día y estado general de la red",        "Etapa 2"),
        ("cajeros",       "Cajeros",        "Monitor y gestión de los cajeros de la red",            "Etapas 2 y 3"),
        ("transacciones", "Transacciones",  "Operaciones de todos los cajeros en tiempo real",       "Etapa 4"),
        ("depositos",     "Depósitos",      "Consulta de depósitos con filtros y totales",           "Etapa 4"),
        ("retiros",       "Retiros",        "Retiros, rechazos y consumo del límite diario",         "Etapa 4"),
        ("cambiospin",    "Cambios de PIN", "Quién cambió su PIN, cuándo y en qué cajero",           "Etapa 4"),
        ("usuarios",      "Usuarios",       "Gestión y consulta de usuarios y tarjetas",             "Etapa 3"),
        ("alertas",       "Alertas",        "Alertas activas y atendidas",                           "Etapa 5"),
        ("reportes",      "Reportes",       "Exportación a PDF o Excel",                             "Etapa 5"),
        ("bitacora",      "Bitácora",       "Auditoría de acciones y eventos del sistema",           "Etapa 5"),
        ("configuracion", "Configuración",  "Parámetros del sistema",                                "Etapa 5"),
    };

    private readonly Dictionary<string, BotonMenu> _botones = new();
    private readonly Dictionary<string, PaginaBase> _paginas = new();
    private readonly Panel _contenido = new();
    private readonly Label _lblTitulo = new();
    private readonly Label _lblSubtitulo = new();
    private readonly Label _lblAlertas = new();
    private readonly Label _lblUsuario = new();
    private readonly Label _lblEstado = new();
    private readonly System.Windows.Forms.Timer _timerDatos = new();
    private readonly System.Windows.Forms.Timer _timerReloj = new();

    private string _claveActual = "";
    private bool _actualizando;

    /// <summary>True si el administrador pidió "Cerrar sesión" (se vuelve al login).</summary>
    public bool VolverALogin { get; private set; }

    public MainForm()
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = "ATM Control — Dashboard del servidor";
        Size = new Size(1320, 800);
        MinimumSize = new Size(1150, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Tema.Fondo;
        Font = Tema.Fuente(10f);

        var raiz = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty
        };
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, AnchoLateral));
        raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        raiz.Controls.Add(ConstruirLateral(), 0, 0);
        raiz.Controls.Add(ConstruirDerecha(), 1, 0);
        Controls.Add(raiz);

        // Una pantalla por módulo. En cada etapa se reemplaza el placeholder por la pantalla real,
        // por ejemplo:  _paginas["resumen"] = new PaginaResumen();
        foreach (var m in Modulos)
            _paginas[m.Clave] = new PaginaProximamente(m.Texto, m.Subtitulo, m.Etapa);

        // Pantallas ya construidas (reemplazan al aviso "Se construye en...")
        _paginas["resumen"] = new PaginaResumen();
        _paginas["cajeros"] = new PaginaCajeros();
        _paginas["usuarios"] = new PaginaUsuarios();
        _paginas["transacciones"] = new PaginaTransacciones();
        _paginas["depositos"] = new PaginaDepositos();
        _paginas["retiros"] = new PaginaRetiros();
        _paginas["cambiospin"] = new PaginaCambiosPin();
        _paginas["alertas"] = new PaginaAlertas();
        _paginas["reportes"] = new PaginaReportes();
        _paginas["configuracion"] = new PaginaConfiguracion();
        _paginas["bitacora"] = new PaginaBitacora(); 

        _timerDatos.Interval = SegundosActualizacion * 1000;
        _timerDatos.Tick += async (s, e) => await ActualizarTodoAsync();
        _timerReloj.Interval = 1000;
        _timerReloj.Tick += (s, e) => ActualizarReloj();

        Load += async (s, e) =>
        {
            ActualizarReloj();
            Navegar("resumen");
            await ActualizarTodoAsync();
            _timerDatos.Start();
            _timerReloj.Start();
        };
    }

    // ------------------------------------------------------------------ construcción de la interfaz

    private Control ConstruirLateral()
    {
        var lateral = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Tema.Lateral,
            Margin = Padding.Empty
        };
        lateral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        lateral.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        lateral.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lateral.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

        // Logo
        var logo = new Panel { Dock = DockStyle.Fill, BackColor = Tema.Lateral, Margin = Padding.Empty };
        logo.Controls.Add(new Label
        {
            Text = "ATM Control",
            ForeColor = Color.White,
            Font = Tema.Fuente(17f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(24, 16)
        });
        logo.Controls.Add(new Label
        {
            Text = "Centro de control de cajeros",
            ForeColor = Tema.TextoSuaveLateral,
            Font = Tema.Fuente(9f),
            AutoSize = true,
            Location = new Point(26, 52)
        });

        // Menú
        var menu = new Panel { Dock = DockStyle.Fill, BackColor = Tema.Lateral, Margin = Padding.Empty };
        int y = 8;
        foreach (var m in Modulos)
        {
            string clave = m.Clave;
            var boton = new BotonMenu(m.Texto) { Location = new Point(0, y), Width = AnchoLateral };
            boton.Click += (s, e) => Navegar(clave);
            _botones[clave] = boton;
            menu.Controls.Add(boton);
            y += 46;
        }

        // Pie: cerrar sesión
        var pie = new Panel { Dock = DockStyle.Fill, BackColor = Tema.Lateral, Margin = Padding.Empty };
        var salir = new BotonMenu("Cerrar sesión") { Location = new Point(0, 14), Width = AnchoLateral };
        salir.Click += (s, e) => CerrarSesion();
        pie.Controls.Add(salir);

        lateral.Controls.Add(logo, 0, 0);
        lateral.Controls.Add(menu, 0, 1);
        lateral.Controls.Add(pie, 0, 2);
        return lateral;
    }

    private Control ConstruirDerecha()
    {
        var derecha = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Tema.Fondo,
            Margin = Padding.Empty
        };
        derecha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        derecha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

        // ----- Encabezado -----
        var cabecera = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.White,
            Margin = Padding.Empty,
            Padding = new Padding(24, 0, 24, 0)
        };
        cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cabecera.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var titulos = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        _lblTitulo.AutoSize = true;
        _lblTitulo.Font = Tema.Fuente(16f, FontStyle.Bold);
        _lblTitulo.ForeColor = Tema.Texto;
        _lblTitulo.Location = new Point(0, 8);
        _lblSubtitulo.AutoSize = true;
        _lblSubtitulo.Font = Tema.Fuente(9.5f);
        _lblSubtitulo.ForeColor = Tema.TextoSuave;
        _lblSubtitulo.Location = new Point(2, 40);
        titulos.Controls.Add(_lblTitulo);
        titulos.Controls.Add(_lblSubtitulo);

        _lblAlertas.AutoSize = true;
        _lblAlertas.Anchor = AnchorStyles.None;
        _lblAlertas.Padding = new Padding(12, 6, 12, 6);
        _lblAlertas.Margin = new Padding(0, 0, 20, 0);
        _lblAlertas.Font = Tema.Fuente(9.5f, FontStyle.Bold);
        _lblAlertas.ForeColor = Color.White;
        _lblAlertas.BackColor = Tema.Verde;
        _lblAlertas.Text = "Sin alertas activas";
        _lblAlertas.Cursor = Cursors.Hand;
        _lblAlertas.Click += (s, e) => Navegar("alertas");

        _lblUsuario.AutoSize = true;
        _lblUsuario.Anchor = AnchorStyles.None;
        _lblUsuario.TextAlign = ContentAlignment.MiddleRight;
        _lblUsuario.Font = Tema.Fuente(9.5f);
        _lblUsuario.ForeColor = Tema.Texto;

        cabecera.Controls.Add(titulos, 0, 0);
        //cabecera.Controls.Add(_lblAlertas, 1, 0);
        cabecera.Controls.Add(_lblUsuario, 2, 0);

        // ----- Contenido -----
        _contenido.Dock = DockStyle.Fill;
        _contenido.Margin = Padding.Empty;
        _contenido.BackColor = Tema.Fondo;

        // ----- Barra de estado -----
        _lblEstado.Dock = DockStyle.Fill;
        _lblEstado.Margin = Padding.Empty;
        _lblEstado.Padding = new Padding(24, 0, 0, 0);
        _lblEstado.TextAlign = ContentAlignment.MiddleLeft;
        _lblEstado.BackColor = Tema.Borde;
        _lblEstado.ForeColor = Tema.TextoSuave;
        _lblEstado.Font = Tema.Fuente(8.5f);
        _lblEstado.Text = "Conectando...";

        derecha.Controls.Add(cabecera, 0, 0);
        derecha.Controls.Add(_contenido, 0, 1);
        derecha.Controls.Add(_lblEstado, 0, 2);
        return derecha;
    }

    // ------------------------------------------------------------------ navegación y actualización

    private void Navegar(string clave)
    {
        if (!_paginas.TryGetValue(clave, out var pagina))
            return;

        _claveActual = clave;
        foreach (var par in _botones)
            par.Value.Activo = par.Key == clave;

        _contenido.SuspendLayout();
        _contenido.Controls.Clear();
        _contenido.Controls.Add(pagina);
        _contenido.ResumeLayout();

        _lblTitulo.Text = pagina.Titulo;
        _lblSubtitulo.Text = pagina.Subtitulo;

        _ = ActualizarTodoAsync();
    }

    /// <summary>Consulta los datos y refresca encabezado y pantalla actual.
    /// Es async: la ventana no se congela mientras se consulta.</summary>
    private async Task ActualizarTodoAsync()
    {
        if (_actualizando) return; // evita solapar dos actualizaciones
        _actualizando = true;
        try
        {
            var tareaResumen = Contexto.Datos.ObtenerResumenAsync();
            var pagina = _paginas.GetValueOrDefault(_claveActual);
            Task tareaPagina = pagina != null ? pagina.ActualizarAsync() : Task.CompletedTask;

            await Task.WhenAll(tareaResumen, tareaPagina);
            ActualizarEncabezado(await tareaResumen);
        }
        catch (Exception ex)
        {
            // El dashboard no se cae si falla una consulta: muestra el error en la barra de estado.
            _lblEstado.ForeColor = Tema.Rojo;
            _lblEstado.Text = "Error al actualizar los datos: " + ex.Message;
        }
        finally
        {
            _actualizando = false;
        }
    }

    private void ActualizarEncabezado(ResumenDia r)
    {
        if (r.AlertasActivas == 0)
        {
            _lblAlertas.Text = "Sin alertas activas";
            _lblAlertas.BackColor = Tema.Verde;
        }
        else
        {
            _lblAlertas.Text = r.AlertasActivas == 1 ? "1 alerta activa" : $"{r.AlertasActivas} alertas activas";
            _lblAlertas.BackColor = Tema.Naranja;
        }

        _lblEstado.ForeColor = Tema.TextoSuave;
        _lblEstado.Text = $"Origen de datos: {Contexto.Datos.Descripcion}     |     " +
                          $"Cajeros en línea: {r.CajerosEnLinea}/{r.CajerosTotal}     |     " +
                          $"Última actualización: {DateTime.Now:HH:mm:ss}";
    }

    private void ActualizarReloj()
    {
        var admin = Contexto.AdminActual;
        string quien = admin == null ? "—" : $"{admin.Usuario} · {admin.Rol}";
        _lblUsuario.Text = $"{quien}\n{DateTime.Now:dd/MM/yyyy  HH:mm:ss}";
    }

    private void CerrarSesion()
    {
        var respuesta = MessageBox.Show("¿Deseas cerrar la sesión del administrador?", "Cerrar sesión",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (respuesta != DialogResult.Yes) return;

        VolverALogin = true;
        Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timerDatos.Stop();
        _timerReloj.Stop();
        _timerDatos.Dispose();
        _timerReloj.Dispose();
        Contexto.AdminActual = VolverALogin ? null : Contexto.AdminActual;
        base.OnFormClosed(e);
    }
}
