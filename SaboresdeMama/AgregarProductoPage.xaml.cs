using SaboresdeMama.Models;
using SaboresdeMama.Services;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace SaboresdeMama
{
    public partial class AgregarProductoPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public AgregarProductoPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private async void OnSeleccionarImagenClicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();

                if (photo != null)
                {
                    ProductoImage.Source = ImageSource.FromStream(() => photo.OpenReadAsync().Result);
                    await DisplayAlert("Subiendo...", "La imagen se está subiendo al servidor. Espere...", "OK");

                    string newImageUrl = await _databaseService.UploadImageAsync(photo.FullPath);

                    if (!string.IsNullOrWhiteSpace(newImageUrl))
                    {
                        ImageURLEntry.Text = newImageUrl;
                        await DisplayAlert("Éxito", "Imagen subida correctamente", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "La subida de la imagen falló. Intente de nuevo.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al seleccionar/subir imagen: {ex.Message}");
                ErrorLabel.Text = $"Error al seleccionar/subir imagen: {ex.Message}";
                ErrorLabel.IsVisible = true;
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NombreEntry.Text) ||
                string.IsNullOrWhiteSpace(DescripcionEditor.Text) ||
                string.IsNullOrWhiteSpace(PrecioEntry.Text) ||
                string.IsNullOrWhiteSpace(StockEntry.Text))
            {
                ErrorLabel.Text = "Por favor, complete todos los campos obligatorios (*)";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (!double.TryParse(PrecioEntry.Text, out double precio) || precio < 0)
            {
                ErrorLabel.Text = "El precio debe ser un número válido mayor o igual a 0";
                ErrorLabel.IsVisible = true;
                return;
            }

            if (!int.TryParse(StockEntry.Text, out int stock) || stock < 0)
            {
                ErrorLabel.Text = "El stock debe ser un número entero válido mayor o igual a 0";
                ErrorLabel.IsVisible = true;
                return;
            }

            var nuevoProducto = new Producto
            {
                Nombre = NombreEntry.Text.Trim(),
                Descripcion = DescripcionEditor.Text.Trim(),
                Precio = precio,
                Stock = stock,
                Categoria = CategoriaEntry.Text?.Trim(),
                InformacionExtra = InformacionExtraEditor.Text?.Trim(),
                FechaCreacion = DateTime.Now,
                ImageURL = ImageURLEntry.Text?.Trim()
            };

            await _databaseService.AddProductoAsync(nuevoProducto);
            await DisplayAlert("Éxito", "Producto agregado correctamente", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}