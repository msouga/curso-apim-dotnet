# Día 04: Laboratorio - Ecosistema del Desarrollador y Operaciones

¡Bienvenido al Día 4! Hoy cambiamos nuestro enfoque desde el interior de la API hacia las personas que la construyen y la consumen. Crearemos una experiencia de primer nivel para los desarrolladores que usarán nuestras APIs y estableceremos las bases para una operativa robusta y automatizada.

## Objetivos del Día

Al finalizar este laboratorio, serás capaz de:

*   Publicar y gestionar el Portal del Desarrollador de Azure API Management.
*   Simular el ciclo de vida completo de un desarrollador consumidor: registro, suscripción y prueba.
*   Monitorizar la salud y el rendimiento de tus APIs usando Azure Monitor y Application Insights.
*   Configurar reglas de alerta proactivas para ser notificado de problemas operativos.
*   Integrar tu instancia de APIM con un repositorio Git para habilitar la gestión de la configuración como código (GitOps).

## Prerrequisitos

*   Haber completado satisfactoriamente los laboratorios de los Días 1, 2 y 3.
*   Una instancia de Azure API Management funcionando con la API y el Producto de los días anteriores.
*   Acceso a un cliente de Git (como VS Code con la extensión de Git o Git Bash).
*   Una cuenta de GitHub o Azure DevOps con un repositorio Git vacío y listo para ser usado.
*   Permisos suficientes en tu suscripción de Azure para crear reglas de alerta y grupos de acciones.


## **Parte 1: La Vitrina - El Portal del Desarrollador**

**Escenario:** Un nuevo desarrollador se ha unido a un equipo de partners y necesita acceder a nuestra "API de Productos". Facilitaremos un proceso de auto-servicio completo, asegurándonos de que nuestra API sea descubrible, accesible y fácilmente comprobable desde la consola interactiva del portal.

### **Paso 1: Verificación y Preparación del Backend**

Antes de interactuar con el portal, es fundamental asegurar que nuestra API en APIM puede comunicarse correctamente con el servicio de backend que se ejecuta en nuestra máquina local.

1.  Asegúrate de que tu API de .NET se está ejecutando en tu máquina.
2.  Verifica que tienes un túnel de Ngrok activo apuntando al puerto de tu API local.
3.  Ve a la configuración de tu API en APIM (sección "APIs" -> "MiApiDeProductos" -> pestaña "Configuración") y confirma que la **URL del servicio web** coincide con tu URL de Ngrok actual.
4.  Realiza una prueba rápida desde la pestaña "Probar" de APIM en el Portal de Azure para verificar que recibes una respuesta `200 OK`.

### **Paso 2: Publicar el Producto**

Para que una API aparezca en el catálogo del portal del desarrollador, el "Producto" que la contiene debe estar explícitamente publicado.

1.  En el Portal de Azure, navega a tu instancia de API Management.
2.  En el menú de la izquierda, bajo la sección "APIs", haz clic en **Productos**.
3.  Busca tu **"Producto Estándar"** y haz clic en su nombre.
4.  En la página de detalles del producto, en la barra de herramientas superior, haz clic en el botón **Publicar**. Confirma la acción cuando se te solicite.

### **Paso 3: Configurar Políticas de Integración del Portal**

Para garantizar una experiencia de prueba fluida en la consola interactiva del portal, añadiremos de forma proactiva dos políticas clave a nuestra API.

