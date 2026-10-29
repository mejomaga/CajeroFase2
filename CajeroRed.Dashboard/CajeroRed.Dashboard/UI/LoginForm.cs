using System.Drawing;
using System.Windows.Forms;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>D-01. Inicio de sesión del administrador.</summary>
public class LoginForm : Form
{
    // Poner en false cuando ya no se necesite la ayuda de la demostración.
    private const bool MostrarAyudaDemo = true;

    private readonly TextBox _txtUsuario = new();
    private readonly TextBox _txtClave = new();
    private readonly Button _btnIngresar = new();
    private readonly Label _lblError = new();

    public LoginForm()
    {
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Text = "ATM Control — Iniciar sesión";
        ClientSize = new Size(900, 540);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.White;
        Font = Tema.Fuente(10f);

        // ---------- Panel izquierdo (marca) ----------
        var izquierdo = new Panel { Bounds = new Rectangle(0, 0, 380, 540), BackColor = Tema.Lateral };
        izquierdo.Controls.Add(Etiqueta("ATM Control", 40, 190, 26f, FontStyle.Bold, Color.White));
        var descripcion = Etiqueta("Centro de control de la red de cajeros automáticos", 42, 248, 11f, FontStyle.Regular, Tema.TextoLateral);
        descripcion.AutoSize = false;
        descripcion.Size = new Size(290, 60);
        izquierdo.Controls.Add(descripcion);
        izquierdo.Controls.Add(Etiqueta("Proyecto Cajero — Fase II", 42, 490, 9f, FontStyle.Regular, Tema.TextoSuaveLateral));

        // ---------- Panel derecho (formulario) ----------
        var derecho = new Panel { Bounds = new Rectangle(380, 0, 520, 540), BackColor = Color.White };
        derecho.Controls.Add(Etiqueta("Bienvenido", 62, 110, 22f, FontStyle.Bold, Tema.Texto));
        derecho.Controls.Add(Etiqueta("Ingresa con tu cuenta de administrador", 64, 164, 10.5f, FontStyle.Regular, Tema.TextoSuave));

        derecho.Controls.Add(Etiqueta("Usuario", 64, 222, 9.5f, FontStyle.Bold, Tema.Texto));
        _txtUsuario.Font = Tema.Fuente(12f);
        _txtUsuario.Location = new Point(62, 246);
        _txtUsuario.Width = 396;
        _txtUsuario.BorderStyle = BorderStyle.FixedSingle;
        _txtUsuario.PlaceholderText = "Nombre de usuario";
        derecho.Controls.Add(_txtUsuario);

        derecho.Controls.Add(Etiqueta("Contraseña", 64, 302, 9.5f, FontStyle.Bold, Tema.Texto));
        _txtClave.Font = Tema.Fuente(12f);
        _txtClave.Location = new Point(62, 326);
        _txtClave.Width = 396;
        _txtClave.BorderStyle = BorderStyle.FixedSingle;
        _txtClave.UseSystemPasswordChar = true;
        _txtClave.PlaceholderText = "Contraseña";
        derecho.Controls.Add(_txtClave);

        _btnIngresar.Text = "Iniciar sesión";
        _btnIngresar.Bounds = new Rectangle(62, 392, 396, 46);
        _btnIngresar.FlatStyle = FlatStyle.Flat;
        _btnIngresar.FlatAppearance.BorderSize = 0;
        _btnIngresar.BackColor = Tema.Acento;
        _btnIngresar.ForeColor = Color.White;
        _btnIngresar.Font = Tema.Fuente(11f, FontStyle.Bold);
        _btnIngresar.Cursor = Cursors.Hand;
        _btnIngresar.UseVisualStyleBackColor = false;
        _btnIngresar.Click += async (s, e) => await IntentarAsync();
        derecho.Controls.Add(_btnIngresar);

        _lblError.ForeColor = Tema.Rojo;
        _lblError.Font = Tema.Fuente(9.5f);
        _lblError.Bounds = new Rectangle(62, 450, 396, 40);
        derecho.Controls.Add(_lblError);

        if (MostrarAyudaDemo)
            derecho.Controls.Add(Etiqueta("Demostración: usuario admin, contraseña admin", 64, 498, 8.5f, FontStyle.Regular, Tema.TextoSuave));

        Controls.Add(izquierdo);
        Controls.Add(derecho);

        AcceptButton = _btnIngresar; // Enter = iniciar sesión
        Shown += (s, e) => _txtUsuario.Focus();
    }

    private static Label Etiqueta(string texto, int x, int y, float tamano, FontStyle estilo, Color color)
    {
        return new Label
        {
            Text = texto,
            AutoSize = true,
            Location = new Point(x, y),
            Font = Tema.Fuente(tamano, estilo),
            ForeColor = color
        };
    }

    private async Task IntentarAsync()
    {
        _lblError.Text = "";
        string usuario = _txtUsuario.Text.Trim();
        string clave = _txtClave.Text;

        if (usuario.Length == 0 || clave.Length == 0)
        {
            _lblError.Text = "Ingresa el usuario y la contraseña.";
            return;
        }

        _btnIngresar.Enabled = false;
        _btnIngresar.Text = "Verificando...";
        try
        {
            var admin = await Contexto.Datos.IniciarSesionAdminAsync(usuario, Seguridad.Sha256Hex(clave));
            if (admin == null)
            {
                _lblError.Text = "Usuario o contraseña incorrectos.";
                _txtClave.Clear();
                _txtClave.Focus();
                return;
            }

            Contexto.AdminActual = admin;
            DialogResult = DialogResult.OK; // cierra el login y abre el dashboard
        }
        catch (Exception ex)
        {
            _lblError.Text = "No se pudo iniciar sesión: " + ex.Message;
        }
        finally
        {
            _btnIngresar.Enabled = true;
            _btnIngresar.Text = "Iniciar sesión";
        }
    }
}
