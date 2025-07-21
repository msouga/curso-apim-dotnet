### Azure Api Manager

# **Día 03: Laboratorio - Transformación, Rendimiento y Resiliencia (Versión Completa y Corregida)**

**Objetivos del Día:**

Al finalizar este laboratorio, serás capaz de:

1.  Mejorar drásticamente el rendimiento de tu API implementando políticas de **caché**.
2.  Aumentar la flexibilidad de tu API **transformando** peticiones sobre la marcha.
3.  Hacer tu API más robusta y profesional implementando un **manejo de errores** personalizado.
4.  (Bonus) Crear APIs compuestas que agreguen valor llamando a múltiples backends con una sola petición.

**Prerrequisitos:**

*   Haber completado exitosamente los laboratorios de los Días 1 y 2.
*   Tu instancia de Azure API Management (APIM) está funcionando.
*   Tu API local (`MiApiDeProductos`) debe estar ejecutándose.
*   El túnel de `ngrok` debe estar activo y apuntando a tu API local.
*   La URL del backend en tu API de APIM debe estar configurada con la URL de `ngrok`.
*   Tener a mano tu clave de suscripción del "Producto Estándar".
*   **Importante:** Para este laboratorio, asegúrate de haber **eliminado o comentado la política `<validate-jwt>`** que añadimos en el Día 2 en la operación `GET /api/products`. Esto simplificará las pruebas de hoy.
    *   ***Solución de Problemas:*** *Si recibes un error `401 Unauthorized` durante las pruebas de hoy, lo más probable es que hayas olvidado quitar esta política.*

> **NOTA CRÍTICA SOBRE COPIAR Y PEGAR POLÍTICAS**
>
> Cuando el manual indique "Reemplaza el contenido de tu política con el siguiente bloque", significa que debes **borrar todo el contenido del editor de políticas del portal (incluyendo las etiquetas `<policies>` de ejemplo) y pegar el bloque completo proporcionado**. Esto evita errores de XML con múltiples elementos raíz.


## **Parte 1: Mejora de Rendimiento con Caching**

**Escenario:** Nuestra operación `GET /api/products` devuelve una lista que no cambia muy a menudo. En lugar de contactar a nuestro backend en cada petición, vamos a guardar la respuesta en el caché de APIM durante 2 minutos para servirla casi instantáneamente.

1.  **Aplica la Política de Caché (Método Portal):**
    *   Navega a tu instancia de APIM -> APIs -> `MiApiDeProductos` -> operación `GET /api/products`.
    *   Abre el editor de políticas (`</>`).
    *   Reemplaza el contenido de tu política con el siguiente bloque.

        ```xml
        <policies>
            <inbound>
                <base />
                <!-- 1. Busca una respuesta en el caché. Si la encuentra, la devuelve y termina el flujo. -->
                <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none" must-revalidate="true" />
            </inbound>
            <backend>
                <base />
            </backend>
            <outbound>
                <base />
                <!-- 2. Si la petición llegó hasta aquí, significa que no había nada en el caché. La guardamos. -->
                <cache-store duration="120" /> <!-- Duración en segundos (2 minutos) -->
            </outbound>
            <on-error>
                <base />
            </on-error>
        </policies>
        ```
    *   Haz clic en **`Save`**.

	> #### Alternativa con Azure CLI
	>
	> 	1.  **Crea el Archivo de Política:** En tu proyecto, crea un archivo llamado `caching-policy.xml` con el contenido del bloque XML anterior.
	> 	2.  **Aplica la Política:** La operación que importamos tiene el ID `get-api-products`. Ejecuta este comando:
	>
	>  	   ```bash
	> 	    az apim api operation policy create \
	> 	      --resource-group $RESOURCE_GROUP \
	> 	      --service-name $APIM_NAME \
	> 	      --api-id "miapideproductos" \
	> 	      --operation-id "get-api-products" \
	> 	      --policy-path "./caching-policy.xml"
	> 	    

2.  **Prueba el Caché (¡La Traza es la Clave!):**
    *   Ve a la pestaña **`Prueba` (Test)** de esta misma operación.
    *   Añade la cabecera `Ocp-apim-subscription-key` con tu clave.
    *   Activa la **`Traza` (Trace)** haciendo clic en el botón correspondiente.
    *   **PRIMERA LLAMADA:** Haz clic en **`Enviar`**. Observa el tiempo de respuesta y el mensaje de `"Cache miss"` en la traza.
    *   **SEGUNDA LLAMADA:** Inmediatamente después, haz clic en **`Enviar`** de nuevo. Observa la drástica reducción del tiempo de respuesta y el mensaje de `"Cache hit"` en la traza.

3.  **Limpieza (¡Importante!):**
    *   **Método Portal:** Elimina las políticas `<cache-lookup>` y `<cache-store>` y guarda.
    
	>    *   **Método CLI:** Crea un archivo `empty-policy.xml` (si no lo tienes del Día 2) y aplícalo para resetear la política.
	>
	>	```bash
