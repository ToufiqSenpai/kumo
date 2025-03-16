using Microsoft.AspNetCore.Mvc;

namespace Item.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemController : ControllerBase
{
    private readonly ILogger<ItemController> _logger;
    private readonly IItemService _itemService;

    public ItemController(ILogger<ItemController> logger, IItemService itemService)
    {
        _logger = logger;
        _itemService = itemService;
    }

    [HttpGet]
    public async Task<IEnumerable<Item>> Get()
    {
        return await _itemService.GetItems();
    }

    [HttpGet("{id}")]
    public async Task<Item> Get(int id)
    {
        return await _itemService.GetItem(id);
    }

    [HttpPost]
    public async Task<Item> Post([FromBody] Item item)
    {
        return await _itemService.AddItem(item);
    }

    [HttpPut("{id}")]
    public async Task<Item> Put(int id, [FromBody] Item item)
    {
        return await _itemService.UpdateItem(id, item);
    }

    [HttpDelete("{id}")]
    public async Task Delete(int id)
    {
        await _itemService.DeleteItem(id);
    }
}