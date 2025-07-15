# Día 01: Fundamentos y Publicación de tu Primera API

**Objetivos del Día:**

Al finalizar este laboratorio, serás capaz de:

1.  Entender el rol de Azure API Management (APIM) como un proxy inteligente.
2.  Crear una instancia de APIM usando la Azure CLI.
3.  Importar una API existente (tu API local de ASP.NET Core) a APIM.
4.  Comprender y crear **Productos** para agrupar y exponer APIs.
5.  Probar el flujo de la petición a través del Gateway de APIM.
6.  Asegurar tu API con el método más básico: la **Clave de Suscripción**.


## Parte 1: Preparación - La API "Desnuda"

1.  **Prepara el Código:** Abre el proyecto en VS Code. Navega a `src/MiApiDeProductos/Program.cs` y asegúrate de que su contenido sea el siguiente para evitar errores de compilación. La clave es que las definiciones de `Product` y `ProductRepository` estén al final del archivo.

    ```csharp
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // --- Endpoints de la API ---
    app.MapGet("/api/products", () => 
    {
        return Results.Ok(ProductRepository.Products);
    });
    // ---------------------------

    app.Run();

    // --- Definición del Modelo y Datos para el Curso ---
    public record Product(int Id, string Name, double Price, int Stock);

    public static class ProductRepository
    {
        public static List<Product> Products { get; } = new List<Product>
        {
            new(1, "Laptop Gamer Pro", 2100.50, 15),
            new(2, "Teclado Mecánico RGB", 120.00, 50),
            new(3, "Monitor Curvo 34\"", 899.99, 10),
            new(4, "Webcam 4K con Aro de Luz", 150.75, 30)
        };
    }
    ```

2.  **Inicia la API:** Abre una terminal integrada en VS Code (`Ctrl+Shift+Ñ`) y ejecuta:
    
    ```bash
    cd src/MiApiDeProductos
    dotnet run
    ```
    La terminal te mostrará que la aplicación está escuchando, algo como `Now listening on: http://localhost:5123`. **Anota este puerto**.

3.  **Verifica la API Localmente:**
    *   Abre tu navegador y navega a `http://localhost:XXXX/api/products` (reemplaza `XXXX` con tu puerto). Deberías ver tus datos.
    *   Navega a `http://localhost:XXXX/swagger`. Deberías ver la interfaz de Swagger.


## Parte 2: Creación del Gestor de APIs

### Paso 1: Define Variables de Entorno
En tu terminal, define las siguientes variables para que los comandos sean más limpios y reutilizables.

```bash
RESOURCE_GROUP="rg-curso-apim"
APIM_NAME="apim-curso-`echo $RANDOM`"
LOCATION="eastus"
PUBLISHER_EMAIL="tu-correo@ejemplo.com" 
```

### Paso 2: Verificar y Establecer la Suscripción Activa
*(NOTA: Antes de crear recursos, es crucial asegurarse de que estamos trabajando en la suscripción de Azure correcta, especialmente si tienes acceso a varias.)*

**A. Lista tus suscripciones:**
Para ver todas las suscripciones disponibles, sus IDs y cuál está configurada como "Default", ejecuta:

```bash
az account list --query "[].{Name:name, SubscriptionId:id, IsDefault:isDefault}" --output table
```

Localiza el `SubscriptionId` de la suscripción que quieres usar para este curso.

**B. Verifica la suscripción activa actualmente:**
Para ver en qué suscripción estás trabajando en este preciso momento, ejecuta:

```bash
az account show --query "{Name:name, SubscriptionId:id}" --output table
```

**C. Establece la suscripción correcta:**
Si la suscripción activa no es la que deseas, ejecuta el siguiente comando. Reemplaza `ID_DE_TU_SUSCRIPCION` con el ID correcto que copiaste del primer comando. **Este es el paso más importante para evitar errores.**

```bash
az account set --subscription "ID_DE_TU_SUSCRIPCION"
```

### Paso 3: Crea el Grupo de Recursos
Una vez que estás seguro de estar en la suscripción correcta, crea el grupo de recursos donde vivirán todos nuestros componentes.