> 	# Contenido de empty-policy.xml: <policies><inbound><base /></inbound>...</policies>
	>
	>   	az apim api operation policy create \
	>           --resource-group $RESOURCE_GROUP \
	>           --service-name $APIM_NAME \
	>           --api-id "miapideproductos" \
	>           --operation-id "get-api-products" \
	>           --policy-path "./empty-policy.xml"


## **Parte 2: Transformando Peticiones sobre la Marcha**

**Escenario:** Vamos a desacoplar la API pública de la implementación interna añadiendo una cabecera de seguimiento.

1.  **Aplica las Políticas de Transformación (Método Portal):**
    *   En el mismo editor de políticas (`GET /api/products`), pega este bloque completo:

        ```xml
        <policies>
            <inbound>
                <base />
                <rewrite-uri template="/api/products" />
                <set-header name="X-Request-ID" exists-action="override">
                    <value>@(context.RequestId.ToString())</value>
                </set-header>
            </inbound>
            <backend><base /></backend>
            <outbound><base /></outbound>
            <on-error><base /></on-error>
        </policies>
        ```
    *   Haz clic en **`Save`** y espera 60 segundos.

	> #### Alternativa con Azure CLI
	>
	> 1.  **Crea el Archivo de Política:** Crea un archivo `transformation-policy.xml` con el contenido del bloque anterior.
	> 2.  **Aplica la Política:**
	>
	>     ```bash
	>     az apim api operation policy create \
	>       --resource-group $RESOURCE_GROUP \
	>       --service-name $APIM_NAME \
	>       --api-id "miapideproductos" \
	>       --operation-id "get-api-products" \
	>       --policy-path "./transformation-policy.xml"
	>     ```

2.  **Prueba la Transformación (La Traza lo Revela Todo):**
    *   Usa la pestaña `Prueba` con la `Traza` activada. En la sección `backend` de la traza, busca la entrada `forward-request` y confirma que la cabecera `X-Request-ID` está presente.

3.  **Limpieza:** Vuelve a aplicar la política vacía (`empty-policy.xml`) usando la CLI o borrando el contenido en el portal.



## **Parte 3: Manejo de Errores Personalizado**

**Escenario:** Si nuestro backend falla o una dependencia crítica no responde, queremos capturar ese error y devolver una respuesta JSON limpia y estandarizada al cliente, en lugar de un error genérico.

 **1. Implementar el Patrón de Error Robusto**

Vamos a simular una llamada a una dependencia interna que falla desde la sección `<inbound>`. Esto nos permite probar nuestra lógica `<on-error>` de una manera limpia y predecible.

