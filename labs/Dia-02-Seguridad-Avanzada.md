### Azure Api Manager
## **Día 02: Laboratorio - Aseguramiento Avanzado de APIs**

**Objetivos del Día:**

Al finalizar este laboratorio, serás capaz de:

1.  Proteger tus APIs contra el abuso mediante la limitación de peticiones (Rate Limiting).
2.  Restringir el acceso a tus APIs basándose en la dirección IP del cliente.
3.  Implementar un esquema de seguridad moderno y robusto utilizando la validación de Tokens JWT con Azure Active Directory.

**Prerrequisitos:**

*   Haber completado exitosamente el laboratorio del Día 1.
*   Tu instancia de APIM y el túnel de `ngrok` están activos y configurados.
*   Tus variables de entorno del Día 1 (`$RESOURCE_GROUP`, `$APIM_NAME`, etc.) están definidas en tu terminal.
*   Tener a mano tu clave de suscripción del "Producto Estándar".

### **Parte 1: Política de Limitación de Tasa (Rate Limiting)**

**Escenario:** Vamos a evitar que un cliente sature nuestra API, configurando un límite de 5 llamadas por minuto.

1.  **Aplica la Política (Método Portal):**
    *   En el Portal de Azure, ve a tu instancia de APIM -> **APIs** -> **"MiApiDeProductos"**.
    *   Selecciona la pestaña **"Design"** y haz clic en **"All operations"**.
    *   En la sección **"Inbound processing"**, haz clic en el icono `</>` para abrir el editor.
    *   Dentro de la sección `<inbound>`, pega la siguiente política:

        ```xml
        <rate-limit-by-key calls="5" renewal-period="60" counter-key="@(context.Subscription.Id)" />
        ```
    *   Haz clic en **"Save"**.

> #### Alternativa con Azure CLI
>
> 1.  **Crea el Archivo de Política:** En la raíz de tu proyecto, crea un archivo llamado `rate-limit-policy.xml` con el siguiente contenido:
>
>     ```xml
>     <policies>
>       <in>
>         <base />
>         <rate-limit-by-key calls="5" renewal-period="60" counter-key="@(context.Subscription.Id)" />
>       </in>
>       <backend>
>         <base />
>       </backend>
>       <out>
>         <base />
>       </out>
>       <on-error>
>         <base />
>       </on-error>
>     </policies>
>     ```
> 2.  **Aplica la Política:** La API que importamos en el Día 1 tiene por defecto el ID `miapideproductos`. Ejecuta el siguiente comando:
>
>     ```bash
>     az apim api policy create \
>       --resource-group $RESOURCE_GROUP \
>       --service-name $APIM_NAME \
>       --api-id "miapideproductos" \
>       --policy-path "./rate-limit-policy.xml"
>     ```

2.  **Prueba la Política:**
    *   En VS Code, abre tu archivo `requests/module-1.http`.
    *   Envía la `PRUEBA 2` (la llamada con clave de suscripción) **6 veces seguidas**.

3.  **Verifica el Resultado:**
    *   Las primeras 5 peticiones devolverán `200 OK`.
    *   La sexta petición fallará con `429 Too Many Requests`.

4.  **Limpieza (¡Importante!):**
    *   **Método Portal:** Vuelve al editor de políticas y elimina la política `<rate-limit-by-key.../>`. Guarda los cambios.
    
>    *   **Método CLI:**
>        1.  Crea un archivo `empty-policy.xml` con una política vacía para resetearla.
>
>             ```xml
>             <policies>
>               <in><base /></in>
>               <backend><base /></backend>
>               <out><base /></out>
>               <on-error><base /></on-error>
>             </policies>
>             ```        2.  Aplica la política vacía:
>
>             ```bash
>             az apim api policy create \
>               --resource-group $RESOURCE_GROUP \
>               --service-name $APIM_NAME \
>               --api-id "miapideproductos" \
>               --policy-path "./empty-policy.xml"
>             ```


### **Parte 2: Política de Restricción por IP (IP Filtering)**

**Escenario:** Solo permitiremos el acceso a la API desde nuestra dirección IP actual.

1.  **Encuentra tu IP Pública:**
    *   En una terminal, ejecuta:
        
        ```bash
        curl ifconfig.me
        ```
    *   Anota la IP resultante.

