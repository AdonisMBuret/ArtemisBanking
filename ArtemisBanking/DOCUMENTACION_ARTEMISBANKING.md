# Documentación del Proyecto ArtemisBanking
**Estudiantes:**  
- Diomar Arianny Fleming Díaz (2024-1872)  
- Adonis Mercedes Buret (2021-2396)

---

## 1. Descripción General

**ArtemisBanking** es un sistema bancario integral desarrollado con **ASP.NET Core (.NET 9)** utilizando el patrón **Razor Pages** y arquitectura en capas. 
El sistema permite la gestión completa de operaciones bancarias, incluyendo cuentas de ahorro, préstamos, tarjetas de crédito, transacciones, y gestión de comercios afiliados.

### Objetivo del Proyecto
Proporcionar una plataforma bancaria segura, escalable y fácil de usar que permita:
- Gestión de usuarios con múltiples roles (Administrador, Cajero, Cliente, Comercio)
- Administración de productos financieros (Cuentas, Préstamos, Tarjetas)
- Procesamiento de transacciones bancarias en tiempo real
- Sistema de comercios afiliados para pagos con tarjeta
- Dashboards personalizados por tipo de usuario

---

### 2 Capas del Sistema

#### **Capa de Dominio (Domain)**
- **Responsabilidad:** Entidades del negocio y contratos (interfaces)
- **Contenido:**
  - Entidades: `Usuario`, `CuentaAhorro`, `Prestamo`, `TarjetaCredito`, `Comercio`, `Transaccion`
  - Interfaces de repositorios
  - Enumeraciones: `TipoUsuario`, `TipoTransaccion`, `EstadoTransaccion`

#### **Capa de Infraestructura (Infrastructure)**
- **Responsabilidad:** Acceso a datos y persistencia
- **Contenido:**
  - `ArtemisBankingDbContext` (Entity Framework Core)
  - Implementación de repositorios
  - Configuraciones de entidades (Fluent API)
  - Migraciones de base de datos
  - `DbSeeder` para datos iniciales

#### **Capa de Aplicación (Application)**
- **Responsabilidad:** Lógica de negocio y orquestación
- **Contenido:**
  - Servicios de negocio (`ServicioUsuario`, `ServicioPrestamo`, `ServicioTarjeta`, etc.)
  - DTOs (Data Transfer Objects)
  - ViewModels para vistas
  - Mappers (AutoMapper)
  - Validaciones de negocio

#### **Capa de Presentación (Web)**
- **Responsabilidad:** Interfaz de usuario y controladores
- **Contenido:**
  - Controllers MVC
  - Razor Views
  - Validaciones del lado del cliente
  - Estilos CSS personalizados (Bootstrap + tema morado)

#### **Capa de API (ArtemisBanking.Api)** ⭐
- **Responsabilidad:** Endpoints REST para integración externa
- **Contenido:**
  - **Controllers API:**
    - `AccountController` - Autenticación y gestión de cuentas
    - `CommerceController` - Gestión de comercios (solo Admin)
    - `PayController` - Procesamiento de pagos (Admin y Comercio)
  - **Autenticación JWT:**
    - Tokens Bearer para autenticación stateless
    - Claims personalizados (UserId, Role, ComercioId)
    - Validación de issuer, audience y firma
  - **Documentación Swagger/OpenAPI:**
    - UI interactiva en la raíz del API
    - Definiciones de schemas
    - Autenticación Bearer en Swagger
  - **Políticas de Autorización:**
    - `SoloAdministrador`
    - `SoloComercio`
    - `AdminOComercio`
  - **CORS:** Configurado para permitir consumo desde aplicaciones externas

---

## 3. Base de Datos

### 3.1 Motor y Configuración
- **Motor:** SQL Server
- **ORM:** Entity Framework Core 9.0
- **Estrategia:** Code-First con migraciones

### 3.2 Diagrama de Entidades Principales

