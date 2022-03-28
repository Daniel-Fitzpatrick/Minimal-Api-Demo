using FluentValidation;
using FluentValidation.Validators;

namespace Minimal.Api.Common.Validation;

public class MinimumAge<T, TDate> : PropertyValidator<T, TDate>
{
    private readonly int _minAge;

    public MinimumAge(int minAge)
    {
        _minAge = minAge;
    }

    public override bool IsValid(ValidationContext<T> context, TDate value)
    {
        context.MessageFormatter.AppendArgument("MinimumAge", _minAge);
        return value is DateTime date && date.AddYears(_minAge) >= DateTime.Now;
    }

    public override string Name => "MinimumAge";

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "The minimum age is {MinimumAge}.";
}