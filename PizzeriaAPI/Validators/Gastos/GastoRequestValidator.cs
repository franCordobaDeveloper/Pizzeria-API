using FluentValidation;
using PizzeriaAPI.DTOs.Gastos;
namespace PizzeriaAPI.Validators.Gastos
{
    public class GastoRequestValidator : AbstractValidator<GastoRequestDto>
    {
        public GastoRequestValidator()
        {
            RuleFor(x => x.CategoriaId)
                .GreaterThan(0).WithMessage("La categoría es requerida");
            RuleFor(x => x.Descripcion)
                .MaximumLength(200).WithMessage("La descripción no puede tener más de 200 caracteres");
            RuleFor(x => x.Monto)
                .GreaterThan(0).WithMessage("El monto debe ser un valor positivo");
            RuleFor(x => x.Fecha)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now)).WithMessage("La fecha no puede ser futura");
        }
    }
}
