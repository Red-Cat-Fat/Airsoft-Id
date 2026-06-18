using FluentValidation;
using StrikeballApi.Application.Features.Auth.Commands;

namespace StrikeballApi.Application.Features.Airsofters.Validators;

public class RegisterAirsofterValidator : AbstractValidator<RegisterAirsofterCommand>
{
    public RegisterAirsofterValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit");

        RuleFor(v => v.ConfirmPassword)
            .Equal(v => v.Password).WithMessage("Passwords do not match");

        RuleFor(v => v.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(v => v.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(v => v.Callsign)
            .NotEmpty().WithMessage("Callsign is required")
            .MaximumLength(50).WithMessage("Callsign must not exceed 50 characters");
    }
}
