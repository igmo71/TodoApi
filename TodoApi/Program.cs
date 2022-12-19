using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDb>(opt =>
    opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);


app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    var result = await db.Todos
        .Select(t => new TodoItemDTO(t))
        .ToArrayAsync();

    return TypedResults.Ok(result);
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    var result = await db.Todos
        .Where(t => t.IsComplete)
        .Select(t => new TodoItemDTO(t))
        .ToArrayAsync();

    return TypedResults.Ok(result);
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(TodoItemDTO todoItemDto, TodoDb db)
{
    var todoItem = new Todo
    {
        Name = todoItemDto.Name,
        IsComplete = todoItemDto.IsComplete
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/{todoItem.Id}", todoItemDto);
}

static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDto, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);
    
    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDto.Name;
    todo.IsComplete = todoItemDto.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.Ok(todo);
    }

    return TypedResults.NotFound();
}


