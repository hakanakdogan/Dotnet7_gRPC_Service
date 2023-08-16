using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class TodoService: ToDoIt.ToDoItBase
{
    private readonly AppDbContext _appDbContext;

    public TodoService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public override async Task<CreateTodoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
    {
        if (request.Title == string.Empty || request.Description == string.Empty)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));
        }

        var todoItem = new TodoItem
        {  
            Description = request.Description,
            Title = request.Title,

        };
        _appDbContext.TodoItems.Add(todoItem);
        await _appDbContext.SaveChangesAsync();

        return await Task.FromResult(new CreateTodoResponse
        {
            Id = todoItem.Id
        });
    }

    public override async Task<ReadTodoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext context)
    {
        if (request.Id < 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than zero"));
        }
        var todoItem = await _appDbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == request.Id);
        if (todoItem == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Todo with given ID ({request.Id}) was not found"));
        }
        return await Task.FromResult(new ReadTodoResponse { Id = todoItem.Id, Description = todoItem.Description, Title = todoItem.Title, ToDoStatus = todoItem.TodoStatus });
    }

    public override async Task<GetAllResponse> ListTodo(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();
        var todoItems = await _appDbContext.TodoItems.ToListAsync();
        foreach (var item in todoItems)
        {
            response.ToDo.Add(new ReadTodoResponse
            {
                Id = item.Id,
                Description = item.Description,
                Title = item.Title,
                ToDoStatus = item.TodoStatus,
            });
        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
    {
        var entity = await _appDbContext.TodoItems.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
        if(entity == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Todo with given ID ({request.Id}) was not found"));
        entity.Title = string.IsNullOrEmpty(request.Title) ? entity.Title : request.Title;
        entity.Description = string.IsNullOrEmpty(request.Description) ? entity.Description : request.Description;
        entity.TodoStatus = string.IsNullOrEmpty(request.ToDoStatus) ? entity.TodoStatus : request.ToDoStatus;

        _appDbContext.TodoItems.Update(entity);
        await _appDbContext.SaveChangesAsync();
        return await Task.FromResult(new UpdateToDoResponse { Id = entity.Id });
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
    {
        var entity = await _appDbContext.TodoItems.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
        if (entity == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Todo with given ID ({request.Id}) was not found"));
        _appDbContext.TodoItems.Remove(entity);
        await _appDbContext.SaveChangesAsync();
        return await Task.FromResult(new DeleteToDoResponse { Id = request.Id });

    }
}

