using Item.Application.Repositories;
using Item.Domain.Models;
using MediatR;

namespace Item.Application.Features.CreateRootFolder;

public class CreateRootFolderHandler(IItemRepository itemRepository) : IRequestHandler<CreateRootFolderCommand, Unit>
{
    private const string RootFolderName = "root";
        
    public async Task<Unit> Handle(CreateRootFolderCommand request, CancellationToken cancellationToken)
    {
        var rootFolder = new Domain.Models.Item
        {
            Name = RootFolderName,
            Folder = new Folder
            {
                View = new FolderView()
            },
            Root = true,
            UserId = request.UserId
        };
        
        await itemRepository.AddAsync(rootFolder, cancellationToken);

        return Unit.Value;
    }
}