*   **Método Portal:**
    *   Navega a tu instancia de APIM -> APIs -> `MiApiDeProductos` -> operación `GET /api/products`.
    *   Abre el editor de políticas (`</>`).
    *   **Reemplaza todo el contenido de tu política** con este bloque completo.

        ```xml
        <policies>
            <inbound>
                <!-- 
                    PASO 1: Simular una llamada a una dependencia crítica que fallará.
                    Usamos send-request porque nos da control total. Como 'ignore-error' es 'false'
                    (por defecto), un fallo aquí detendrá el flujo y saltará a la sección <on-error>.
                -->
                <send-request mode="new" response-variable-name="simulatedBackendCall" timeout="5" ignore-error="false">
                    <set-url>https://thisurldoesnotexist.error</set-url>
                    <set-method>GET</set-method>
                </send-request>
                
                <!-- 
                    La etiqueta <base /> es necesaria para que el editor nos deje guardar.
                    En este escenario de error, el flujo nunca llegará a ejecutarla.
                -->
                <base />
            </inbound>
            <backend>
                <!-- El flujo nunca llegará aquí en este escenario de prueba -->
                <base />
            </backend>
            <outbound>
                <base />
            </outbound>
            <on-error>
                <!-- ¡AQUÍ SÍ LLEGARÁ! Cuando <send-request> falle, la ejecución salta a esta sección. -->
                <base />
                <set-header name="Content-Type" exists-action="override">
                    <value>application/json</value>
                </set-header>
                <set-status code="500" reason="Internal Server Error" />
                <set-body>@{
                    var errorResponse = new JObject(
                        new JProperty("errorId", context.RequestId),
                        new JProperty("message", "Lo sentimos, ha ocurrido un error inesperado en una dependencia interna."),
                        new JProperty("timestamp", DateTime.UtcNow.ToString("o"))
                    );
                    return errorResponse.ToString();
                }</set-body>
            </on-error>
        </policies>
        ```
    *   Haz clic en **`Save`**. No deberías recibir ningún error ni advertencia.

> #### Alternativa con Azure CLI
>
> 1.  **Crea el Archivo de Política:** En tu proyecto, crea un archivo llamado `on-error-policy.xml` con el contenido del bloque XML anterior.
> 2.  **Aplica la Política:**
>
>     ```bash
>     az apim api operation policy create \
>       --resource-group $RESOURCE_GROUP \
>       --service-name $APIM_NAME \
>       --api-id "miapideproductos" \
>       --operation-id "get-api-products" \
>       --policy-path "./on-error-policy.xml"
>     ```

#### **2. Prueba el Resultado Final**

1.  Espera 60 segundos para asegurar que la política se ha propagado al gateway.
2.  Ve a la pestaña **`Prueba` (Test)** de la operación `GET /api/products`.
3.  Añade la cabecera `Ocp-apim-subscription-key` con tu clave.
4.  Haz clic en **`Enviar`**.

*   **Resultado Esperado:** Ahora deberías recibir una respuesta `HTTP/1.1 500 Internal Server Error` con el cuerpo de la respuesta siendo el JSON limpio y profesional que acabas de definir, demostrando que tu sección `<on-error>` se ha ejecutado correctamente.

#### **3. Limpieza FINAL (Paso Obligatorio)**

¡No olvides este paso! Tu API está actualmente configurada para fallar siempre. Debes restaurarla a un estado funcional.

*   **Método Portal:** Vuelve al editor de políticas y reemplaza el contenido con la política vacía.

    ```xml
    <policies>
        <inbound><base /></inbound>
        <backend><base /></backend>
        <outbound><base /></outbound>
        <on-error><base /></on-error>
    </policies>
    ```
*   **Método CLI:** Aplica tu archivo `empty-policy.xml`.

    > ```bash
    > az apim api operation policy create \
    >   --resource-group $RESOURCE_GROUP \
    >   --service-name $APIM_NAME \
    >   --api-id "miapideproductos" \
    >   --operation-id "get-api-products" \
    >   --policy-path "./empty-policy.xml"
    > ```


## **Parte 4 (Bonus): Composición de APIs - Uniendo dos mundos**

**Escenario:** Crearemos un endpoint virtual `GET /products/{id}/movements` que llame a dos APIs internas para devolver una respuesta combinada.

### **Paso 1: Expandir Nuestro Backend**

1.  Abre tu proyecto `MiApiDeProductos` en `Program.cs`.
2.  **Justo antes de la línea `app.Run();`**, pega los nuevos endpoints:

    ```csharp
    // --- INICIO DEL CÓDIGO A PEGAR (ENDPOINTS) ---
    app.MapGet("/api/products/{id}", (int id) => {
        var product = ProductRepository.Products.FirstOrDefault(p => p.Id == id);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }).WithName("GetProductById").WithOpenApi();

    app.MapGet("/api/inventory/product/{id}/movements", (int id) => {
        var movements = InventoryRepository.Movements.Where(m => m.ProductId == id);
        return Results.Ok(movements);
    }).WithName("GetInventoryMovements").WithOpenApi();
    // --- FIN DEL CÓDIGO A PEGAR (ENDPOINTS) ---
    ```
