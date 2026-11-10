// Классы таблиц: Customer, Product, Order (шаги 1–2).
public class Customer
{
    public int Id { get; set; }              // первичный ключ: EF узнаёт его по имени Id
    public string Name { get; set; } = "";   // имя
    public string Phone { get; set; } = "";  // телефон
    public string City { get; set; } = "";   // город
 
    public List<Order> Orders { get; set; } = new();   // все заказы покупателя
}
 
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";       // название
    public decimal Price { get; set; }           // цена коробка в рублях (для денег всегда decimal)
    public int MatchesPerBox { get; set; }       // сколько спичек в коробке
    public int Stock { get; set; }               // остаток на складе, в коробках
 

    public string? Description { get; set; }
 
    public List<Order> Orders { get; set; } = new();   // все заказы на этот товар
}
 
public class Order
{
    public int Id { get; set; }
    public int Quantity { get; set; }                 // сколько коробок
 
    public DateTime OrderDate { get; set; }
 
    public int CustomerId { get; set; }               // внешний ключ: Id покупателя
    public Customer Customer { get; set; } = null!;   // сам покупатель
 
    public int ProductId { get; set; }                // внешний ключ: Id товара
    public Product Product { get; set; } = null!;     // сам товар
}