```bash
az group create --name $RESOURCE_GROUP --location $LOCATION
```

### Paso 4: Crea la Instancia de APIM
*(NOTA: Este comando puede tardar entre 30 y 45 minutos. Lo escribimos en una sola línea para evitar problemas al copiar y pegar en terminales Linux/WSL).*

```bash
az apim create --name $APIM_NAME --resource-group $RESOURCE_GROUP --location $LOCATION --publisher-email $PUBLISHER_EMAIL --publisher-name "Cursos Inc." --sku-name Developer
```

## Parte 3: El Puente - Importando la API

*(NOTA: Una vez que tu instancia de APIM esté creada, es hora de enseñarle sobre nuestra API local. Para ello, necesitamos que tanto nuestra API como el túnel de `ngrok` estén funcionando.)*

### Paso 1: Inicia tus Servicios Locales (API y Ngrok)

**A. Recordatorio: Asegúrate de que tu API local esté en ejecución.**

Ngrok necesita un servicio local al que apuntar. Si tu API no está corriendo, el túnel no funcionará correctamente.

*   Abre una terminal, navega a la carpeta de la API y ejecútala:
    
    ```bash
    cd src/MiApiDeProductos
    dotnet run
    ```
*   Esta terminal debe permanecer abierta durante todo el laboratorio. Anota el puerto en el que se está ejecutando (ej. `http://localhost:5123`).

**B. Expón tu API con Ngrok.**

Ahora, crearemos el túnel desde internet hacia tu API local.

*   **Abre una nueva terminal** (deja la de la API corriendo).
*   Ejecuta el siguiente comando, reemplazando `XXXX` con el puerto de tu API y `tu-dominio-estatico.ngrok-free.app` con el dominio que obtuviste en el Día 00:
    
    ```bash
    ngrok http --domain=tu-dominio-estatico.ngrok-free.app XXXX
    ```
*   Ngrok te mostrará la URL pública de tu túnel. **Copia esta URL completa** (ej. `https://tu-dominio-estatico.ngrok-free.app`).
*   *(Recordatorio: Cuando visites esta URL por primera vez en un navegador, deberás hacer clic en "Visit Site" en una página de advertencia. Esto es normal y no afecta a las llamadas de API).*

### Paso 2: Importa la API en el Portal de Azure

Con tus servicios locales listos, ahora podemos configurar APIM.

1.  **Navega al Portal de Azure:** Busca el recurso de API Management que creaste.
2.  **Inicia el Proceso de Importación:**
    *   En el menú de la izquierda, ve a **"APIs"** y haz clic en **"+ Add API"**.
    *   Selecciona **"OpenAPI"**.
3.  **Configura la Importación (Método Robusto):**
    *   **OpenAPI Specification:**
        *   Abre una nueva pestaña en tu navegador y ve a `http://localhost:XXXX/swagger/v1/swagger.json`.
        *   Haz clic derecho en la página y selecciona **"Guardar como..."**. Guarda el archivo como `swagger.json`.
        *   Vuelve al portal y haz clic en el botón **"Select a file"**. Selecciona el archivo `swagger.json` que acabas de guardar.
    *   **API URL suffix:** Escribe `productos`. **(Este paso es importante para estandarizar la URL)**.
    *   Haz clic en **"Create"**.

### Paso 3: Configura la URL del Backend

1.  La API ya está importada. Ve a la pestaña **"Settings"** de la API recién creada.
2.  En **"Web service URL"**, pega la URL de `ngrok` que copiaste en el Paso 1.
3.  Haz clic en **"Save"**.


## Parte 4: La Vitrina - Productos y Suscripciones

*(NOTA: "No exponemos las APIs directamente. Las exponemos a través de 'Productos'. Un producto es un paquete de una o más APIs con ciertas condiciones de uso, como límites de llamadas.")*

