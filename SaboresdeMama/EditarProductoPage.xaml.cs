using SaboresdeMama.Models;
using SaboresdeMama.Services;
using Microsoft.Maui.Controls.Compatibility;
using System.Diagnostics;

namespace SaboresdeMama
{
    [QueryProperty(nameof(Producto), "Producto")]
    public partial class EditarProductoPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Producto _producto;

        public Producto Producto
        {
            get => _producto;
            set
            {
                _producto = value;
                if (_producto != null)
                {
                    OnPropertyChanged();
                    CargarDatosProducto();
                }
            }
        }

        public EditarProductoPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private void CargarDatosProducto()
        {
            if (Producto != null)
            {
                // Carga de campos existentes...
                // ...

                // Cargar la URL de la imagen si existe
                if (!string.IsNullOrWhiteSpace(Producto.ImageURL))
                {
                    ImageURLEntry.Text = Producto.ImageURL;
                    ProductoImage.Source = Producto.ImageURL;
                }
                else
                {
                    ProductoImage.Source = "placeholder_image.png";
                }
            }
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
                        await DisplayAlert("Error", "La subida de la imagen falló.", "OK");
                        ProductoImage.Source = Producto.ImageURL ?? "placeholder_image.png";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al seleccionar/subir imagen: {ex.Message}");
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            // ... Validaciones ...

            Producto.Nombre = NombreEntry.Text.Trim();
            // ... otros campos
            Producto.ImageURL = ImageURLEntry.Text?.Trim(); // Guardar la URL

            await _databaseService.UpdateProductoAsync(Producto);
            await DisplayAlert("Éxito", "Producto actualizado correctamente", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}