```
┌─────────────────┐         ┌──────────────────┐
│     Usuario     │◄───────►│    Comercio      │
│  (IdentityUser) │         │                  │
├─────────────────┤         ├──────────────────┤
│ Id (PK)         │         │ Id (PK)          │
│ Nombre          │         │ Nombre           │
│ Apellido        │         │ RNC              │
│ Cedula          │         │ UsuarioId (FK)   │
│ Email           │         │ EstaActivo       │
│ EstaActivo      │         └──────────────────┘
│ ComercioId (FK) │
└─────────────────┘
         │
         ├─────────┬─────────┬──────────┐
         │         │         │          │
         ▼         ▼         ▼          ▼
┌─────────────┐ ┌────────┐ ┌────────┐ ┌────────────┐
│CuentaAhorro │ │Prestamo│ │Tarjeta │ │Beneficiario│
├─────────────┤ ├────────┤ ├────────┤ ├────────────┤
│Id (PK)      │ │Id (PK) │ │Id (PK) │ │Id (PK)     │
│NumeroCuenta │ │Numero  │ │Numero  │ │Nombre      │
│Balance      │ │Monto   │ │Limite  │ │Cuenta      │
│EsPrincipal  │ │Tasa    │ │Deuda   │ │Banco       │
│UsuarioId(FK)│ │Plazo   │ │CVC     │ │UsuarioId   │
└─────────────┘ └────────┘ └────────┘ └────────────┘
      │              │           │
      │              │           │
      ▼              ▼           ▼
┌──────────────┐ ┌──────────┐ ┌──────────────┐
│ Transaccion  │ │CuotaPres.│ │ConsumoTarjeta│
├──────────────┤ ├──────────┤ ├──────────────┤
│Id (PK)       │ │Id (PK)   │ │Id (PK)       │
│Monto         │ │FechaPago │ │Monto         │
│Tipo          │ │Monto     │ │ComercioId    │
│Estado        │ │EstaPagada│ │TarjetaId     │
│Beneficiario  │ │PrestamoId│ │FechaConsumo  │
│CuentaId (FK) │ └──────────┘ └──────────────┘
└──────────────┘
```

### 3.3 Tablas Principales

| Tabla | Descripción | Campos Clave |
|-------|-------------|--------------|
| **AspNetUsers** | Usuarios del sistema (Identity) | Id, Nombre, Apellido, Cedula, Email, ComercioId |
| **CuentasAhorro** | Cuentas bancarias | NumeroCuenta (9 dígitos), Balance, EsPrincipal |
| **Prestamos** | Préstamos otorgados | NumeroPrestamo, MontoCapital, TasaInteres, PlazoMeses |
| **TarjetasCredito** | Tarjetas de crédito | NumeroTarjeta (16 dígitos), LimiteCredito, DeudaActual |
| **Comercios** | Comercios afiliados | Nombre, RNC, UsuarioId |
| **Transacciones** | Movimientos bancarios | Monto, TipoTransaccion, EstadoTransaccion |
| **CuotasPrestamo** | Tabla de amortización | FechaPago, MontoCuota, EstaPagada |
| **ConsumosTarjeta** | Compras con TC | Monto, ComercioId, TarjetaId |

---

## 4. Roles y Permisos

### 4.1 Tipos de Usuario

El sistema implementa **4 roles principales**:

#### 🔵 **Administrador**
- **Funciones:**
  - Gestión completa de usuarios (CRUD)
  - Asignación de préstamos
  - Asignación de tarjetas de crédito
  - Creación de cuentas secundarias
  - Gestión de comercios
  - Visualización de dashboards administrativos
  - Modificación de tasas de interés
- **Política de Acceso:** `SoloAdministrador`

#### 🟢 **Cajero**
- **Funciones:**
  - Realizar depósitos en cuentas
  - Procesar retiros
  - Registrar pagos de préstamos
  - Registrar pagos de tarjetas de crédito
  - Consultar información de clientes
  - Dashboard con estadísticas del día
- **Política de Acceso:** `SoloCajero`

#### 🟡 **Cliente**
- **Funciones:**
  - Consultar cuentas de ahorro
  - Ver historial de transacciones
  - Transferir entre cuentas propias
  - Transferir a beneficiarios
  - Gestionar beneficiarios
  - Ver préstamos activos
  - Ver tarjetas de crédito
  - Realizar pagos de servicios
- **Política de Acceso:** `SoloCliente`

#### 🟣 **Comercio**
- **Funciones:**
  - Procesar pagos con tarjeta de crédito
  - Consultar consumos realizados
  - Ver historial de transacciones
  - Gestionar perfil del comercio
- **Política de Acceso:** `SoloComercio`

---

## 5. Funcionalidades Principales

### 5.1 Gestión de Usuarios

#### Creación de Usuarios
```csharp
// Flujo de creación
1. Validar datos (nombre, correo, cédula únicos)
2. Crear usuario con UserManager
3. Asignar rol correspondiente
4. Si es Cliente → crear cuenta principal
5. Si es Comercio → asociar a comercio existente
6. Generar token de confirmación
7. Enviar correo de activación
```

