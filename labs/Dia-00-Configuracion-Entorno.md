# Día 00: Configuración y Verificación del Entorno de Desarrollo

¡Bienvenido/a al curso **"Domina Azure API Management"**!

Este documento es una guía de verificación para asegurar que tu entorno de desarrollo está listo para el curso. Nuestro entorno de trabajo estándar es **Ubuntu sobre WSL2**, utilizando **Bash** como shell.

Aunque es probable que ya tengas todo instalado, por favor, sigue estos pasos para verificar cada componente. Esto garantizará que todos partamos desde la misma base y podamos centrarnos en el contenido del curso.


## Checklist de Prerrequisitos

### 1. Entorno de Trabajo (WSL2 + Ubuntu)

Es el sistema operativo base donde ejecutaremos todos nuestros comandos y nuestra API.

*   **Verificación:** Abre tu terminal de Ubuntu y comprueba la versión del sistema operativo.

    ```bash
    cat /etc/os-release
    ```
    *Deberías ver una salida que confirme que estás usando una versión de Ubuntu (ej. 22.04 LTS).*

### 2. SDK de .NET 8

Es el framework que usaremos para compilar y ejecutar nuestra API de ejemplo.

*   **Verificación:** Ejecuta el siguiente comando para confirmar que tienes .NET 8 instalado.
    
    ```bash
    dotnet --version
    ```
    *La salida debe ser `8.0.x` o superior.*

*   **En caso de no tenerlo:** [Instrucciones de instalación de .NET para Ubuntu](https://learn.microsoft.com/es-es/dotnet/core/install/linux-ubuntu)

### 3. Azure CLI

La Interfaz de Línea de Comandos de Azure es nuestra herramienta principal para interactuar con los recursos en la nube, incluyendo Azure API Management.

*   **Verificación:** Comprueba la versión instalada.
    
    ```bash
    az --version
    ```
    *Esto mostrará la versión de la CLI, que debería ser reciente.*

*   **Acción Requerida:** Asegúrate de haber iniciado sesión en tu cuenta de Azure.
    
    ```bash
    az login
    ```
    *Este comando abrirá un navegador para que te autentiques. Una vez completado, cierra la pestaña y vuelve a la terminal.*

### 4. Git

Es la herramienta que usamos para el control de versiones y para clonar el repositorio del curso.

*   **Verificación:** Comprueba que Git está instalado.
    
    ```bash
    git --version
    ```


## Editor de Código: Visual Studio Code

Usaremos Visual Studio Code como nuestro editor principal debido a su excelente integración con WSL y su potente ecosistema de extensiones.

### Extensiones Esenciales

Asegúrate de tener instaladas las siguientes extensiones en VS Code. Son clave para el flujo de trabajo del curso.

*   [**WSL (de Microsoft)**](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-wsl): **¡CRÍTICA!** Permite que VS Code en Windows se conecte y trabaje directamente sobre tu sistema de archivos de Ubuntu. Cuando abres el proyecto, VS Code debe mostrar "WSL: Ubuntu" en la esquina inferior izquierda.

*   [**C# (de Microsoft)**](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp): Proporciona resaltado de sintaxis, IntelliSense, y capacidades de depuración para nuestro proyecto de ASP.NET Core.

*   [**REST Client (de Huachao Mao)**](https://marketplace.visualstudio.com/items?itemName=humao.rest-client): Nos permitirá ejecutar las pruebas de API directamente desde archivos `.http` que están versionados en el repositorio, manteniendo todo dentro del mismo editor.


## Verificación Final

Si has podido ejecutar todos los comandos de verificación con éxito y tienes las extensiones de VS Code instaladas, **¡estás listo para empezar!**

El siguiente paso es clonar el repositorio del curso si aún no lo has hecho:

```bash
git clone https://github.com/msouga/curso-apim-dotnet.git
cd curso-apim-dotnet
```

¡Nos vemos en el Día 1 para empezar a publicar nuestra primera API!