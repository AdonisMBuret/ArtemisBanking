using ArtemisBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioPrestamo : IRepositorioGenerico<Prestamo>
    {
        // Obtener préstamo por número con todas sus relaciones
        Task<Prestamo> ObtenerPorNumeroPrestamoAsync(string numeroPrestamo);

        // Obtener préstamo activo de un usuario
        Task<Prestamo> ObtenerPrestamoActivoDeUsuarioAsync(string usuarioId);

        // Obtener todos los préstamos de un usuario
        Task<IEnumerable<Prestamo>> ObtenerPrestamosDeUsuarioAsync(string usuarioId);

        // Obtener préstamos paginados con filtros
        Task<(IEnumerable<Prestamo> prestamos, int total)> ObtenerPrestamosPaginadosAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActivo = null);

        // Calcular deuda promedio de todos los clientes
        Task<decimal> CalcularDeudaPromedioAsync();

        // Calcular deuda total de un cliente (préstamos + tarjetas)
        Task<decimal> CalcularDeudaTotalClienteAsync(string usuarioId);

        // Generar número de préstamo único
        Task<string> GenerarNumeroPrestamoUnicoAsync();

        // Verificar si un cliente tiene préstamo activo
        Task<bool> TienePrestamoActivoAsync(string usuarioId);

        // Obtener cantidad de préstamos activos
        Task<int> ContarPrestamosActivosAsync();
    }
}
