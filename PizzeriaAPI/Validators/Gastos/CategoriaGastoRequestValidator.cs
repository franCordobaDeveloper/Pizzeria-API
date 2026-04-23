using FluentValidation;
using PizzeriaAPI.DTOs.Gastos;

namespace PizzeriaAPI.Validators.Gastos
{
    public class CategoriaGastoRequestValidator : AbstractValidator<CategoriaGastoRequestDto>
    {
        public CategoriaGastoRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre de la categoría es requerido")
                .MaximumLength(100).WithMessage("El nombre de la categoría no puede tener más de 100 caracteres");

        }
    }
}
