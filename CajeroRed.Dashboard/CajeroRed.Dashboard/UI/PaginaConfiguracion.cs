using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CajeroRed.Dashboard.Servicios;

namespace CajeroRed.Dashboard.UI;

/// <summary>Etapa 5. Configuración global: puerto, umbral de efectivo bajo, vigencia del token y montos máximos (BD).</summary>
public class PaginaConfiguracion : PaginaBase
{
    private readonly NumericUpDown _numPuerto = new();
    private readonly NumericUpDown _numUmbralEfectivo = new();
    private readonly NumericUpDown _numVigenciaToken = new();
    private readonly NumericUpDown _numMontoMaximo = new();
    private readonly Button _btnGuardar = new();
    private readonly Label _lblEstado = new();

    public override string Titulo => "Configuración Global del Sistema";
    public override string Subtitulo => "Puerto de red, umbral de efectivo bajo, vigencia de token y montos máximos (Almacenado en BD)";

    public PaginaConfiguracion()
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
        raiz.RowStyles.Add(new RowStyle(SizeType.Absolute, 380)); // Altura ampliada para evitar cortes
        raiz.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var tarjetaConfig = CrearPanelConfiguracion();
        raiz.Controls.Add(tarjetaConfig, 0, 0);

        Controls.Add(raiz);
    }

    private Control CrearPanelConfiguracion()
    {
        var panelContenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0) };
        var tarjeta = new TarjetaTitulada("Parámetros del Sistema (Base de Datos Central)", panelContenedor)
        {
            Dock = DockStyle.Top,
            Height = 360, // Tarjeta más alta y holgada
            Margin = new Padding(0)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(15, 15, 15, 15)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));

        // 1. Puerto de Comunicación
        var lblPuerto = new Label { Text = "Puerto de Red / Sockets:", AutoSize = true, Font = Tema.Fuente(9.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
        _numPuerto.Dock = DockStyle.Fill;
        _numPuerto.Font = Tema.Fuente(10f);
        _numPuerto.Minimum = 1024;
        _numPuerto.Maximum = 65535;
        _numPuerto.Value = 8080;
        _numPuerto.Margin = new Padding(0, 8, 0, 8);

        // 2. Umbral de Efectivo Bajo (Por defecto Q 1,000.00)
        var lblUmbral = new Label { Text = "Umbral Alerta Efectivo Bajo (Q):", AutoSize = true, Font = Tema.Fuente(9.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
        _numUmbralEfectivo.Dock = DockStyle.Fill;
        _numUmbralEfectivo.Font = Tema.Fuente(10f);
        _numUmbralEfectivo.Minimum = 100m;
        _numUmbralEfectivo.Maximum = 50000m;
        _numUmbralEfectivo.Value = 1000m;
        _numUmbralEfectivo.DecimalPlaces = 2;
        _numUmbralEfectivo.Margin = new Padding(0, 8, 0, 8);

        // 3. Vigencia del Token
        var lblToken = new Label { Text = "Vigencia del Token (Minutos):", AutoSize = true, Font = Tema.Fuente(9.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
        _numVigenciaToken.Dock = DockStyle.Fill;
        _numVigenciaToken.Font = Tema.Fuente(10f);
        _numVigenciaToken.Minimum = 1;
        _numVigenciaToken.Maximum = 60;
        _numVigenciaToken.Value = 5;
        _numVigenciaToken.Margin = new Padding(0, 8, 0, 8);

        // 4. Montos Máximos de Retiro por Transacción
        var lblMaximo = new Label { Text = "Monto Máximo por Retiro (Q):", AutoSize = true, Font = Tema.Fuente(9.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
        _numMontoMaximo.Dock = DockStyle.Fill;
        _numMontoMaximo.Font = Tema.Fuente(10f);
        _numMontoMaximo.Minimum = 500m;
        _numMontoMaximo.Maximum = 50000m;
        _numMontoMaximo.Value = 5000m;
        _numMontoMaximo.DecimalPlaces = 2;
        _numMontoMaximo.Margin = new Padding(0, 8, 0, 8);

        // 5. Botón de Guardar y Estado (Ancho de botón holgado a 250px)
        var panelAccion = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 8, 0, 0) };

        _btnGuardar.Text = "💾 Guardar en Base de Datos";
        _btnGuardar.BackColor = Color.FromArgb(37, 99, 235);
        _btnGuardar.ForeColor = Color.White;
        _btnGuardar.FlatStyle = FlatStyle.Flat;
        _btnGuardar.Font = Tema.Fuente(9.5f, FontStyle.Bold);
        _btnGuardar.Width = 250; // Amplio para que quepa todo el texto sin recortes
        _btnGuardar.Height = 40;
        _btnGuardar.Cursor = Cursors.Hand;
        _btnGuardar.FlatAppearance.BorderSize = 0;
        _btnGuardar.Click += BtnGuardar_Click;

        _lblEstado.Text = "Parámetros listos para sincronizarse con la BD de SQL Server.";
        _lblEstado.Font = Tema.Fuente(9f);
        _lblEstado.ForeColor = Color.DimGray;
        _lblEstado.AutoSize = true;
        _lblEstado.Margin = new Padding(15, 12, 0, 0);

        panelAccion.Controls.Add(_btnGuardar);
        panelAccion.Controls.Add(_lblEstado);

        layout.Controls.Add(lblPuerto, 0, 0);
        layout.Controls.Add(_numPuerto, 1, 0);
        layout.Controls.Add(lblUmbral, 0, 1);
        layout.Controls.Add(_numUmbralEfectivo, 1, 1);
        layout.Controls.Add(lblToken, 0, 2);
        layout.Controls.Add(_numVigenciaToken, 1, 2);
        layout.Controls.Add(lblMaximo, 0, 3);
        layout.Controls.Add(_numMontoMaximo, 1, 3);
        layout.Controls.Add(panelAccion, 1, 4);

        panelContenedor.Controls.Add(layout);
        return tarjeta;
    }

    private async void BtnGuardar_Click(object sender, EventArgs e)
    {
        try
        {
            // Usamos los nombres EXACTOS que tiene tu tabla en SQL Server (como se ve en tu imagen)
            await Contexto.Datos.GuardarConfiguracionAsync("PUERTO_SERVIDOR", ((int)_numPuerto.Value).ToString());
            await Contexto.Datos.GuardarConfiguracionAsync("UMBRAL_EFECTIVO_BAJO", _numUmbralEfectivo.Value.ToString("F2"));
            await Contexto.Datos.GuardarConfiguracionAsync("VIGENCIA_TOKEN_MINUTOS", ((int)_numVigenciaToken.Value).ToString());
            await Contexto.Datos.GuardarConfiguracionAsync("MAX_CAPACIDAD_CAJERO", _numMontoMaximo.Value.ToString("F2"));

            MessageBox.Show(
                $"¡Parámetros guardados exitosamente en la Base de Datos!\n\n" +
                $"• Puerto de red: {_numPuerto.Value}\n" +
                $"• Umbral efectivo bajo: Q {_numUmbralEfectivo.Value:N2}\n" +
                $"• Vigencia del token: {_numVigenciaToken.Value} minutos\n" +
                $"• Monto máximo por retiro: Q {_numMontoMaximo.Value:N2}",
                "Configuración Actualizada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            _lblEstado.Text = $"Última sincronización con BD: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar en BD: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public override async Task ActualizarAsync()
    {
        try
        {
            var config = await Contexto.Datos.ObtenerConfiguracionAsync();

            if (config.ContainsKey("PUERTO_SERVIDOR") && int.TryParse(config["PUERTO_SERVIDOR"], out int puerto))
                _numPuerto.Value = puerto;

            if (config.ContainsKey("UMBRAL_EFECTIVO_BAJO") && decimal.TryParse(config["UMBRAL_EFECTIVO_BAJO"], out decimal umbral))
                _numUmbralEfectivo.Value = umbral;

            if (config.ContainsKey("VIGENCIA_TOKEN_MINUTOS") && int.TryParse(config["VIGENCIA_TOKEN_MINUTOS"], out int token))
                _numVigenciaToken.Value = token;

            if (config.ContainsKey("MAX_CAPACIDAD_CAJERO") && decimal.TryParse(config["MAX_CAPACIDAD_CAJERO"], out decimal maximo))
                _numMontoMaximo.Value = maximo;
        }
        catch
        {
            // Si falla la lectura (ej. por primera vez), dejamos los valores por defecto
        }
    }
}