using ArtemisBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Interfaces
{
    public interface IRepositorioCuentaAhorro : IRepositorioGenerico<CuentaAhorro>
    {
        // Obtener cuenta por número de cuenta
        Task<CuentaAhorro> ObtenerPorNumeroCuentaAsync(string numeroCuenta);

        // Obtener cuenta principal de un usuario
        Task<CuentaAhorro> ObtenerCuentaPrincipalAsync(string usuarioId);

        // Obtener todas las cuentas de un usuario (incluyendo relaciones)
        Task<IEnumerable<CuentaAhorro>> ObtenerCuentasDeUsuarioAsync(string usuarioId);

        // Obtener cuentas activas de un usuario
        Task<IEnumerable<CuentaAhorro>> ObtenerCuentasActivasDeUsuarioAsync(string usuarioId);

        // Obtener cuentas paginadas con filtros
        Task<(IEnumerable<CuentaAhorro> cuentas, int total)> ObtenerCuentasPaginadasAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActiva = null,
            bool? esPrincipal = null);

        // Generar número de cuenta único
        Task<string> GenerarNumeroCuentaUnicoAsync();

        // Verificar si un número de cuenta ya existe
        Task<bool> ExisteNumeroCuentaAsync(string numeroCuenta);
    }
}
