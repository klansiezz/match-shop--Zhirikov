using Microsoft.EntityFrameworkCore;

Console.WriteLine("Магазин спичек «Чирк»: проект запущен!");

// Шаги 1-2. Создание базы
using var db = new ShopContext();
db.Database.EnsureDeleted();   // удалить базу, если она осталась с прошлого запуска
db.Database.EnsureCreated();   // создать базу и таблицы по классам (Customers, Products, Orders)
 
// Шаг 3. Заполнение 
Console.WriteLine(" Шаг 3 ");
 
// Покупатели
var ivan = new Customer { Name = "Иван Петров", Phone = "+7 900 111-22-33", City = "Омск" };
var olga = new Customer { Name = "Ольга Смирнова", Phone = "+7 900 444-55-66", City = "Томск" };
var maria = new Customer { Name = "Мария Козлова", Phone = "+7 900 777-88-99", City = "Казань" };
var dmitry = new Customer { Name = "Дмитрий Орлов", Phone = "+7 900 123-45-67", City = "Омск" };
var anna = new Customer { Name = "Анна Белова", Phone = "+7 900 765-43-21", City = "Тверь" };
db.Customers.Add(ivan);
db.Customers.Add(olga);
db.Customers.AddRange(maria, dmitry, anna);
 
// Товары (два дешевле 100 ₽: «Хозяйственные» и «Для гриля»)
var household = new Product { Name = "Хозяйственные", Price = 45m, MatchesPerBox = 60, Stock = 120, Description = "Обычные спички для дома" };
var hunting = new Product { Name = "Охотничьи", Price = 180m, MatchesPerBox = 25, Stock = 40, Description = "Горят даже под дождём" };
var fireplace = new Product { Name = "Каминные", Price = 150m, MatchesPerBox = 40, Stock = 55, Description = "Длинные, для розжига камина" };
var sparkler = new Product { Name = "Бенгальские", Price = 220m, MatchesPerBox = 12, Stock = 30 };   // описания нет: будет NULL
var grill = new Product { Name = "Для гриля", Price = 85m, MatchesPerBox = 50, Stock = 80 };
var souvenir = new Product { Name = "Сувенирные", Price = 320m, MatchesPerBox = 20, Stock = 15, Description = "Подарочная упаковка" };
db.Products.AddRange(household, hunting, fireplace, sparkler, grill, souvenir);
 
db.Orders.AddRange(
    new Order { Customer = ivan, Product = fireplace, Quantity = 3, OrderDate = DateTime.UtcNow.AddDays(-6) },
    new Order { Customer = ivan, Product = household, Quantity = 10, OrderDate = DateTime.UtcNow.AddDays(-4) },
    new Order { Customer = maria, Product = hunting, Quantity = 2, OrderDate = DateTime.UtcNow.AddDays(-3) },
    new Order { Customer = dmitry, Product = grill, Quantity = 5, OrderDate = DateTime.UtcNow.AddDays(-2) },
    new Order { Customer = anna, Product = sparkler, Quantity = 1, OrderDate = DateTime.UtcNow.AddDays(-1) }
);
db.SaveChanges();   // только здесь данные уходят в базу
 
// Шаг 4. Выборка 
Console.WriteLine(" Шаг 4 ");
 
// Пример: покупатели из Омска по алфавиту
var omsk = db.Customers
    .Where(c => c.City == "Омск")   // только из Омска
    .OrderBy(c => c.Name)            // по алфавиту
    .ToList();                       // здесь запрос уходит в базу
 
foreach (var c in omsk)
    Console.WriteLine($"{c.Name}, {c.Phone}");
 
// Задание 1. Спички дешевле 100 ₽ от дешёвых к дорогим
Console.WriteLine("--- Дешевле 100 ₽ ---");
var cheap = db.Products
    .Where(p => p.Price < 100)
    .OrderBy(p => p.Price)
    .ToList();
foreach (var p in cheap)
    Console.WriteLine($"{p.Name} — {p.Price:0.##} ₽");
 