#### Roles disponibles:
- Administrador
- Cajero
- Cliente
- Comercio

### 5.2 Cuentas de Ahorro

#### Características:
- **Cuenta Principal:** Automática al registrarse como cliente
- **Cuentas Secundarias:** Pueden crearse múltiples
- **Número de Cuenta:** 9 dígitos únicos generados aleatoriamente
- **Balance:** Precisión decimal (18,2)

#### Operaciones:
- ✅ Crear cuenta secundaria
- ✅ Transferir entre cuentas propias
- ✅ Cancelar cuenta secundaria (transfiere fondos a principal)
- ✅ Consultar movimientos

### 5.3 Préstamos

#### Sistema de Amortización Francés
```csharp
Cuota Mensual = P * [r(1+r)^n] / [(1+r)^n - 1]

Donde:
- P = Monto del préstamo (capital)
- r = Tasa de interés mensual (anual/12)
- n = Número de cuotas (meses)
```

#### Características:
- **Plazos disponibles:** 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 meses
- **Tasa de interés:** Configurable por préstamo
- **Validación de riesgo:** Compara deuda vs promedio del sistema
- **Tabla de amortización:** Generada automáticamente
- **Seguimiento:** Cuotas pagadas/pendientes, estado de mora

#### Proceso de asignación:
1. Seleccionar cliente sin préstamo activo
2. Configurar monto, plazo y tasa
3. Validar riesgo crediticio
4. Generar tabla de amortización
5. Acreditar fondos a cuenta principal
6. Enviar notificación por correo

### 5.4 Tarjetas de Crédito

#### Características:
- **Número:** 16 dígitos únicos
- **CVC:** Cifrado con SHA-256
- **Fecha de expiración:** Formato MM/AA (5 años de vigencia)
- **Límite de crédito:** Configurable
- **Crédito disponible:** Calculado en tiempo real

#### Operaciones:
- ✅ Asignación de tarjeta
- ✅ Realizar consumos en comercios
- ✅ Pago de deuda (total o parcial)
- ✅ Modificación de límite
- ✅ Cancelación de tarjeta

### 5.5 Transacciones

#### Tipos de Transacción:
- **DÉBITO:** Salida de fondos (retiros, transferencias salientes, pagos)
- **CRÉDITO:** Entrada de fondos (depósitos, transferencias entrantes, acreditaciones)

#### Estados:
- **APROBADA:** Transacción exitosa
- **RECHAZADA:** Transacción fallida

#### Registro Completo:
- Fecha y hora
- Monto
- Tipo (DÉBITO/CRÉDITO)
- Origen
- Beneficiario
- Estado
- Cuenta asociada

### 5.6 Comercios

#### Gestión de Comercios:
1. **Registro:** Nombre, RNC único
2. **Asignación de Usuario:** Un usuario con rol "Comercio" por negocio
3. **Cuenta de Ahorro:** Se crea automáticamente con balance $0
4. **Procesamiento de Pagos:** Consumos con tarjetas de crédito

#### Relación Bidireccional:
```csharp
Usuario.ComercioId → Comercio.Id
Comercio.UsuarioId → Usuario.Id
```

---

## 6. Seguridad

### 6.1 Autenticación
- **ASP.NET Core Identity** para gestión de usuarios
- **Cookies de autenticación** con tiempo de expiración configurable
- **Confirmación de correo electrónico** obligatoria
- **Tokens de reseteo de contraseña** con expiración

### 6.2 Cifrado
- **Contraseñas:** Hasheadas con Identity (PBKDF2)
- **CVC de tarjetas:** SHA-256
- **Tokens:** Generados criptográficamente

### 6.3 Validaciones
- **Lado del servidor:** Data Annotations + FluentValidation
- **Lado del cliente:** jQuery Validation
- **Unicidad:** Email, Username, Cédula, Número de Cuenta/Tarjeta
- **Formato:** Validación de cédula (11 dígitos), emails, montos

---

## 7. API REST (ArtemisBanking.Api)

### 7.1 Descripción General
La **API REST** de ArtemisBanking proporciona endpoints HTTP para que aplicaciones externas (como apps móviles, sistemas de comercios o integraciones de terceros) puedan consumir los servicios del sistema bancario de forma segura y estandarizada.

### 7.2 Características Principales

