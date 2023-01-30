#region Imports

using Microsoft.AspNetCore.Mvc.Rendering;
using Portfolio.Enums;

#endregion

namespace Portfolio.Extensions;

public static class MultiSelectListExtension
{
    public static List<SelectListItem> CreateProjectSelectListItems(this List<SelectListItem> selectListItems)
    {
        selectListItems = new List<SelectListItem>();
        foreach (var category in Enum.GetValues(typeof(ProjectCategories)))
        {
            var enumCategory = (Enum)category;
            var categoryDisplayName = enumCategory.GetEnumDisplayName();
            var categorySelectListItem = new SelectListItem
            {
                Text = categoryDisplayName,
                Value = categoryDisplayName
            };
            selectListItems.Add(categorySelectListItem);
        }

        return selectListItems;
    }
}