#region Imports

using Portfolio.Models.Content;
using Portfolio.Models.Filters;

#endregion

namespace Portfolio.Services.Interfaces;

public interface IMWSCategoryService
{
    public Task AddCategoriesAsync(Guid blogId, List<string> categoryValues);
    public Task DeleteCategoryAsync(Category category);
    public Task<List<Category>> GetCategoriesAsync(Guid blogId);
    public Task<Category> GetCategoryByIdAsync(Guid blogId, Guid categoryId);
    public Task<Category> GetCategoryByNameAsync(Guid blogId, string categoryName);
    public Task<bool> IsCategoryUnique(Guid blogId, string categoryName);
    public Task MovePostsToDefaultCategoryAsync(Guid blogId, Category categoryToDelete);
    public Task<List<string>> RemoveDuplicateCategoriesAsync(Guid blogId, List<string> categoryValues);
    public Task RemoveStaleCategories(Blog blog);
}