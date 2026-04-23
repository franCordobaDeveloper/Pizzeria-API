using System.ComponentModel.DataAnnotations.Schema;

namespace PizzeriaAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }

        [Column("password_hash")] 
        public string PasswordHash { get; set; }

        public string Rol { get; set; }
        public bool Activo { get; set; }

        public ICollection<Pedido> Pedidos { get; set; }
        public ICollection<Gasto> Gastos { get; set; }
    }
}