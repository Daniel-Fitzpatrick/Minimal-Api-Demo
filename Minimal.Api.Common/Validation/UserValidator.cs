using FluentValidation;
using Minimal.Api.Common.Model;

namespace Minimal.Api.Common.Validation
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(u => u.Email)
                .EmailAddress()
                .WithMessage("Value must be a valid email address");

            RuleFor(u => u.FirstName)
                .NotEmpty();
            RuleFor(u => u.LastName)
                .NotEmpty();
            RuleFor(u => u.YearsOfExperience)
                .NotEmpty()
                .GreaterThan(0);
            RuleFor(u => u.Skills)
                .NotEmpty();
            RuleFor(u => u.DateOfBirth)
                .NotEmpty()
                .MinimumAge(15);
        }
      
    }
}
