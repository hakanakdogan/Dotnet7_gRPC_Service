using Microsoft.EntityFrameworkCore;
using System.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    }
}
