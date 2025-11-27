using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace SaboresdeMama.Services
{
    public class FirebaseService
    {
        private static HttpClient _httpClient;
        private static string _projectId;
        private static string _accessToken;
        private static bool _initialized = false;

        private static string _projectIdValue = "TU_PROJECT_ID";
        private static string AccessToken = null;

        public static string ProjectId => _projectIdValue;

        public static void Configure(string projectId, string accessToken = null)
        {
            _projectIdValue = projectId;
            AccessToken = accessToken;
            _initialized = false;
            _httpClient = null;
        }

        private static void InitializeHttpClient()
        {
            if (_httpClient != null)
                return;

            if (string.IsNullOrEmpty(_projectIdValue) || _projectIdValue == "TU_PROJECT_ID")
                throw new InvalidOperationException("Debes configurar el Project ID de Firebase.");

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"https://firestore.googleapis.com/v1/projects/{_projectIdValue}/databases/(default)/documents/");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            if (!string.IsNullOrEmpty(AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            }
        }

        public static async Task<T> GetDocumentAsync<T>(string collection, string documentId) where T : class
        {
            InitializeHttpClient();
            
            try
            {
                var response = await _httpClient.GetAsync($"{collection}/{documentId}");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var firestoreDoc = JsonConvert.DeserializeObject<FirestoreDocument>(json);
                
                if (firestoreDoc?.Fields != null)
                {
                    var item = ConvertFromFirestore<T>(firestoreDoc.Fields);
                    // Asegurar que el ID se asigne correctamente
                    if (item is IHasId hasId && !string.IsNullOrEmpty(documentId))
                    {
                        hasId.Id = documentId;
                    }
                    return item;
                }
                
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error HTTP obteniendo documento: {httpEx.Message}");
                throw new InvalidOperationException(
                    "Error al comunicarse con Firebase. Verifica que el Project ID sea correcto y que tengas acceso a Firestore.", httpEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo documento: {ex.Message}");
                throw;
            }
        }

        public static async Task<List<T>> GetCollectionAsync<T>(string collection) where T : class
        {
            InitializeHttpClient();
            
            try
            {
                var response = await _httpClient.GetAsync(collection);
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var firestoreResponse = JsonConvert.DeserializeObject<FirestoreListResponse>(json);
                
                var results = new List<T>();
                if (firestoreResponse?.Documents != null)
                {
                    foreach (var doc in firestoreResponse.Documents)
                    {
                        if (doc.Fields != null)
                        {
                            var item = ConvertFromFirestore<T>(doc.Fields);
                            if (item != null)
                            {
                                // Extraer el ID del nombre del documento
                                var docId = doc.Name?.Split('/').LastOrDefault();
                                if (item is IHasId hasId && !string.IsNullOrEmpty(docId))
                                {
                                    hasId.Id = docId;
                                }
                                results.Add(item);
                            }
                        }
                    }
                }
                
                return results;
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error HTTP obteniendo colección: {httpEx.Message}");
                throw new InvalidOperationException(
                    "Error al comunicarse con Firebase. Verifica que el Project ID sea correcto y que tengas acceso a Firestore.", httpEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo colección: {ex.Message}");
                throw;
            }
        }

        public static async Task<string> AddDocumentAsync(string collection, Dictionary<string, object> data)
        {
            InitializeHttpClient();
            
            try
            {
                var firestoreFields = ConvertToFirestore(data);
                var requestBody = new { fields = firestoreFields };
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(collection, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Incluir el cuerpo completo de la respuesta para ver el mensaje exacto de Firebase
                    var msg = $"Error HTTP al agregar documento en colección '{collection}': " +
                              $"{(int)response.StatusCode} {response.StatusCode}. " +
                              $"Respuesta: {responseJson}";
                    System.Diagnostics.Debug.WriteLine(msg);
                    throw new InvalidOperationException(msg);
                }

                var result = JsonConvert.DeserializeObject<FirestoreDocument>(responseJson);
                
                // Extraer el ID del nombre del documento
                return result?.Name?.Split('/').LastOrDefault() ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error agregando documento: {ex.Message}");
                throw;
            }
        }

        public static async Task UpdateDocumentAsync(string collection, string documentId, Dictionary<string, object> data)
        {
            InitializeHttpClient();
            
            try
            {
                var firestoreFields = ConvertToFirestore(data);
                var requestBody = new { fields = firestoreFields };
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PatchAsync($"{collection}/{documentId}?updateMask.fieldPaths=" + string.Join("&updateMask.fieldPaths=", data.Keys), content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando documento: {ex.Message}");
                throw;
            }
        }

        public static async Task DeleteDocumentAsync(string collection, string documentId)
        {
            InitializeHttpClient();
            
            try
            {
                var response = await _httpClient.DeleteAsync($"{collection}/{documentId}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error eliminando documento: {ex.Message}");
                throw;
            }
        }

        private static Dictionary<string, FirestoreValue> ConvertToFirestore(Dictionary<string, object> data)
        {
            var result = new Dictionary<string, FirestoreValue>();
            foreach (var kvp in data)
            {
                result[kvp.Key] = ConvertToFirestoreValue(kvp.Value);
            }
            return result;
        }

        private static FirestoreValue ConvertToFirestoreValue(object value)
        {
            if (value == null)
                return new FirestoreValue { NullValue = "NULL_VALUE" };
            
            if (value is string str)
                return new FirestoreValue { StringValue = str };
            
            if (value is int || value is long)
                return new FirestoreValue { IntegerValue = Convert.ToInt64(value).ToString() };
            
            if (value is double || value is float || value is decimal)
                return new FirestoreValue { DoubleValue = Convert.ToDouble(value) };
            
            if (value is bool boolVal)
                return new FirestoreValue { BooleanValue = boolVal };
            
            if (value is DateTime dateTime)
                return new FirestoreValue { TimestampValue = dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") };
            
            // Para otros tipos, convertir a string
            return new FirestoreValue { StringValue = value.ToString() };
        }

        private static T ConvertFromFirestore<T>(Dictionary<string, FirestoreValue> fields) where T : class
        {
            var dict = new Dictionary<string, object>();
            
            foreach (var kvp in fields)
            {
                var value = ConvertFromFirestoreValue(kvp.Value);
                dict[kvp.Key] = value;
            }
            
            var json = JsonConvert.SerializeObject(dict);
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static object ConvertFromFirestoreValue(FirestoreValue value)
        {
            if (value.StringValue != null)
                return value.StringValue;
            
            if (value.IntegerValue != null)
                return long.Parse(value.IntegerValue);
            
            if (value.DoubleValue.HasValue)
                return value.DoubleValue.Value;
            
            if (value.BooleanValue.HasValue)
                return value.BooleanValue.Value;
            
            if (value.TimestampValue != null)
                return DateTime.Parse(value.TimestampValue);
            
            if (value.NullValue != null)
                return null;
            
            return null;
        }

        // Clases auxiliares para deserialización
        private class FirestoreDocument
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("fields")]
            public Dictionary<string, FirestoreValue> Fields { get; set; }
        }

        private class FirestoreListResponse
        {
            [JsonProperty("documents")]
            public List<FirestoreDocument> Documents { get; set; }
        }

        private class FirestoreValue
        {
            // Importante: solo se debe serializar UNA de estas propiedades.
            // Usamos NullValueHandling.Ignore para que las que estén en null NO se envíen en el JSON.

            [JsonProperty("stringValue", NullValueHandling = NullValueHandling.Ignore)]
            public string StringValue { get; set; }
            
            [JsonProperty("integerValue", NullValueHandling = NullValueHandling.Ignore)]
            public string IntegerValue { get; set; }
            
            [JsonProperty("doubleValue", NullValueHandling = NullValueHandling.Ignore)]
            public double? DoubleValue { get; set; }
            
            [JsonProperty("booleanValue", NullValueHandling = NullValueHandling.Ignore)]
            public bool? BooleanValue { get; set; }
            
            [JsonProperty("timestampValue", NullValueHandling = NullValueHandling.Ignore)]
            public string TimestampValue { get; set; }
            
            [JsonProperty("nullValue", NullValueHandling = NullValueHandling.Ignore)]
            public string NullValue { get; set; }
        }
    }

    // Interfaz para modelos que tienen ID
    public interface IHasId
    {
        string Id { get; set; }
    }
}