3.  Ahora, ve hasta el **final del archivo `Program.cs`** y pega los nuevos modelos de datos:

    ```csharp
    // --- INICIO DEL CÓDIGO A PEGAR (MODELOS Y DATOS) ---
    public record InventoryMovement(int ProductId, DateTime Timestamp, string Type, int Quantity);
    public static class InventoryRepository
    {
        public static List<InventoryMovement> Movements { get; } = new List<InventoryMovement>
        { new(1, DateTime.UtcNow.AddDays(-10), "Restock", 5), new(1, DateTime.UtcNow.AddDays(-8), "Sale", -1), new(1, DateTime.UtcNow.AddDays(-5), "Sale", -1), new(2, DateTime.UtcNow.AddDays(-15), "Restock", 20), new(2, DateTime.UtcNow.AddDays(-12), "Sale", -5), new(3, DateTime.UtcNow.AddDays(-20), "Restock", 10) };
    }
    // --- FIN DEL CÓDIGO A PEGAR (MODELOS Y DATOS) ---
    ```
4.  Guarda y reinicia tu API local (`dotnet run`).

***Recordatorio Crucial:*** *Después de modificar el archivo `Program.cs`, no olvides detener y reiniciar tu API local (`dotnet run`) para que los cambios surtan efecto.*

### **Paso 2: Definir la Nueva Operación Compuesta en APIM (Método Portal)**

1.  En tu API `MiApiDeProductos` en el portal, haz clic en **`+ Add operation`**.
2.  Configura:
    *   **URL:** `GET` `/products/{id}/movements`
    *   **Display name:** `GetProductWithMovements`
    *   **Name:** `get-product-with-movements`
3.  Ve a la pestaña **`Parameters`** y añade el parámetro `id` de tipo `number`.
4.  Guarda la operación.

### **Paso 3: La Magia de la Política de Composición**

*   **Método Portal:** Selecciona la nueva operación y abre su editor de políticas. Pega la política de abajo, **reemplazando `TU_URL_DE_NGROK`** con tu URL de Ngrok.
    *   ***¡TRIPLE-CHECK!*** *El error más común en este paso es un error de tipeo en la URL de Ngrok. Asegúrate de que las dos URLs en la política sean correctas y empiecen por `https://`.*

*   **Contenido para `composition-policy.xml` o para pegar en el portal:**

```xml
<policies>
    <inbound>
        <base />
        <!-- PASO 1: Llamar a la API de Productos -->
        <send-request mode="new" response-variable-name="productResponse" timeout="20" ignore-error="false">
            <set-url>@($"https://organic-swine-eminent.ngrok-free.app/api/products/{int.Parse(context.Request.MatchedParameters["id"])}")</set-url>
            <set-method>GET</set-method>
        </send-request>
        
        <!-- PASO 2: Llamar a la API de Inventario -->
        <send-request mode="new" response-variable-name="inventoryResponse" timeout="20" ignore-error="false">
            <set-url>@($"https://organic-swine-eminent.ngrok-free.app/api/inventory/product/{int.Parse(context.Request.MatchedParameters["id"])}/movements")</set-url>
            <set-method>GET</set-method>
        </send-request>
        
        <!-- 
            PASO 3: Construir y devolver la respuesta en un solo paso.
            Esto asegura que el cuerpo que construimos es el que se envía.
        -->
        <return-response>
            <set-status code="200" reason="OK" />
            <set-header name="Content-Type" exists-action="override">
                <value>application/json</value>
            </set-header>
            <set-body>@{
                var product = ((IResponse)context.Variables["productResponse"]).Body.As<JObject>();
                var movements = ((IResponse)context.Variables["inventoryResponse"]).Body.As<JArray>();
                var finalResponse = new JObject(new JProperty("productId", product["id"]), new JProperty("productName", product["name"]), new JProperty("currentStock", product["stock"]), new JProperty("inventoryMovements", movements));
                return finalResponse.ToString();
            }</set-body>
        </return-response>
    </inbound>
    <backend><base /></backend>
    <outbound><base /></outbound>
    <on-error><base /></on-error>
</policies>
```
    
