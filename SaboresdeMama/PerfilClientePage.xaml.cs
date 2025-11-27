using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama;

public partial class PerfilClientePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private Usuario _usuarioActual;

    public PerfilClientePage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CargarUsuario();
    }

    private void CargarUsuario()
    {
        _usuarioActual = AuthService.UsuarioActual;
        if (_usuarioActual == null)
        {
            EstadoLabel.Text = "Debes iniciar sesión.";
            EstadoLabel.IsVisible = true;
            GuardarButton.IsEnabled = false;
            return;
        }

        CorreoEntry.Text = _usuarioActual.Username;
        NombreEntry.Text = _usuarioActual.Nombre;
        TelefonoEntry.Text = _usuarioActual.Telefono;
        DireccionEditor.Text = _usuarioActual.Direccion;
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (_usuarioActual == null)
            return;

        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            EstadoLabel.Text = "El nombre es obligatorio.";
            EstadoLabel.TextColor = Colors.Red;
            EstadoLabel.IsVisible = true;
            return;
        }

        GuardarButton.IsEnabled = false;
        EstadoLabel.IsVisible = false;

        try
        {
            _usuarioActual.Nombre = NombreEntry.Text?.Trim();
            _usuarioActual.Telefono = TelefonoEntry.Text?.Trim();
            _usuarioActual.Direccion = DireccionEditor.Text?.Trim();

            await _databaseService.UpdateUsuarioAsync(_usuarioActual);
            AuthService.SetUsuarioActual(_usuarioActual);

            EstadoLabel.Text = "Perfil actualizado correctamente.";
            EstadoLabel.TextColor = Colors.Green;
            EstadoLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            EstadoLabel.Text = $"Error al guardar: {ex.Message}";
            EstadoLabel.TextColor = Colors.Red;
            EstadoLabel.IsVisible = true;
        }
        finally
        {
            GuardarButton.IsEnabled = true;
        }
    }
}

