using FluentValidation;
using PizzeriaAPI.DTOs.Clientes;

namespace PizzeriaAPI.Validators.Clientes
{
    public class ClienteRequestValidator: AbstractValidator<ClienteRequestDto>
    {
        public ClienteRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre del cliente no puede exceder los 100 caracteres.");

            RuleFor(x => x.Telefono)
                 .MaximumLength(20).WithMessage("El teléfono no puede superar 20 caracteres")
                 .When(x => !string.IsNullOrEmpty(x.Telefono));

            RuleFor(x => x.Direccion)
                  .MaximumLength(255).WithMessage("La dirección no puede superar 255 caracteres")
                  .When(x => !string.IsNullOrEmpty(x.Direccion));

            RuleFor(x => x.Notas)
                  .MaximumLength(500).WithMessage("Las notas no pueden superar 500 caracteres")
                  .When(x => !string.IsNullOrEmpty(x.Notas));

        }
    }
}
