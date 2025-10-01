# 🍝 Delizioso - Sistema de Restaurante

Delizioso es un sistema de gestión para un restaurante, desarrollado en **.NET C#** con integración a **SQL Server**.  
Este proyecto está en desarrollo y busca ofrecer un sistema completo para administrar menús, pedidos y clientes.

---

## 🚀 Requisitos previos

Antes de comenzar asegúrate de tener instalado:

- [SQL Server](https://www.microsoft.com/en-us/sql-server)
- [Visual Studio 2022](https://visualstudio.microsoft.com/)

---

## 📥 Instalación

1. Clonar o descargar este repositorio:
   ```bash
   git clone https://github.com/Yan-Git18/Restaurant.git
   
   cd Restaurant

2. Abrir el proyecto en Visual Studio 2022
3. Configurar la cadena de conexión en appsettings.json:
   
   ```bash
   "ConnectionStrings": {
    "CadenaSql": "Data Source= TU_SERVIDOR_SQL; Initial Catalog= NOMBRE_BD; Integrated Security= True; Trusted_Connection= True; TrustServerCertificate= True;"
    }

4. Abrir la Consola del Administrador de Paquetes en Visual Studio y ejecutar:

   ```bash
   Update-Database
   
5. Ejecutar el Proyecto
---

## 📂 Estructura del proyecto

- **Controllers/** → Controladores del proyecto MVC, encargados de manejar la lógica de las solicitudes HTTP.
- **Data/** → Contexto de la base de datos (DbContext) y configuración para Entity Framework.
- **Migrations/** → Migraciones generadas automáticamente por Entity Framework.
- **Models/** → Clases de los modelos de dominio que representan las tablas de la base de datos.
- **ViewModels/** → Clases diseñadas para enviar datos específicos desde los controladores hacia las vistas (no necesariamente reflejan la estructura de la BD).
- **Views/** → Vistas en formato Razor (.cshtml) para la parte visual del sistema.
- **wwwroot/** → Carpeta pública que contiene archivos estáticos como:
  - **css/** → Hojas de estilo personalizadas.
  - **images/** → Imágenes del proyecto.
  - **js/** → Archivos JavaScript.
  - **lib/** → Librerías externas (ej. Bootstrap, jQuery, etc.).
- **appsettings.json** → Archivo de configuración principal, contiene la cadena de conexión a la base de datos.
- **Program.cs** → Punto de entrada del proyecto, donde se configuran los servicios e inicia la aplicación.