> #### Alternativa con Azure CLI
>
> 1.  **Define una Variable de Entorno para Ngrok:**
>
>     ```bash
>     export NGROK_URL="https://TU_URL_DE_NGROK.ngrok-free.app"
>     ```
>
> 2.  **Crea el Archivo de Política:** Crea `composition-policy.xml`. Usa `$NGROK_URL` como placeholder.
>
> 3.  **Aplica la Política con Sustitución:**
>
>     ```bash
>     # El ID de la nueva operación es 'get-product-with-movements'
>     cat composition-policy.xml | envsubst | az apim api operation policy create \
>       --resource-group $RESOURCE_GROUP \
>       --service-name $APIM_NAME \
>       --api-id "miapideproductos" \
>       --operation-id "getproductwithmovements" \
>       --policy-content -
>     ```


### **Paso 4: Probar el Resultado Compuesto**

1.  Ve a la pestaña **`Prueba`** de tu nueva operación.
2.  Introduce un `id` de producto válido (ej. `1`).
3.  Añade tu clave de suscripción y envía la petición.
4.  ¡Deberías ver una única respuesta JSON con los detalles del producto y su historial de movimientos

### **Paso 5: Manejar el Caso de 'Producto no Encontrado'**

**Escenario:** Nuestra política actual funciona perfectamente si el producto existe. Pero si un cliente pide un producto con un ID inválido (ej. `99`), la primera llamada al backend devolverá un `404 Not Found`. Nuestra política actual no maneja esto y puede devolver un error 500 genérico. Vamos a mejorarla para que, si el producto no se encuentra, devolvamos un mensaje de error `404 Not Found` claro y en formato JSON.

1.  **Modifica la Política de Composición con Lógica Condicional**
    *   Navega a la operación `GetProductWithMovements` y abre su editor de políticas.
    *   **Reemplaza todo el contenido** con esta nueva versión mejorada. Lee los comentarios para entender los cambios.

        ```xml
        <policies>
            <inbound>
                <base />
                <!-- 
                    PASO 1: Llamar a la API de Productos. 
                    ¡CAMBIO CLAVE! Usamos 'ignore-error="true"' para que si la llamada falla (ej. con un 404),
                    la ejecución de la política continúe en lugar de saltar a <on-error>. Esto nos da control.
                -->
                <send-request mode="new" response-variable-name="productResponse" timeout="20" ignore-error="true">
                    <set-url>@($"https://TU_URL_DE_NGROK.ngrok-free.app/api/products/{int.Parse(context.Request.MatchedParameters["id"])}")</set-url>
                    <set-method>GET</set-method>
                </send-request>
                
                <!-- 
                    PASO 2: Lógica Condicional. Usamos <choose> para inspeccionar el resultado del PASO 1.
                -->
                <choose>
                    <!-- CASO A: Si la llamada al producto NO fue exitosa (ej. devolvió un 404 Not Found) -->
                    <when condition="@(((IResponse)context.Variables["productResponse"]).StatusCode != 200)">
                        <!-- Devolvemos una respuesta de error 404 clara y terminamos el proceso. -->
                        <return-response>
                            <set-status code="404" reason="Not Found" />
                            <set-header name="Content-Type" exists-action="override">
                                <value>application/json</value>
                            </set-header>
                            <set-body>@{
                                return new JObject(
                                    new JProperty("error", "Producto no encontrado."),
                                    new JProperty("productId", context.Request.MatchedParameters["id"])
                                ).ToString();
                            }</set-body>
                        </return-response>
                    </when>
                    
                    <!-- CASO B: Si la llamada al producto FUE exitosa (el "camino feliz") -->
                    <otherwise>
                        <!-- Continuamos con la lógica original: llamamos a la API de inventario y combinamos los resultados. -->
                        <send-request mode="new" response-variable-name="inventoryResponse" timeout="20" ignore-error="false">
                            <set-url>@($"https://TU_URL_DE_NGROK.ngrok-free.app/api/inventory/product/{int.Parse(context.Request.MatchedParameters["id"])}/movements")</set-url>
                            <set-method>GET</set-method>
                        </send-request>
                        <return-response>
                            <set-status code="200" reason="OK" />
                            <set-header name="Content-Type" exists-action="override">
                                <value>application/json</value>
                            </set-header>
                            <set-body>@{
                                var product = ((IResponse)context.Variables["productResponse"]).Body.As<JObject>();
                                var movements = ((IResponse)context.Variables["inventoryResponse"]).Body.As<JArray>();
                                var finalResponse = new JObject(new JProperty("productId", product["id"]), new JProperty("productName", product["name"]), new JProperty("currentStock", product["stock"]), new JProperty("inventoryMovements", movements));
                                return finalResponse.ToString();
                            }</set-body>
                        </return-response>
                    </otherwise>
                </choose>
            </inbound>
            <backend><base /></backend>
            <outbound><base /></outbound>
            <on-error><base /></on-error>
        </policies>
        ```
    *   **¡IMPORTANTE!** No olvides reemplazar `TU_URL_DE_NGROK` con tu URL real.
    *   Haz clic en **`Save`** y espera 60 segundos.

