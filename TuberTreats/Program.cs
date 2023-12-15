using System.Collections.Specialized;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using TuberTreats.Models;
using TuberTreats.Models.DTOs;

// Drivers
List<TuberDriver> drivers = new()
{
    new() { Id = 1, Name = "Dale Earnhardt"},
    new() { Id = 2, Name = "Jimmie Johnson"},
    new() { Id = 3, Name = "Ricky Bobby"}
};

// Customers
List<Customer> customers = new()
{
    new() { Id = 1, Name = "Bob Ross", Address = "123 Main Street"},
    new() { Id = 2, Name = "Michael Jordan", Address = "321 Main Street"},
    new() { Id = 3, Name = "Michelle Obama", Address = "456 Other Street"},
    new() { Id = 4, Name = "Taylor Swift", Address = "111 That Street"},
    new() { Id = 5, Name = "Tom Cruise", Address = "789 Grape Street"}
};

// Topping Options
List<Topping> toppings = new()
{
    new() { Id = 1, Name = "Bacon"},
    new() { Id = 2, Name = "Sour Cream"},
    new() { Id = 3, Name = "Chives"},
    new() { Id = 4, Name = "Butter"},
    new() { Id = 5, Name = "Salt"}
};

// Orders
List<TuberOrder> orders = new()
{
    new() { Id = 1, CustomerId = 5, TuberDriverId = 1, OrderPlacedOnDate = new DateTime(2023, 11 , 23), DeliveredOnDate = null},
    new() { Id = 2, CustomerId = 4, TuberDriverId = 2, OrderPlacedOnDate = new DateTime(2023, 12 , 2), DeliveredOnDate = DateTime.Now},
    new() { Id = 3, CustomerId = 3, TuberDriverId = 3, OrderPlacedOnDate = new DateTime(2023, 11 , 28), DeliveredOnDate = DateTime.Now},
    new() { Id = 4, CustomerId = 2, TuberDriverId = 1, OrderPlacedOnDate = new DateTime(2023, 12 , 11), DeliveredOnDate = DateTime.Now},
    new() { Id = 5, CustomerId = 1, OrderPlacedOnDate = new DateTime(2023, 12 , 10), DeliveredOnDate = null}
};

// Toppings for orders
List<TuberTopping> tuberToppings = new()
{
    new() { Id = 1, ToppingId = 1, TuberOrderId = 1 },
    new() { Id = 2, ToppingId = 2, TuberOrderId = 2 },
    new() { Id = 3, ToppingId = 3, TuberOrderId = 3 },
    new() { Id = 4, ToppingId = 4, TuberOrderId = 4 },
    new() { Id = 5, ToppingId = 5, TuberOrderId = 5 }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Order endpoints ---------------------------------------

// Get all orders
app.MapGet("/tuberorders", () =>
{
    return orders.Select(o =>
        {
            var customer = customers.FirstOrDefault(c => c.Id == o.CustomerId);
            var driver = drivers.FirstOrDefault(d => d.Id == o.TuberDriverId);
            // find where tuberTopping order id = order id
            var orderToppings = tuberToppings.Where(tt => tt.TuberOrderId == o.Id);
            // create a list of toppings that match toppings for order
            var toppingsForOrder = orderToppings.Select(ot =>
            {
                // find matching tuberTopping to toppings based off id
                var matchingTopping = toppings.FirstOrDefault(t => t.Id == ot.ToppingId);

                // check if matchingTopping is null and if not then return matching object
                return matchingTopping != null ? new ToppingDTO
                {
                    Id = matchingTopping.Id,
                    Name = matchingTopping.Name
                } : null; 
            }).ToList();

            return new TuberOrderDTO
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                TuberDriverId = o.TuberDriverId,
                OrderPlacedOnDate = o.OrderPlacedOnDate,
                DeliveredOnDate = o.DeliveredOnDate,
                Toppings = toppingsForOrder,
                Customer = customer == null ? null : new CustomerDTO
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Address = customer.Address
                },
                Driver = driver == null ? null : new TuberDriverDTO
                {
                    Id = driver.Id,
                    Name = driver.Name,
                }
            };
        });
});