1.  Navega a la sección **APIs** y selecciona tu API **MiApiDeProductos**.
2.  En la pestaña **Diseño**, en la sección **Procesamiento de entrada** (Inbound processing), haz clic en el icono `</>` para editar las políticas.
3.  Dentro de la sección `<inbound>`, justo después de la etiqueta `<base />`, pega el siguiente bloque de código XML. Estas políticas habilitan la comunicación segura desde el navegador y aseguran la conexión directa con nuestro backend.

    ```xml
    <!-- Habilita la comunicación entre el portal y la puerta de enlace de APIM (CORS) -->
    <cors allow-credentials="true">
        <allowed-origins>
            <origin>https://apim-curso-msouga-29804.developer.azure-api.net</origin>
        </allowed-origins>
        <allowed-methods>
            <method>*</method>
        </allowed-methods>
        <allowed-headers>
            <header>*</header>
        </allowed-headers>
        <expose-headers>
            <header>*</header>
        </expose-headers>
    </cors>

    <!-- Asegura la comunicación directa con el backend de Ngrok -->
    <set-header name="ngrok-skip-browser-warning" exists-action="override">
        <value>true</value>
    </set-header>
    ```
    *   **Nota del Instructor:** La política `<cors>` es necesaria porque el portal y la API viven en dominios diferentes. La política `<set-header>` asegura la comunicación directa con los servicios expuestos a través de Ngrok.
4.  Haz clic en **Guardar**.

### **Paso 4: Publicar el Portal del Desarrollador**

Con nuestra API y nuestras políticas listas, es hora de poner en línea el portal.

1.  En el menú de la izquierda, ve a "Portal para desarrolladores" y haz clic en **Información general del portal**.
2.  **Aprovisionamiento:** Si es la primera vez que visitas esta sección, verás una advertencia de que el portal "no se ha aprovisionado". Para activarlo, haz clic en el enlace **Portal para desarrolladores** en la barra de herramientas superior. Espera a que la nueva pestaña cargue por completo, luego vuelve a esta pestaña del Portal de Azure y refresca la página (F5).
3.  **Publicación:** El botón **Publicar** ahora estará activo. Haz clic en él para lanzar el portal.

### **Paso 5: Simular el Ciclo de Vida del Desarrollador**

Ahora, actuaremos como un desarrollador externo que utiliza nuestro portal por primera vez.

1.  En la sección **Información general del portal**, haz clic en **Portal para desarrolladores** para abrirlo.
2.  **Registro:** En la esquina superior derecha, haz clic en **Sign up**. Completa el formulario para crear una nueva cuenta de desarrollador.
3.  **Suscripción:** Una vez dentro, haz clic en **Productos**. Selecciona el "Producto Estándar", dale un nombre a tu suscripción (ej. "Mi Suscripción de Prueba") y haz clic en **Suscribirse**.
4.  **Obtener Clave:** Ve a tu **Perfil** (haciendo clic en tu nombre). En "Suscripciones", haz clic en el icono del ojo para mostrar y **copiar la Clave principal**.

### **Paso 6: Probar la API Exitosamente desde el Portal**

Este es el momento de la verdad, donde comprobamos que toda nuestra configuración funciona en conjunto.

1.  En el portal, ve a **APIs** y selecciona tu **API de Productos**.
2.  Busca la operación `GET /api/products` y haz clic en **Try it**.
3.  En la consola de pruebas, pega la clave de suscripción que copiaste en el campo de la cabecera `Ocp-Apim-Subscription-Key`.
4.  Haz clic en **Send**.

¡Felicidades! Deberías ver una respuesta **`200 OK`** con el cuerpo JSON de tus productos. Has configurado y probado con éxito el ciclo de vida completo de un desarrollador, estableciendo una experiencia de primera clase.

## **Parte 2: El Vigilante - Monitorización y Alertas**

**Escenario:** Queremos garantizar la fiabilidad de nuestra API. Para ello, exploraremos sus métricas de rendimiento y configuraremos una alerta que nos notifique por correo electrónico si detectamos un número anómalo de intentos de acceso no autorizados.

###**Paso 1: Explorar Métricas Clave**

1.  Vuelve al Portal de Azure y a tu instancia de APIM.
2.  En el menú de la izquierda, en la sección "Supervisión", haz clic en **Métricas**.
3.  Se abrirá el explorador de métricas de Azure Monitor. Vamos a visualizar algunas métricas importantes:
    *   **Métrica:** Selecciona `Requests`. Esto te mostrará el número total de peticiones a lo largo del tiempo.
    *   Haz clic en **Agregar métrica** y ahora selecciona `Backend Duration`.
    *   Agrega una tercera métrica: `Gateway Duration`.
