using Microsoft.EntityFrameworkCore;
using PizzeriaAPI.Models;

namespace PizzeriaAPI.Data
{
    public class PizzeriaContext : DbContext
    {
        public PizzeriaContext(DbContextOptions<PizzeriaContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Pizza> Pizzas { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Gasto> Gastos { get; set; }

        public DbSet<CategoriaGasto> CategoriasGasto { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }
        public DbSet<PagoEmpleado> PagoEmpleados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Snake_case automático para todas las tablas y columnas
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(ToSnakeCase(entity.GetTableName()));

                foreach (var property in entity.GetProperties())
                    property.SetColumnName(ToSnakeCase(property.GetColumnName()));

                foreach (var key in entity.GetForeignKeys())
                    key.SetConstraintName(ToSnakeCase(key.GetConstraintName()));
            }

            // Precisión de decimales
            modelBuilder.Entity<Pizza>()
                .Property(p => p.PrecioEntera).HasPrecision(10, 2);
            modelBuilder.Entity<Pizza>()
                .Property(p => p.PrecioMitad).HasPrecision(10, 2);
            modelBuilder.Entity<Pedido>()
                .Property(p => p.CostoDelivery).HasPrecision(10, 2);
            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total).HasPrecision(10, 2);
            modelBuilder.Entity<DetallePedido>()
                .Property(d => d.PrecioUnitario).HasPrecision(10, 2);
            modelBuilder.Entity<DetallePedido>()
                .Property(d => d.Subtotal).HasPrecision(10, 2);
            modelBuilder.Entity<Empleado>()
                .Property(e => e.SueldoSemanal).HasPrecision(10, 2);
            modelBuilder.Entity<Gasto>()
                .Property(g => g.Monto).HasPrecision(10, 2);
            modelBuilder.Entity<PagoEmpleado>()
                .Property(p => p.Monto).HasPrecision(10, 2);
            modelBuilder.Entity<Configuracion>()
                .Property(c => c.CostoDelivery).HasPrecision(10, 2);

            // FK de DetallePedido hacia Pizzas
            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.PizzaMitad1)
                .WithMany(p => p.DetallesComoMitad1)
                .HasForeignKey(dp => dp.PizzaMitad1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.PizzaMitad2)
                .WithMany(p => p.DetallesComoMitad2)
                .HasForeignKey(dp => dp.PizzaMitad2Id)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return string.Concat(name.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? "_" + c.ToString() : c.ToString())).ToLower();
        }
    }
}