# Día 00: Configuración y Verificación del Entorno de Desarrollo

¡Bienvenido/a al curso **"Domina Azure API Management"**!

Este documento es una guía de verificación para asegurar que tu entorno de desarrollo está listo para el curso. Nuestro entorno de trabajo estándar es **Ubuntu sobre WSL2**, utilizando **Bash** como shell.

Aunque es probable que ya tengas todo instalado, por favor, sigue estos pasos para verificar cada componente. Esto garantizará que todos partamos desde la misma base y podamos centrarnos en el contenido del curso.


## Checklist de Prerrequisitos

### 1. Entorno de Trabajo (WSL2 + Ubuntu)

*   **Verificación:** Abre tu terminal de Ubuntu y comprueba la versión del sistema operativo.
    
    ```bash
    cat /etc/os-release
    ```

### 2. SDK de .NET 8

*   **Verificación:** Confirma que tienes .NET 8 instalado.
    
    ```bash
    dotnet --version
    ```
*   **En caso de no tenerlo:** [Instrucciones de instalación de .NET para Ubuntu](https://learn.microsoft.com/es-es/dotnet/core/install/linux-ubuntu)

### 3. Azure CLI

*   **Verificación:** Comprueba la versión instalada.
    
    ```bash
    az --version
    ```
*   **Acción Requerida:** Asegúrate de haber iniciado sesión en tu cuenta de Azure.
    
    ```bash
    az login
    ```

### 4. Git

*   **Verificación:** Comprueba que Git está instalado.
    
    ```bash
    git --version
    ```

### 5. Ngrok: Túnel a Internet

Para que Azure API Management (en la nube) pueda comunicarse con nuestra API (en nuestra máquina local), necesitamos un túnel seguro. Usaremos `ngrok`.

**A. Instalación en Ubuntu/WSL:**

```bash
# Añade el repositorio de ngrok y su clave de seguridad
curl -s https://ngrok-agent.s3.amazonaws.com/ngrok.asc | \
  sudo tee /etc/apt/trusted.gpg.d/ngrok.asc >/dev/null && \
  echo "deb https://ngrok-agent.s3.amazonaws.com buster main" | \
  sudo tee /etc/apt/sources.list.d/ngrok.list

# Actualiza la lista de paquetes e instala ngrok
sudo apt update && sudo apt install ngrok
```

**B. Creación de Cuenta y Autenticación:**

1.  **Crea una cuenta:** Ve a [https://dashboard.ngrok.com/signup](https://dashboard.ngrok.com/signup) (puedes usar tu cuenta de GitHub o Google).
2.  **Copia tu Authtoken:** En la sección "Getting Started" -> "Your Authtoken", copia el comando de autenticación.
3.  **Ejecuta el Comando:** Pega el comando en tu terminal de Ubuntu.

    ```bash
    ngrok config add-authtoken TU_TOKEN_PERSONAL_AQUI
    ```

**C. Crear y Obtener tu Dominio Estático Fijo:**
El plan gratuito de `ngrok` te asigna un dominio estático y permanente. Sigue estos pasos para encontrarlo:

1.  **Navega:** En el dashboard de `ngrok`, ve a la sección del menú izquierdo **Universal Gateway** -> **Domains**.
2.  **Inicia la Creación:** Verás una página explicando la funcionalidad. Haz clic en el botón azul **`+ Create Domain`**.
3.  **Encuentra tu Dominio:** Aparecerá una ventana emergente (pop-up). Puede que muestre un mensaje pidiendo que actualices tu plan. **Puedes ignorar ese mensaje.** Dentro de esa misma ventana, `ngrok` te mostrará el dominio estático que ha sido asignado a tu cuenta. Será un nombre como `organic-swine-eminent.ngrok-free.app`.
4.  **Anótalo:** **Copia este dominio completo y guárdalo.** Lo necesitarás en el laboratorio del Día 1. Ya puedes cerrar la ventana emergente.

**D. Verificación:**
Confirma que la instalación fue exitosa.

```bash
ngrok --version
```


## Editor de Código: Visual Studio Code

Usaremos Visual Studio Code como nuestro editor principal.

### Extensiones Esenciales
Asegúrate de tener instaladas las siguientes extensiones:

*   [**WSL (de Microsoft)**](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-wsl)
*   [**C# (de Microsoft)**](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
*   [**REST Client (de Huachao Mao)**](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

---

## Verificación Final

Si has podido ejecutar todos los comandos de verificación con éxito, **¡estás listo para empezar!**

El siguiente paso es clonar el repositorio del curso si aún no lo has hecho:

```bash
git clone https://github.com/msouga/curso-apim-dotnet.git
cd curso-apim-dotnet
```

¡Nos vemos en el Día 1 para empezar a publicar nuestra primera API!