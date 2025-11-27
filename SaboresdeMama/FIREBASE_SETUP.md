# Configuración de Firebase

Esta aplicación ahora usa Firebase Firestore como base de datos en lugar de SQLite.

## Pasos para configurar Firebase

### 1. Crear un proyecto en Firebase

1. Ve a [Firebase Console](https://console.firebase.google.com/)
2. Haz clic en "Agregar proyecto" o selecciona un proyecto existente
3. Sigue las instrucciones para crear/configurar el proyecto

### 2. Habilitar Firestore

1. En la consola de Firebase, ve a "Firestore Database"
2. Haz clic en "Crear base de datos"
3. Selecciona "Iniciar en modo de prueba" (para desarrollo) o "Iniciar en modo de producción"
4. Elige una ubicación para tu base de datos
5. Haz clic en "Habilitar"

### 3. Obtener credenciales de servicio

1. En Firebase Console, ve a "Configuración del proyecto" (ícono de engranaje)
2. Ve a la pestaña "Cuentas de servicio"
3. Haz clic en "Generar nueva clave privada"
4. Esto descargará un archivo JSON con tus credenciales
5. **IMPORTANTE**: Guarda este archivo de forma segura y no lo subas a repositorios públicos

### 4. Configurar la aplicación

Tienes dos opciones para configurar Firebase en la aplicación:

#### Opción A: Usar archivo de credenciales

1. Coloca el archivo JSON descargado en la carpeta de tu proyecto
2. En `MauiProgram.cs`, descomenta y actualiza la configuración:

```csharp
FirebaseService.Configure(
    projectId: "tu-project-id", // Reemplaza con tu Project ID
    credentialsPath: Path.Combine(FileSystem.AppDataDirectory, "firebase-credentials.json")
);
```

3. Asegúrate de que el archivo JSON esté incluido en el proyecto y se copie al directorio de salida

#### Opción B: Usar variable de entorno (recomendado para producción)

1. Establece la variable de entorno `GOOGLE_APPLICATION_CREDENTIALS` apuntando al archivo JSON
2. En `MauiProgram.cs`, solo configura el Project ID:

```csharp
FirebaseService.Configure(
    projectId: "tu-project-id" // Reemplaza con tu Project ID
);
```

### 5. Estructura de colecciones en Firestore

La aplicación creará automáticamente las siguientes colecciones:
- `usuarios` - Usuarios del sistema
- `productos` - Productos disponibles
- `recetas` - Recetas del recetario
- `pedidos` - Pedidos realizados
- `ventas` - Ventas completadas

### 6. Reglas de seguridad de Firestore

Para desarrollo, puedes usar estas reglas básicas (ajusta según tus necesidades de seguridad):

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /{document=**} {
      allow read, write: if request.auth != null;
    }
  }
}
```

**NOTA**: Estas reglas permiten lectura/escritura a cualquier usuario autenticado. Ajusta según tus necesidades de seguridad.

### 7. Usuarios por defecto

La aplicación creará automáticamente dos usuarios por defecto la primera vez que se ejecute:
- **Admin**: Username: `admin`, Password: `admin123`
- **Cliente**: Username: `cliente`, Password: `cliente123`

**IMPORTANTE**: Cambia estas contraseñas en producción.

## Solución de problemas

### Error: "Debes configurar el Project ID de Firebase"
- Asegúrate de haber llamado `FirebaseService.Configure()` con un Project ID válido

### Error: "Error al inicializar Firebase"
- Verifica que el archivo de credenciales existe y es válido
- Asegúrate de que el Project ID sea correcto
- Verifica que Firestore esté habilitado en tu proyecto de Firebase

### Error de permisos
- Verifica las reglas de seguridad de Firestore
- Asegúrate de que las credenciales tengan los permisos necesarios

## Migración de datos

Si tenías datos en SQLite, necesitarás migrarlos manualmente a Firestore o crear un script de migración.