4.  Analiza el gráfico. `Gateway Duration` es el tiempo total que APIM tarda en procesar la petición (incluyendo políticas), mientras que `Backend Duration` es el tiempo que tu API de backend tardó en responder. La diferencia entre ambas es la sobrecarga introducida por APIM.

###**Paso 2: Trazabilidad con Application Insights**

1.  En el menú de la izquierda, en la sección "Supervisión", haz clic en **Application Insights**.
2.  Haz clic en el nombre del recurso de Application Insights asociado.
3.  En el menú de Application Insights, haz clic en **Búsqueda de transacciones**.
4.  Aquí verás una lista de las peticiones recientes. Haz clic en una de las peticiones `GET /api/products` que realizaste desde el portal.
5.  Se abrirá la "Traza de un extremo a otro". Esta vista es increíblemente poderosa. Te muestra la petición entrando en APIM, el tiempo que pasó en la puerta de enlace, la llamada saliente al backend y la respuesta. Es fundamental para depurar dónde se producen los cuellos de botella o los errores.

###**Paso 3: Crear una Regla de Alerta**

Vamos a crear una regla que nos avise si hay más de 5 peticiones con error 401 (No autorizado) en un periodo de 5 minutos.

1.  En la hoja de tu instancia de APIM en el Portal de Azure, ve a **Alertas** (en la sección "Supervisión").
2.  Haz clic en **Crear regla de alerta**.
3.  **Condición:**
    *   Haz clic en **Agregar condición**.
    *   En "Tipo de señal", elige **Métricas**.
    *   Busca y selecciona la señal `Requests`.
    *   En la sección "Lógica de alerta", configura lo siguiente:
        *   **Tipo de umbral:** Estático.
        *   **Operador:** Mayor que.
        *   **Tipo de agregación:** Total.
        *   **Valor del umbral:** 5.
    *   Expande la sección "Dividir por dimensiones":
        *   En "Nombre de la dimensión", selecciona `Gateway Response Code`.
        *   En "Operador", deja ` = `.
        *   En "Valores de dimensión", selecciona `401`.
    *   Haz clic en **Listo**.
4.  **Acciones:**
    *   Haz clic en **Agregar grupo de acciones** y luego en **Crear grupo de acciones**.
    *   **Básico:** Elige tu suscripción y grupo de recursos, y dale un nombre como `ag-apim-admins`.
    *   **Notificaciones:**
        *   Tipo de notificación: `Correo electrónico/SMS/Push/Voz`.
        *   Nombre: `email-admin`.
        *   En la ventana emergente, marca la casilla "Correo electrónico" e introduce tu propia dirección de correo electrónico.
        *   Haz clic en **Aceptar**.
    *   Haz clic en **Revisar + crear** y luego en **Crear**.
5.  **Detalles:**
    *   Dale un nombre a la regla de alerta, como `Alerta de Peticiones no Autorizadas`.
    *   Haz clic en **Revisar + crear** y finalmente en **Crear**.

¡Listo! Si ahora intentas hacer más de 5 llamadas a la API con una clave incorrecta en menos de 5 minutos, recibirás un correo electrónico de alerta.


## **Parte 3: La Fábrica - Introducción a GitOps/DevOps**

**Escenario:** Hemos estado configurando todo manualmente en el portal. Esto es propenso a errores y no es escalable. Daremos el primer paso hacia la "Configuración como Código" guardando la configuración de nuestra instancia de APIM en un repositorio Git.

###**Paso 1: Conectar APIM a un Repositorio Git**