// Get orders by Id
app.MapGet("/tuberorders/{id}", (int id) =>
{
    // find where order.id = the id its given
    TuberOrder order = orders.FirstOrDefault(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound();
    }

    var customer = customers.FirstOrDefault(w => w.Id == order.CustomerId);
    var driver = drivers.FirstOrDefault(t => t.Id == order.TuberDriverId);
    // find where tuberTopping order id = order id
    var orderToppings = tuberToppings.Where(tt => tt.TuberOrderId == order.Id);
    // create a list of toppings that match toppings for order
    var toppingsForOrder = orderToppings.Select(ot =>
    {
        // find matching tuberTopping to toppings based off id
        var matchingTopping = toppings.FirstOrDefault(t => t.Id == ot.ToppingId);

        // check if matchingTopping is null and if not then return matching object
        return matchingTopping != null ? new ToppingDTO
        {
            Id = matchingTopping.Id,
            Name = matchingTopping.Name
        } : null; 
    }).ToList();

    return Results.Ok(new TuberOrderDTO
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        TuberDriverId = order.TuberDriverId,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        DeliveredOnDate = order.DeliveredOnDate,
        Toppings = toppingsForOrder,
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Driver = driver == null ? null : new TuberDriverDTO
        {
            Id = driver.Id,
            Name = driver.Name,
        }
    });

});

// Post a new order 
app.MapPost("/tuberorders", (TuberOrder order) =>
{
    // Assigns Id to new order
    order.Id = orders.Max(o => o.Id) + 1;

    // adds order to order list
    orders.Add(order);

    // returns new order
    return Results.Created($"/tuberorders/{order.Id}", new TuberOrderDTO
    {
        Id = order.Id,
        CustomerId = order.CustomerId,
        TuberDriverId = order.TuberDriverId,
        OrderPlacedOnDate = DateTime.Now,
        DeliveredOnDate = null,
    });
});

// Assign driver to order
app.MapPut("/tuberorders/{id}", (int id, TuberOrder order) =>
{
    TuberOrder orderToUpdate = orders.FirstOrDefault(o => o.Id == id);

    if (orderToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != order.Id)
    {
        return Results.BadRequest();
    }

    orderToUpdate.Id = order.Id;
    orderToUpdate.CustomerId = order.CustomerId;
    orderToUpdate.TuberDriverId = order.TuberDriverId;
    orderToUpdate.OrderPlacedOnDate = order.OrderPlacedOnDate;
    orderToUpdate.DeliveredOnDate = order.DeliveredOnDate;


    return Results.NoContent();
});

// Mark order as completed by posting a DeliveredOnDate to the order based off of Id 
app.MapPost("/tuberorders/{id}/complete", (int id) =>
{

    TuberOrder orderToComplete = orders.FirstOrDefault(o => o.Id == id);

    if (orderToComplete == null)
    {
        return Results.NotFound();
    }
    //assign DeliveredOnDate to right now 
    orderToComplete.DeliveredOnDate = DateTime.Now;

    return Results.NoContent();

});


// Topping endpoints -------------------------------------

// Get all toppings 
app.MapGet("/toppings", () =>
{
    return toppings.Select(t => new ToppingDTO
    {
        Id = t.Id,
        Name = t.Name

    });
});

// Get topping by Id
app.MapGet("/toppings/{id}", (int id) =>
{
    Topping topping = toppings.FirstOrDefault(t => t.Id == id);

    if (topping == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new ToppingDTO
    {
        Id = topping.Id,
        Name = topping.Name,

    });
});

// tuberTopping endoints ---------------------------------

// Get all tuberToppings
app.MapGet("/tubertoppings", () =>
{
    return tuberToppings.Select(tt => new TuberToppingDTO
    {
        Id = tt.Id,
        ToppingId = tt.ToppingId,
        TuberOrderId = tt.TuberOrderId
    });
});

// Add a topping to a tuberOrder (POST new tuberTopping with toppingId and tuberOrderId)
app.MapPost("/tubertoppings", (TuberTopping tuberTopping) =>
{
    // Assigns Id to new tuberTopping
    tuberTopping.Id = tuberToppings.Max(o => o.Id) + 1;

    // adds tuberTopping to list
    tuberToppings.Add(tuberTopping);

    // returns new tuberTopping
    return Results.Created($"/tubertoppings/{tuberTopping.Id}", new TuberToppingDTO
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        ToppingId = tuberTopping.ToppingId
    });
});

// Remove a topping 
app.MapDelete("/tubertoppings/{id}", (int id) =>
{
    // Find the customer by ID
    TuberTopping gettingDeleted = tuberToppings.FirstOrDefault(tt => tt.Id == id);

    if (gettingDeleted == null)
    {
        return Results.NotFound();
    }

    // Remove the customer from list
    tuberToppings.Remove(gettingDeleted);

    return Results.Ok(gettingDeleted);
});
// Customer endpoints ------------------------------------

