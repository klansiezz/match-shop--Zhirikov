using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
 
public class ShopContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }   // таблица Customers
    public DbSet<Product> Products { get; set; }     // таблица Products
    public DbSet<Order> Orders { get; set; }         // таблица Orders
 
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Database=match_shop;Username=postgres;Password=postgres");
        options.LogTo(Console.WriteLine, LogLevel.Information);   // показывать SQL, который отправляет EF
    }
}