2.  **Prueba Ambos Escenarios**
    *   Ve a la pestaña **`Prueba` (Test)** de tu operación.
    *   Añade tu clave de suscripción.

    *   **PRUEBA 1 (Camino Feliz):**
        *   Introduce un `id` de producto válido, como **`1`**.
        *   Haz clic en **`Enviar`**.
        *   **Resultado Esperado:** Deberías recibir el mismo `200 OK` con el JSON combinado de siempre.

    *   **PRUEBA 2 (Producto no Encontrado):**
        *   Introduce un `id` de producto que **no exista**, como **`99`**.
        *   Haz clic en **`Enviar`**.
        *   **Resultado Esperado:** ¡Ahora deberías recibir una respuesta `HTTP/1.1 404 Not Found` con tu mensaje de error JSON personalizado!

            ```json
            {
                "error": "Producto no encontrado.",
                "productId": "99"
            }
            ```

## **Conclusión del Día 3: De un Proxy a una Fachada Inteligente**

¡Felicidades por completar el día más intenso de políticas! Hoy hemos elevado nuestra API de ser un simple proxy seguro a convertirse en una **fachada de API inteligente, rápida y robusta**.

**Lo que has logrado hoy:**

*   **Rendimiento Mejorado:** Has aprendido a usar el **caché** para servir datos comunes a velocidades de milisegundos, reduciendo la carga en tus backends y mejorando drásticamente la experiencia del usuario.
*   **Flexibilidad y Desacoplamiento:** Con las **políticas de transformación**, has visto cómo modificar peticiones sobre la marcha, desacoplando tu API pública de la implementación interna.
*   **Resiliencia y Profesionalismo:** Has implementado un **manejo de errores personalizado**, asegurando que tu API responda de forma elegante y predecible, incluso cuando las cosas van mal.
*   **Creación de Valor:** Con la **composición de APIs**, has creado un nuevo endpoint que es mucho más valioso para el consumidor que los microservicios individuales, demostrando el verdadero poder de un Gateway de APIs.

Tu API ya no es solo una puerta; es un conserje inteligente que optimiza, traduce, protege y enriquece cada interacción.

**Lo que nos espera mañana (Día 4):**

Hoy hemos perfeccionado la API para el sistema y el backend. Mañana, nos enfocaremos en las personas: **los desarrolladores que la consumirán y los equipos que la operarán.**

Exploraremos:
*   **El Portal del Desarrollador (La Vitrina):** Publicaremos y personalizaremos el portal para que otros puedan descubrir, aprender y suscribirse a tus APIs.
*   **Monitorización y Alertas:** Usaremos Azure Monitor y Application Insights para vigilar la salud de nuestra API y saber proactivamente si algo va mal.
*   **Principios de DevOps:** Daremos un primer vistazo a cómo automatizar la gestión y el despliegue de estas configuraciones.