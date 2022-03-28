using System.Linq;
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
                x => Enumerable.ToArray<string>(x.Select(y => y.ErrorMessage))
            );
    }

    public static IRuleBuilderOptions<T, TElement> MinimumAge<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder, int minimumAge)
    {
        return ruleBuilder.SetValidator(new MinimumAge<T, TElement>(minimumAge));
    }
}