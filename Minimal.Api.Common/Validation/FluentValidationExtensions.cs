using FluentValidation;
using FluentValidation.Results;

namespace Minimal.Api.Common.Validation;

public static class FluentValidationExtensions
{
    public static IDictionary<string, string[]> ToValidationDictionary(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.ErrorMessage).ToArray()
            );
    }

    public static void MinimumAge<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder, int minimumAge)
    {
        ruleBuilder.SetValidator(new MinimumAge<T, TElement>(minimumAge));
    }
}