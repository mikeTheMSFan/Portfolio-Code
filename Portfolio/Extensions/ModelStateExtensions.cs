#region Imports

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Portfolio.Models;

#endregion

namespace Portfolio.Extensions;

public static class ModelStateExtensions
{
    public static IEnumerable<ModelStateError> AllErrors(this ModelStateDictionary modelState)
    {
        var result = new List<ModelStateError>();
        var erroneousFields = modelState.Where(ms => ms.Value!.Errors.Any())
            .Select(x => new { x.Key, x.Value!.Errors });

        foreach (var erroneousField in erroneousFields)
        {
            var fieldKey = erroneousField.Key;
            var fieldErrors = erroneousField.Errors
                .Select(error => new ModelStateError(fieldKey, error.ErrorMessage));
            result.AddRange(fieldErrors);
        }

        return result;
    }
}