#### ✅ **Autenticación JWT (JSON Web Token)**
- Autenticación **stateless** (sin sesiones en servidor)
- Tokens firmados con **HS256** (HMAC SHA-256)
- Claims personalizados: `UserId`, `Role`, `ComercioId`
- Tiempo de expiración configurable
- Refresh tokens (opcional para futuras implementaciones)

#### ✅ **Documentación Swagger/OpenAPI**
- **UI interactiva** disponible en la raíz del API
- Permite **probar endpoints** directamente desde el navegador
- Autenticación Bearer integrada en Swagger
- Schemas automáticos de request/response

#### ✅ **CORS (Cross-Origin Resource Sharing)**
- Configurado para permitir consumo desde dominios externos
- Política `AllowAll` en desarrollo (personalizable en producción)

---

### 8 Swagger/OpenAPI

**Características de Swagger UI:**
- ✅ Listado completo de endpoints
- ✅ Modelos de datos (schemas)
- ✅ Probar requests directamente
- ✅ Autenticación Bearer integrada
- ✅ Códigos de respuesta HTTP
- ✅ Ejemplos de request/response

#### Configurar Autenticación en Swagger
1. Hacer clic en el botón **"Authorize"**
2. Ingresar: `Bearer <tu-token-jwt>`
3. Hacer clic en **"Authorize"**
4. Probar endpoints protegidos


### 8.1 Manejo de Errores

La API utiliza códigos de estado HTTP estándar:

| Código | Significado | Cuándo se usa |
|--------|-------------|---------------|
| **200 OK** | Solicitud exitosa | GET con datos |
| **201 Created** | Recurso creado | POST exitoso |
| **204 No Content** | Operación exitosa sin retorno | PUT, PATCH, DELETE |
| **400 Bad Request** | Datos inválidos | Validación fallida |
| **401 Unauthorized** | No autenticado | Token ausente/inválido |
| **403 Forbidden** | No autorizado | Sin permisos suficientes |
| **404 Not Found** | Recurso no existe | GET con ID inexistente |
| **500 Internal Server Error** | Error del servidor | Excepción no controlada |


### 8.2 Seguridad de la API

#### ✅ Protección contra ataques comunes
- **SQL Injection:** Entity Framework con queries parametrizadas
- **XSS:** DTOs tipados, no se retorna HTML
- **CSRF:** No aplica (autenticación stateless con JWT)
- **Replay Attacks:** Tokens con expiración
- **Brute Force:** Rate limiting (recomendado implementar)

#### ✅ Validación de datos
- **Model State:** Validación automática con DataAnnotations
- **FluentValidation:** Validaciones complejas en servicios
- **Sanitización:** Inputs validados antes de persistir

#### ✅ HTTPS obligatorio
```csharp
app.UseHttpsRedirection();
```

### 8.3 Testing de la API

#### Herramientas Recomendadas
1. **Swagger UI** - Incluido en el proyecto
2. **Postman** - Cliente HTTP popular
3. **cURL** - Línea de comandos
4. **Thunder Client** (VS Code) - Extensión ligera

### 8.4 Diferencias API vs Web

| Característica | Web (MVC/Razor) | API (REST) |
|----------------|-----------------|------------|
| **Autenticación** | Cookies | JWT Bearer |
| **Sesión** | Stateful (servidor) | Stateless |
| **Respuesta** | HTML (Views) | JSON |
| **CSRF Protection** | Requerido | No requerido |
| **Uso principal** | Navegadores | Apps móviles, integraciones |
| **Documentación** | Manual | Swagger automático |

---

## 9. Tecnologías y Librerías

### 9.1 Framework Principal
- **.NET 9.0**
- **ASP.NET Core MVC**
- **Entity Framework Core 9.0**

### 9.2 Frontend
- **Bootstrap 5.3**
- **jQuery 3.7**
- **Font Awesome 6.0**
- **SweetAlert2** (alertas personalizadas)
- **DataTables** (tablas dinámicas)

### 9.3 Herramientas de Desarrollo
- **Visual Studio 2022**
- **SQL Server Management Studio**
- **Postman** (para API testing)
- **Git/GitHub** (control de versiones)

---

## 10. Dashboards

### 10.1 Dashboard Administrador
**Métricas mostradas:**
- Total de transacciones
- Transacciones del día
- Total de pagos
- Pagos del día
- Clientes activos/inactivos
- Préstamos vigentes
- Tarjetas activas
- Cuentas de ahorro
- Deuda promedio por cliente
- Total de productos financieros

### 10.2 Dashboard Cajero
**Métricas mostradas:**
- Transacciones realizadas hoy
- Pagos procesados hoy
- Depósitos del día
- Retiros del día

