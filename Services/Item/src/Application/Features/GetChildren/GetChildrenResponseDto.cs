namespace Item.Application.Features.GetChildren;

public class GetItemsResponseDto
{
    public List<Domain.Models.Item> Data { get; set; }
    
    public string? NextToken { get; set; }
}