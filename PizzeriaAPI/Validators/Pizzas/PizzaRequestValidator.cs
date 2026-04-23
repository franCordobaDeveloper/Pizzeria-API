using FluentValidation;
using PizzeriaAPI.DTOs.Pizzas;

namespace PizzeriaAPI.Validators.Pizzas
{
        public class PizzaRequestValidator : AbstractValidator<PizzaRequestDto>
        {
            public PizzaRequestValidator()
            {
                RuleFor(x => x.Nombre)
                    .NotEmpty().WithMessage("El nombre de la pizza es obligatorio.")
                    .MaximumLength(100).WithMessage("El nombre de la pizza no puede exceder los 100 caracteres.");

                RuleFor(x => x.PrecioEntera)
                    .GreaterThan(0).WithMessage("El precio de la pizza entera debe ser mayor que cero.");

                RuleFor(x => x.PrecioMitad)
                    .GreaterThan(0).WithMessage("El precio de la pizza mitad debe ser mayor que cero.")
                    .LessThanOrEqualTo(x => x.PrecioEntera).WithMessage("El precio de la pizza mitad no puede ser mayor que el precio de la pizza entera.");

            }
        }
}