2.  **Aplica la Política (Método Portal):**
    *   Regresa al editor de políticas para **"All operations"**.
    *   Dentro de `<inbound>`, pega la política, **reemplazando `TU_IP_PUBLICA`**:

        ```xml
        <ip-filter action="allow">
            <address>TU_IP_PUBLICA</address>
        </ip-filter>
        ```
    *   Guarda los cambios.

> #### Alternativa con Azure CLI
>
> 1.  **Crea el Archivo de Política:** Crea un archivo llamado `ip-filter-policy.xml`. **Pega tu IP pública** donde corresponde.
>
>     ```xml
>     <policies>
>       <in>
>         <base />
>         <ip-filter action="allow">
>           <address>TU_IP_PUBLICA</address>
>         </ip-filter>
>       </in>
>       <backend><base /></backend>
>       <out><base /></out>
>       <on-error><base /></on-error>
>     </policies>
>     ```
> 2.  **Aplica la Política:**
>
>     ```bash
>     az apim api policy create \
>       --resource-group $RESOURCE_GROUP \
>       --service-name $APIM_NAME \
>       --api-id "miapideproductos" \
>       --policy-path "./ip-filter-policy.xml"
>     ```

3.  **Prueba la Política:**
    *   **Prueba de Éxito:** Envía la petición desde VS Code. Debería funcionar (`200 OK`).
    *   **Prueba de Fallo:** Modifica la política (vía Portal o CLI) con una IP falsa (ej. `1.2.3.4`) y vuelve a enviar la petición. Debería fallar con `403 Forbidden`.

4.  **Limpieza:**
    *   **Método Portal:** Elimina la política `<ip-filter.../>` y guarda.
    *   **Método CLI:** Re-aplica la política vacía usando el archivo `empty-policy.xml`.
        
        ```bash
        az apim api policy create \
          --resource-group $RESOURCE_GROUP \
          --service-name $APIM_NAME \
          --api-id "miapideproductos" \
          --policy-path "./empty-policy.xml"
        ```

### **Parte 3: Seguridad con Validación de Tokens JWT (El Método a Prueba de Fallos)**

**Escenario:** Implementaremos un control de acceso profesional. Solo las aplicaciones autenticadas con Azure AD podrán acceder a nuestra API. Seguiremos una ruta que previene los errores más comunes.

#### **Paso A: Registrar una Aplicación en Azure AD**

> **¡NOTA CRÍTICA PARA ENTORNOS COMPARTIDOS!**
>
> Como varios alumnos están trabajando en el mismo Directorio Activo (`dcp.pe`), es **vital** que cada uno use un nombre único para su aplicación para evitar confusiones.
>
> 1.  En el Portal de Azure, ve a **Azure Active Directory** -> **"App registrations"** -> **"+ New registration"**.
> 2.  **Nombre:** Usa el formato `Cliente API del Curso - [Tus Iniciales]`. Por ejemplo: `Cliente API del Curso - MS`.
> 3.  Deja las demás opciones por defecto y haz clic en **"Register"**.
> 4.  Una vez creada, copia y guarda en un lugar seguro el **"Id. de aplicación (cliente)"** y el **"Id. de directorio (inquilino)"**. Los necesitarás.

#### **Paso B: Exponer una API y Definir un Permiso (Scope)**

1.  Dentro del registro de tu aplicación, en el menú de la izquierda, ve a **`Exponer una API`**.
2.  Junto a "URI de id. de aplicación", haz clic en el enlace azul **`Agregar`**.
3.  Acepta el URI por defecto que te propone el portal y haz clic en **`Guardar`**. Copia y guarda este URI.
4.  Ahora, haz clic en **`+ Agregar un ámbito`** y define tu permiso:
    *   **Nombre del ámbito:** `Productos.Leer`
    *   **Quién puede dar el consentimiento?:** Administradores y usuarios
    *   **Nombre para mostrar del consentimiento del administrador:** `Leer la lista de productos`
    *   **Descripción del consentimiento del administrador:** `Permite a la aplicación leer la lista de productos.`
    *   Haz clic en **`Agregar ámbito`**.

#### **Paso C: Aplicar la Política 'Anti-Errores' en APIM**

Aplicaremos una política `validate-jwt` robusta que se anticipa al problema de las versiones de los tokens (v1 vs v2) que descubrimos.

