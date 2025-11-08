# ArtemisBanking

Practica de Programación para un mini proyecto de aula

# NOTA IMPORTANTE SOBRE LA ESTRUCTURA DEL PROYECTO:

Este proyecto sigue la arquitectura Onion (también conocida como Clean Architecture).
Las capas están organizadas de la siguiente manera:

1.  Domain (núcleo) - No depende de nadie

    - Entidades
    - Enums
    - Constantes

2.  Application - Solo depende de Domain
    - Interfaces de repositorios
    - Interfaces de servicios
    - DTOs
3.  Infrastructure - Depende de Application y Domain

    - Implementación de repositorios
    - Implementación de servicios
    - DbContext y configuraciones de EF
    - Identity
    - Jobs de Hangfire

4.  Web (Presentation) - Depende de todas las anteriores
    - Controllers
    - ViewModels
    - Views

Esta arquitectura permite:

- Separación clara de responsabilidades
- Fácil testing (podemos mockear las interfaces)
- Cambiar implementaciones sin afectar otras capas
- Mantenimiento más sencillo del código

INSTRUCCIONES PARA CONFIGURAR EL CORREO ELECTRÓNICO:
  
 * 1. Ve a tu cuenta de Gmail
 * 2. Activa la verificación en dos pasos
 * 3. Ve a "Contraseñas de aplicaciones" en tu cuenta de Google
 * 4. Genera una nueva contraseña de aplicación para "Correo"
 * 5. Copia esa contraseña (de 16 caracteres) y úsala en SmtpPassword
 * 6. Reemplaza "tu-correo@gmail.com" con tu correo real

