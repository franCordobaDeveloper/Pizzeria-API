using FluentValidation;
using PizzeriaAPI.DTOs.Pedidos;

namespace PizzeriaAPI.Validators.Pedidos
{
    public class DetallePedidoRequestValidator : AbstractValidator<DetallePedidoRequestDto>
    {
        public DetallePedidoRequestValidator()
        {
            RuleFor(x => x.PizzaMitad1Id)
                .GreaterThan(0).WithMessage("La pizza mitad 1 es requerida");

            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("El tipo es requerido")
                .Must(t => t == "entera" || t == "combo" || t == "media")
                .WithMessage("El tipo debe ser 'entera', 'combo' o 'media'");

            // Validación según el tipo
            When(x => x.Tipo == "entera", () =>
            {
                RuleFor(x => x.PizzaMitad2Id)
                    .NotNull().WithMessage("Para pizza entera se requiere la segunda mitad")
                    .GreaterThan(0).WithMessage("La segunda mitad es requerida");

                RuleFor(x => x)
                    .Must(x => x.PizzaMitad1Id == x.PizzaMitad2Id)
                    .WithMessage("Para pizza entera, ambas mitades deben ser la misma pizza");
            });

            When(x => x.Tipo == "combo", () =>
            {
                RuleFor(x => x.PizzaMitad2Id)
                    .NotNull().WithMessage("Para pizza combo se requiere la segunda mitad")
                    .GreaterThan(0).WithMessage("La segunda mitad es requerida");

                RuleFor(x => x)
                    .Must(x => x.PizzaMitad1Id != x.PizzaMitad2Id)
                    .WithMessage("Para pizza combo, las mitades deben ser diferentes");
            });

            When(x => x.Tipo == "media", () =>
            {
                RuleFor(x => x.PizzaMitad2Id)
                    .Null().WithMessage("Para media pizza, la segunda mitad debe ser nula");
            });

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero");
        }
    }
}