// Задание 2. Сколько видов спичек есть на складе (остаток больше нуля)
Console.WriteLine("--- Видов на складе ---");
var inStockCount = db.Products.Count(p => p.Stock > 0);
Console.WriteLine($"Видов спичек в наличии: {inStockCount}");
 
// Задание 3. Три самых дорогих товара
Console.WriteLine("--- Три самых дорогих ---");
var top3 = db.Products
    .OrderByDescending(p => p.Price)
    .Take(3)
    .ToList();
foreach (var p in top3)
    Console.WriteLine($"{p.Name} — {p.Price:0.##} ₽");
 
Console.WriteLine("--- Все заказы ---");
var allOrders = db.Orders
    .Include(o => o.Customer)
    .Include(o => o.Product)
    .OrderBy(o => o.Id)
    .ToList();
 
foreach (var o in allOrders)
{
    var sum = o.Product.Price * o.Quantity;   // цена товара × количество коробок
    Console.WriteLine($"{o.Customer.Name} — {o.Product.Name} — {o.Quantity} шт. — {sum:0.##} ₽");
}
var revenue = allOrders.Sum(o => o.Product.Price * o.Quantity);
Console.WriteLine($"Общая выручка: {revenue:0.##} ₽");
 
// Шаг 5. Изменение
Console.WriteLine(" Шаг 5 ");
 
var customer = db.Customers.First(c => c.Name == "Иван Петров");   // найти
customer.Phone = "+7 900 999-00-00";                                  // изменить
db.SaveChanges();   // EF сам видит, что изменилось, и отправляет UPDATE
 
// Задание 1. Все спички дороже на 10%
foreach (var p in db.Products.ToList())
    p.Price = Math.Round(p.Price * 1.1m, 2);   // 1.1m — число типа decimal
db.SaveChanges();                              // одного SaveChanges достаточно: EF отправит UPDATE для каждого товара
 
// Задание 2. Один из товаров закончился («Сувенирные», заказов на него нет)
var sold = db.Products.First(p => p.Name == "Сувенирные");
sold.Stock = 0;
db.SaveChanges();
 
// Усложнение. Функция «Оформить заказ»
void PlaceOrder(ShopContext context, Customer buyer, Product product, int boxes)
{
    if (product.Stock < boxes)
    {
        Console.WriteLine($"Недостаточно на складе: {product.Name} (есть {product.Stock}, нужно {boxes})");
        return;   // ничего не меняем
    }
 
    context.Orders.Add(new Order
    {
        Customer = buyer,
        Product = product,
        Quantity = boxes,
        OrderDate = DateTime.UtcNow
    });
    product.Stock -= boxes;   // уменьшаем остаток
 

    context.SaveChanges();
    Console.WriteLine($"Заказ оформлен: {buyer.Name} — {product.Name} — {boxes} шт. Остаток: {product.Stock}");
}
 
// Проверка 1: товара хватает
var annaDb = db.Customers.First(c => c.Name == "Анна Белова");
var householdDb = db.Products.First(p => p.Name == "Хозяйственные");
PlaceOrder(db, annaDb, householdDb, 5);
 
// Проверка 2: товара не хватает
var mariaDb = db.Customers.First(c => c.Name == "Мария Козлова");
var huntingDb = db.Products.First(p => p.Name == "Охотничьи");
PlaceOrder(db, mariaDb, huntingDb, 1000);
 
// Шаг 6. Удаление 
Console.WriteLine(" Шаг 6 ");
 

var leaving = db.Customers.First(c => c.Name == "Ольга Смирнова");   // найти
db.Customers.Remove(leaving);                                         // пометить на удаление
db.SaveChanges();                                                     // отправить DELETE
 
// Задание. Удалить товары с остатком 0
var soldOut = db.Products.Where(p => p.Stock == 0).ToList();
db.Products.RemoveRange(soldOut);
db.SaveChanges();
Console.WriteLine($"Товаров осталось: {db.Products.Count()}");
 

var offended = db.Customers
    .Include(c => c.Orders)
    .First(c => c.Name == "Дмитрий Орлов");
db.Customers.Remove(offended);
db.SaveChanges();
Console.WriteLine($"Заказов осталось: {db.Orders.Count()}");
