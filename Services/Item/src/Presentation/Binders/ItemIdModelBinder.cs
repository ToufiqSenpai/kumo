using Item.Application.Repositories;
using Item.Presentation.Constants;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;

namespace Item.Presentation.Binders;

public class ItemIdModelBinder(IItemRepository itemRepository) : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

        if (value is not null && value.Equals("root"))
        {
            var userId =  bindingContext.HttpContext.Items[HttpContextItemKey.UserId] as Guid? ?? Guid.Empty;
            var rootFolderId = await itemRepository.GetRootFolderIdByUserIdAsync(
                userId,
                bindingContext.HttpContext.RequestAborted);
            
            bindingContext.HttpContext.Items[HttpContextItemKey.ItemId] = rootFolderId;
            bindingContext.Result = ModelBindingResult.Success(rootFolderId);
        }
        else if (ObjectId.TryParse(value, out var objectId))
        {
            bindingContext.HttpContext.Items[HttpContextItemKey.ItemId] = objectId;
            bindingContext.Result = ModelBindingResult.Success(objectId);
        }
        else
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid ObjectId format.");
        }
    }
}