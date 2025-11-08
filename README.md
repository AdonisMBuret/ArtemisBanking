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
