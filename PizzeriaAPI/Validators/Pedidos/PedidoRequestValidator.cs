using FluentValidation;
using PizzeriaAPI.DTOs.Pedidos;

namespace PizzeriaAPI.Validators.Pedidos
{
    public class PedidoRequestValidator : AbstractValidator<PedidoRequestDto>
    {
        public PedidoRequestValidator()
        {
            RuleFor(x => x.ClienteId)
                .GreaterThan(0).WithMessage("El cliente es requerido");

            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("El tipo de pedido es requerido")
                .Must(t => t == "delivery" || t == "retiro")
                .WithMessage("El tipo debe ser 'delivery' o 'retiro'");

            RuleFor(x => x.MetodoPago)
                .NotEmpty().WithMessage("El método de pago es requerido")
                .Must(m => m == "efectivo" || m == "transferencia")
                .WithMessage("El método de pago debe ser 'efectivo' o 'transferencia'");

            RuleFor(x => x.Pizzas)
                .NotEmpty().WithMessage("El pedido debe tener al menos una pizza");

            RuleForEach(x => x.Pizzas)
                .SetValidator(new DetallePedidoRequestValidator());
        }
    }
}