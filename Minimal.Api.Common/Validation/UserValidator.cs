using FluentValidation;
using Minimal.Api.Common.Model;

namespace Minimal.Api.Common.Validation
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(u => u.Email)
                .EmailAddress();

            RuleFor(u => u.FirstName)
                .NotEmpty();

            RuleFor(u => u.LastName)
                .NotEmpty();

            RuleFor(u => u.YearsOfExperience)
                .NotEmpty()
                .GreaterThan(0);

            RuleFor(u => u.DateOfBirth)
                .NotEmpty()
                .MinimumAge(15);
        }
      
    }
}
