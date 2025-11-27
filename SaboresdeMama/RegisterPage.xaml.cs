using SaboresdeMama.Models;
using SaboresdeMama.Services;

namespace SaboresdeMama;

public partial class RegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public RegisterPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // 1. Validaciones
        if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
            string.IsNullOrWhiteSpace(UsernameEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            MostrarError("Por favor, completa todos los campos.");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            MostrarError("Las contraseñas no coinciden.");
            return;
        }

        IsBusy(true);

        try
        {
            // 2. Verificar si ya existe
            var usuarioExistente = await _databaseService.GetUsuarioByUsernameAsync(UsernameEntry.Text.Trim());

            if (usuarioExistente != null)
            {
                MostrarError("Ese usuario ya está registrado.");
                IsBusy(false);
                return;
            }

            // 3. Crear el objeto Usuario
            var nuevoUsuario = new Usuario
            {
                Nombre = NombreEntry.Text.Trim(),
                Username = UsernameEntry.Text.Trim(),
                Password = PasswordEntry.Text.Trim(),
                TipoUsuario = "Cliente", // Por defecto creamos Clientes
                FechaRegistro = DateTime.Now
            };

            // 4. Guardar usuario
            await _databaseService.AddUsuarioAsync(nuevoUsuario);

            IsBusy(false);
            await DisplayAlert("¡Bienvenido!", "Cuenta creada exitosamente. Por favor inicia sesión.", "OK");

            // Volver atrás
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            IsBusy(false);
            MostrarError($"Error: {ex.Message}");
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void MostrarError(string mensaje)
    {
        ErrorLabel.Text = mensaje;
        ErrorLabel.IsVisible = true;
    }

    private void IsBusy(bool busy)
    {
        LoadingIndicator.IsVisible = busy;
        LoadingIndicator.IsRunning = busy;
        RegisterButton.IsEnabled = !busy;
        ErrorLabel.IsVisible = false;
    }
}