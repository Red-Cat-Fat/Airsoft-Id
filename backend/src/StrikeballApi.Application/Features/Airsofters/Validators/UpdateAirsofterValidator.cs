using FluentValidation;
using StrikeballApi.Application.Features.Airsofters.Commands;

namespace StrikeballApi.Application.Features.Airsofters.Validators;

public class UpdateAirsofterValidator : AbstractValidator<UpdateAirsofterCommand>
{
    public UpdateAirsofterValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(v => v.KitDescription)
            .MaximumLength(1000).WithMessage("Kit description must not exceed 1000 characters");

        RuleFor(v => v.AirsoftGunsList)
            .MaximumLength(1000).WithMessage("Airsoft guns list must not exceed 1000 characters");
    }
}
