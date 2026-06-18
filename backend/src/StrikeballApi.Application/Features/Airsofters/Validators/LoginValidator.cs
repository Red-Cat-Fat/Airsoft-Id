using FluentValidation;
using StrikeballApi.Application.Features.Auth.Commands;

namespace StrikeballApi.Application.Features.Airsofters.Validators;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