1.  En tu instancia de APIM en el Portal de Azure, busca la sección "Implementación e infraestructura" en el menú de la izquierda y haz clic en **Repositorio**.
2.  Se te presentarán las opciones para guardar la configuración. Haz clic en **Guardar en el repositorio**.
3.  Se abrirá una nueva hoja para configurar la conexión:
    *   **Tipo de repositorio:** Elige `Git`.
    *   **URL del repositorio:** Pega la URL de tu repositorio Git vacío (ej. `https://github.com/tu-usuario/mi-repo-apim.git`).
    *   **Rama:** `main` o `master`, dependiendo de la rama por defecto de tu repositorio.
    *   **Token de acceso:** Necesitarás generar un "Token de Acceso Personal" (PAT) desde GitHub (en Settings > Developer settings) o Azure DevOps. Este token necesita permisos de `repo`. Pégalo aquí.
    *   Marca la casilla para confirmar el uso de un repositorio no administrado.
4.  Haz clic en **Guardar**. APIM extraerá toda su configuración (APIs, políticas, productos, etc.) como archivos y la enviará a tu repositorio Git.

> **Alternativa con Azure CLI**
> Aunque este paso inicial es más sencillo a través del portal para establecer la conexión, la gestión futura se puede automatizar. Las operaciones de backup y restore con la CLI son la base para los pipelines de CI/CD.
> 
> ```bash
> # Ejemplo de cómo se haría un backup de la configuración
> az apim backup --name "MiInstanciaAPIM" \
>   -g "MiGrupoRecursos" \
>   --backup-name "apim-backup-$(date +%Y%m%d)" \
>   --storage-account-name "mistorageaccount" \
>   --storage-account-container "apim-backups" \
>   --storage-account-key "TU_STORAGE_KEY"
> ```

###**Paso 2: Clonar y Explorar la Configuración como Código**

1.  En tu máquina local, abre una terminal o un cliente de Git.
2.  Clona el repositorio que acabas de vincular:

    ```bash
    git clone https://github.com/tu-usuario/mi-repo-apim.git
    ```
3.  Abre la carpeta clonada en VS Code.
4.  Explora la estructura de carpetas. Verás directorios como `apis`, `products`, `policies`, etc. Toda tu configuración de APIM está ahora representada en archivos de texto (principalmente `json` y `xml`).
5.  Navega a `api-management/apis/tu-api-de-productos/` y abre el archivo `policy.xml`. Reconocerás la estructura de políticas que hemos trabajado en los días anteriores.

###**Paso 3: Realizar un Cambio y Sincronizar**

1.  En el archivo `policy.xml` dentro de VS Code, añade un comentario XML inofensivo dentro de la sección `<inbound>`, como:
    
    ```xml
    <!-- Este cambio fue hecho desde VS Code -->
    ```
2.  Guarda el archivo.
3.  Usa Git para confirmar y empujar el cambio al repositorio remoto:
    
    ```bash
    git add .
    git commit -m "feat: Añadir comentario a la política de productos"
    git push
    ```

###**Paso 4: Entendiendo el Flujo de CI/CD (Conceptual)**

Acabas de realizar un cambio "localmente" (en código) y lo has enviado al repositorio central. Sin embargo, este cambio no se refleja automáticamente en la instancia de APIM.

El siguiente paso, que está fuera del alcance de este laboratorio pero es crucial en un entorno real, sería configurar un pipeline de CI/CD (usando GitHub Actions o Azure Pipelines). Este pipeline se activaría con cada `push` a la rama `main`, y ejecutaría un script (usando Azure CLI o PowerShell) que tomaría la configuración del repositorio y la aplicaría a la instancia de APIM, completando el ciclo de GitOps.

## Conclusión del Laboratorio

¡Felicidades! Hoy has cerrado el círculo de la gestión de APIs. No solo tienes una API segura, inteligente y de alto rendimiento, sino que ahora también has construido:

*   Una **puerta de entrada para desarrolladores** a través de un portal de auto-servicio.
*   Un **sistema de vigilancia** con monitorización y alertas proactivas.
*   Los **cimientos de una fábrica de software** gestionando tu configuración como código.

**Comandos de Git para Guardar tu Progreso:**

```bash
# Asegúrate de estar en la raíz de tu repositorio clonado
git add .
git commit -m "feat: Finalización del laboratorio del Día 4"
git push
```