### 10.3 Dashboard Cliente
**Información mostrada:**
- Balance total (suma de todas las cuentas)
- Cuentas de ahorro
- Préstamos activos
- Tarjetas de crédito
- Últimas transacciones
- Beneficiarios registrados

### 10.4 Dashboard Comercio
**Información mostrada:**
- Total de consumos procesados
- Monto total acumulado
- Balance de cuenta
- Últimos consumos
- Estadísticas mensuales

---

## 11. Validaciones de Negocio

### 11.1 Préstamos
✅ Cliente solo puede tener **1 préstamo activo** a la vez  
✅ Validación de **riesgo crediticio** (deuda vs promedio)  
✅ Plazos permitidos: **6 a 60 meses** (múltiplos de 6)  
✅ Tasa de interés: **> 0% y < 100%**  
✅ Monto: **> $0**

### 11.2 Tarjetas de Crédito
✅ Cliente puede tener **múltiples tarjetas**  
✅ Número único de **16 dígitos**  
✅ Consumo no puede exceder **crédito disponible**  
✅ Pago no puede exceder **deuda actual**  
✅ Límite de crédito **> $0**

### 11.3 Cuentas de Ahorro
✅ **1 cuenta principal** (no se puede cancelar)  
✅ **Múltiples cuentas secundarias** permitidas  
✅ Cancelación de secundaria **transfiere fondos a principal**  
✅ Número único de **9 dígitos**  
✅ Balance **≥ $0**

### 11.4 Transacciones
✅ Retiros/transferencias requieren **fondos suficientes**  
✅ Transferencias entre cuentas propias **mismo usuario**  
✅ Transferencias a beneficiarios **validar cuenta destino**  
✅ Monto **> $0**  
✅ Cuentas deben estar **activas**

---

## 12. Sistema de Notificaciones

### 12.1 Correos Electrónicos
El sistema envía notificaciones automáticas por email:

| Evento | Destinatario | Contenido |
|--------|--------------|-----------|
| Registro de usuario | Cliente | Token de confirmación + link |
| Préstamo aprobado | Cliente | Monto, plazo, tasa, cuota mensual |
| Cambio de tasa de préstamo | Cliente | Nueva tasa, nueva cuota |
| Tarjeta asignada | Cliente | Últimos 4 dígitos, límite, fecha exp. |
| Consumo en comercio | Cliente | Monto, comercio, crédito disponible |
| Pago de préstamo | Cliente | Cuota pagada, saldo pendiente |
| Pago de tarjeta | Cliente | Monto pagado, nueva deuda |

### 12.2 Plantillas de Email
- Diseño HTML responsivo
- Branding del banco 
- Información clara y concisa
- Botones de acción (call-to-action)

---

## 13. Inicialización de Datos (DbSeeder)

### 13.1 Datos de Prueba Creados

**Roles:**
```csharp
- Administrador
- Cajero
- Cliente
- Comercio
```

**Usuarios de Prueba:**
| Usuario | Contraseña | Rol | Email |
|---------|------------|-----|-------|
| admin | Admin123@ | Administrador | admin@artemisbanking.com |
| cajero | Cajero123@ | Cajero | cajero@artemisbanking.com |
| cliente | Cliente123@ | Cliente | cliente@artemisbanking.com |
| comerciante |comerciante123@ | Comercio | comercio@artemisbanking.com |


## 14. Configuración (Opcional)

### 14.1 Requisitos Previos
- ✅ Visual Studio 2022 o superior
- ✅ .NET 9 SDK
- ✅ SQL Server 2019+
- ✅ Git (opcional)


### 14.3 Configuración de Email
Editar `appsettings.json`:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@artemisbanking.com",
    "SenderName": "ArtemisBanking",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

## 15. Conclusión

**ArtemisBanking** es un sistema bancario completo y robusto que demuestra la implementación correcta de:

✅ **Arquitectura en Capas** con separación clara de responsabilidades  
✅ **Doble Capa de Presentación:** Web (MVC/Razor) y API REST  
✅ **Autenticación Dual:** Cookies para Web + JWT para API  
✅ **Patrones de Diseño** estándar de la industria  
✅ **Buenas Prácticas** de desarrollo de software  
✅ **Seguridad** implementada en múltiples niveles  
✅ **API REST** con Swagger/OpenAPI para integraciones  
✅ **Escalabilidad** para crecimiento futuro  
✅ **Mantenibilidad** con código limpio y bien documentado