1.  Ve a tu instancia de APIM -> APIs -> `MiApiDeProductos`.
2.  En la vista de **Diseño (Design)**, selecciona la operación específica **`GET /api/products`**.
3.  En la sección **"Inbound processing"**, haz clic en el icono `</>` para abrir el editor.
4.  **Borra todo el contenido** y pega este bloque exacto. Este bloque ya incluye la solución al error `Issuer validation failed`.

    ```xml
    <policies>
        <inbound>
            <base />
            <!-- ESTA POLÍTICA ESTÁ CORREGIDA PARA ACEPTAR EL TOKEN DE AZURE CLI -->
            <validate-jwt header-name="Authorization" failed-validation-httpcode="401">
                <openid-config url="https://login.microsoftonline.com/TU_TENANT_ID/v2.0/.well-known/openid-configuration"/>
                <audiences>
                    <audience>TU_APP_ID_URI</audience>
                </audiences>
                <!-- La solución al error de emisor v1/v2 -->
                <issuers>
                    <issuer>https://sts.windows.net/TU_TENANT_ID/</issuer>
                </issuers>
                <required-claims>
                    <claim name="scp" match="any" separator=" ">
                        <value>Productos.Leer</value>
                    </claim>
                </required-claims>
            </validate-jwt>
        </inbound>
        <backend>
            <base />
        </backend>
        <outbound>
            <base />
        </outbound>
        <on-error>
            <base />
        </on-error>
    </policies>
    ```

5.  **Reemplaza los placeholders** con los valores que guardaste:
    *   `TU_TENANT_ID`: Tu "Id. de directorio (inquilino)".
    *   `TU_APP_ID_URI`: Tu "URI de id. de aplicación".
6.  Haz clic en **`Save`** y **ESPERA 60 SEGUNDOS** para que el cambio se propague.

#### **Paso D: Preparar VS Code**

Descubrimos que la extensión REST Client puede tener problemas al manejar tokens JWT desde el archivo `settings.json`. Usaremos el método más fiable: definir la variable del token directamente en el archivo `.http`.

1.  Crea un nuevo archivo `requests/module-2.http`.
2.  Pega el siguiente contenido. Nota que la variable `@jwt_token` se define al principio del archivo.

    ```http
    ### PRUEBA FINAL CON VARIABLE LOCAL DE ARCHIVO

    # 1. Obtén un token FRESCO ejecutando el comando en el Paso E.
    # 2. Pega el token aquí abajo, DESPUÉS del signo '=', SIN COMILLAS.
    @jwt_token = eyJ0eXAiOiJKV1Qi...

    ###

    GET https://{{APIM_NAME}}.azure-api.net/productos/api/products
    Authorization: Bearer {{jwt_token}}
    Ocp-Apim-Subscription-Key: {{subscriptionKey}}
    ```
3.  Asegúrate de que tu archivo `.vscode/settings.json` sigue teniendo las variables `APIM_NAME` y `subscriptionKey`, y que el entorno `dev` está seleccionado en VS Code.

#### **Paso E: Obtener el Token JWT (La Secuencia Completa)**

Sigue estos pasos en tu terminal para obtener un token válido y evitar todos los errores de consentimiento y caché.

1.  **Limpieza del Caché:** Para evitar usar tokens antiguos, ejecuta:

    ```bash
    az account clear
    ```
2.  **Inicio de Sesión:**
    
    ```bash
    az login
    ```
3.  **Obtención del Token:** Usa el `APP_ID_URI` que guardaste.
    
    ```bash
    az account get-access-token --resource TU_APP_ID_URI --query accessToken --output tsv
    ```

#### **Paso F: ¡La Prueba Final!**

1.  Copia el **token nuevo y fresco** que te dio la terminal en el paso anterior.
2.  Pégalo en tu archivo `module-2-corrected.http`, en la línea `@jwt_token = ...`.
3.  Haz clic en **`Send Request`** sobre la petición `GET`.

**¡Ahora recibirás una respuesta `HTTP/1.1 200 OK` con la lista de tus productos!**


### **Resumen del Día**

¡Felicidades! Hoy has blindado tu API utilizando políticas avanzadas, tanto desde el portal como de forma automatizada con la CLI. Has aprendido a controlar el tráfico, restringir el acceso y, lo más importante, a implementar un estándar de seguridad moderno como es la autenticación basada en tokens JWT con Azure AD.