// Get all customers
app.MapGet("/customers", () =>
{
    return customers.Select(cus =>
        {
            // find where customer id = order.CustomerId
            var customerOrders = orders.Where(o => o.CustomerId == cus.Id);

            // Create a list of TuberOrderDTO objects for the related TuberOrders
            var ordersForCustomer = customerOrders.Select(cusOrders =>
                new TuberOrderDTO
                {
                    Id = cusOrders.Id,
                    CustomerId = cusOrders.CustomerId,
                    TuberDriverId = cusOrders.TuberDriverId,
                    OrderPlacedOnDate = cusOrders.OrderPlacedOnDate,
                    DeliveredOnDate = cusOrders.DeliveredOnDate,

                })
                .ToList();

            return new CustomerDTO
            {
                Id = cus.Id,
                Address = cus.Address,
                Name = cus.Name,
                TuberOrders = ordersForCustomer,
            };
        })
        .ToList(); // entire result is converted to a list
});

// Get customers by Id
app.MapGet("/customers/{id}", (int id) =>
{
    // Find the customer by Id
    Customer customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    // Get orders for the specific customer
    var customerOrders = orders.Where(o => o.CustomerId == customer.Id);

    // Create a list of TuberOrderDTO objects for the related TuberOrders
    var ordersForCustomer = customerOrders.Select(cusOrders =>
        new TuberOrderDTO
        {
            Id = cusOrders.Id,
            CustomerId = cusOrders.CustomerId,
            TuberDriverId = cusOrders.TuberDriverId,
            OrderPlacedOnDate = cusOrders.OrderPlacedOnDate,
            DeliveredOnDate = cusOrders.DeliveredOnDate,

        })
        .ToList();

    // Create and return the CustomerDTO for the specific customer
    var thisCustomer = new CustomerDTO
    {
        Id = customer.Id,
        Address = customer.Address,
        Name = customer.Name,
        TuberOrders = ordersForCustomer,
    };

    return Results.Ok(thisCustomer);
});

// Post a new customer 
app.MapPost("/customers", (Customer customer) =>
{
    // Assigns Id to new customer
    customer.Id = customers.Max(o => o.Id) + 1;

    customers.Add(customer);

    return Results.Created($"/customers/{customer.Id}", new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address
    });
});

// Delete a customer
app.MapDelete("/customers/{id}", (int id) =>
{
    // Find the customer by ID
    Customer customerToDelete = customers.FirstOrDefault(c => c.Id == id);

    if (customerToDelete == null)
    {
        return Results.NotFound();
    }

    // Remove the customer from list
    customers.Remove(customerToDelete);

    return Results.Ok(customerToDelete);
});


// tuberDriver endpoints ---------------------------------

// Get all tuberDrivers
app.MapGet("/tuberdrivers", () =>
{
    return drivers
        .Select(dr =>
        {
            var driverOrders = orders.Where(o => o.TuberDriverId == dr.Id);

            // Create a list of TuberOrderDTO objects for the related TuberOrders
            var ordersForDriver = driverOrders.Select(drOrders =>
                new TuberOrderDTO
                {
                    Id = drOrders.Id,
                    CustomerId = drOrders.CustomerId,
                    TuberDriverId = drOrders.TuberDriverId,
                    OrderPlacedOnDate = drOrders.OrderPlacedOnDate,
                    DeliveredOnDate = drOrders.DeliveredOnDate,

                })
                .ToList();

            return new TuberDriverDTO
            {
                Id = dr.Id,
                Name = dr.Name,
                TuberDeliveries = ordersForDriver,
            };
        })
        .ToList(); // entire result is converted to a list
});

// Get tuberDrivers by Id 
app.MapGet("/tuberdrivers/{id}", (int id) =>
{
    // Find the customer by ID
    TuberDriver driver = drivers.FirstOrDefault(td => td.Id == id);

    if (driver == null)
    {
        return Results.NotFound();
    }

    // Get orders for the specific customer
    var driverOrders = orders.Where(o => o.TuberDriverId == driver.Id);

    // Create a list of TuberOrderDTO objects for the related TuberOrders
    var ordersForDriver = driverOrders.Select(drOrders =>
        new TuberOrderDTO
        {
            Id = drOrders.Id,
            CustomerId = drOrders.CustomerId,
            TuberDriverId = drOrders.TuberDriverId,
            OrderPlacedOnDate = drOrders.OrderPlacedOnDate,
            DeliveredOnDate = drOrders.DeliveredOnDate,

        })
        .ToList();

    // Create and return the CustomerDTO for the specific customer
    var thisDriver = new TuberDriverDTO
    {
        Id = driver.Id,
        Name = driver.Name,
        TuberDeliveries = ordersForDriver,
    };

    return Results.Ok(thisDriver);
});

app.Run();
//don't touch or move this!
public partial class Program { }