1.  **Crea un Producto:**
    *   En el menú de la izquierda de tu APIM, ve a **"Productos"**.
    *   Haz clic en **"+ Añadir"**.
    *   **Nombre para mostrar:** `Producto Estándar`
    *   **ID:** `producto-estandar`
    *   **Descripción:** `Acceso básico a las APIs del curso.`
    *   Asegúrate de que la casilla **"Published"** esté marcada.
    *   **NO** marques "Requires approval".
    *   Deja "Requires subscription" **marcado**.
    *   Haz clic en la pestaña **"+ APIs"**.
    *   Selecciona la API "MiApiDeProductos" de la lista para añadirla a este producto.
    *   Haz clic en **"Create"**.


> ### Alternativa con Azure CLI
>
> Al igual que con la importación de APIs, la creación y gestión de Productos se puede y debe automatizar. Los siguientes comandos replican el proceso del portal.
>
> 1.  **Crea el Producto:**
>     ```bash
>     az apim product create --resource-group $RESOURCE_GROUP --service-name $APIM_NAME --product-id "producto-estandar-cli" --display-name "Producto Estándar (CLI)" --description "Acceso básico a las APIs del curso, creado desde la CLI." --subscription-required true --approval-required false --state published
>     ```
>
> 2.  **Asocia la API al Producto:**
>     ```bash
>     az apim product api add --resource-group $RESOURCE_GROUP --service-name $APIM_NAME --product-id "producto-estandar-cli" --api-id "miapideproductos"
>     ```
>


## Parte 5: La Prueba de Fuego - Testing

### Prueba 1: Desde el Portal de Azure

1.  Ve al menú **"APIs"** -> **"MiApiDeProductos"** -> pestaña **"Prueba"**.
2.  Selecciona la operación `GET /api/products` y haz clic en **"Enviar"**. Deberías obtener un `200 OK`.

### Prueba 2: Desde un Cliente Externo (VS Code)

1.  **Prepara el Archivo de Peticiones:**
    *   Crea `requests/module-1.http` con este contenido (nota la ruta `/productos/` y las variables `{{...}}`):
        ```http
        ### PRUEBA 1: Llamada sin clave (FALLARÁ)
        GET https://{{APIM_NAME}}.azure-api.net/productos/api/products

        ### PRUEBA 2: Llamada con clave (FUNCIONARÁ)
        GET https://{{APIM_NAME}}.azure-api.net/productos/api/products
        Ocp-Apim-Subscription-Key: {{subscriptionKey}}
        ```

2.  **Configura las Variables (Método Robusto):**
    *   **Crea una carpeta `.vscode`** en la raíz de tu proyecto si no existe.
    *   Dentro de `.vscode`, crea un nuevo archivo llamado **`settings.json`**.
    *   Pega el siguiente contenido en `settings.json`:
        ```json
        {
            "rest-client.environmentVariables": {
                "$shared": {},
                "dev": {
                    "APIM_NAME": "apim-curso-msouga-29804",
                    "subscriptionKey": "PEGA_AQUI_TU_CLAVE"
                }
            }
        }
        ```
    *   **Reemplaza** los valores de `APIM_NAME` y `subscriptionKey` con los tuyos. Para obtener la `subscriptionKey`, ve al Portal de Azure -> Productos -> Producto Estándar -> Suscripciones -> y muestra la clave.
    *   Para activar el entorno, abre la Paleta de Comandos (`Ctrl+Shift+P` en Ubuntu, `Cmd+Shift+P` en Mac), busca **"REST Client: Switch Environment"** y selecciona **`dev`**.

3.  **Ejecuta las Pruebas:**
    *   Abre `module-1.http`. Haz clic en **"Send Request"** sobre la primera petición. El resultado esperado es **`401 Unauthorized`**.
    *   Haz clic en **"Send Request"** sobre la segunda petición. El resultado esperado es **`200 OK`**.


## Resumen y Conclusiones del Día

Hoy hemos realizado el viaje completo de una API "desnuda" a una API gestionada. Hemos aprendido que APIM actúa como una fachada segura, que las APIs se exponen a través de Productos y que el acceso se controla, como mínimo, con Claves de Suscripción.

**Próximos Pasos:** En el próximo módulo, exploraremos políticas de seguridad mucho más avanzadas, como la limitación de velocidad (rate limiting) y la validación de tokens JWT.