using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// הוספת Swagger לשירותים
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הוספת Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific",
      policy =>
      {
          policy.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod();   // מאפשר כל שיטת HTTP
      });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        // החליפו את הדומיין הזה בדומיין *המדויק* של האפליקציה שלכם
        builder.WithOrigins("https://todolistserver-otge.onrender.com") // דוגמה
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// הוספת שירותי חיבור למסד נתונים
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
    return new MySqlConnection(connectionString);
});

var app = builder.Build();

// הפעלת המדיניות של CORS
app.UseCors("AllowSpecific");

// הפעלת Swagger בהתאם לסביבה

app.UseSwagger(); // יוצר את המסמכים של Swagger
app.UseSwaggerUI(); // מציג את המסמכים של Swagger


// הגדרת מסלולים
app.MapGet("/items", async (IDbConnection db) =>
{
    var items = await db.QueryAsync<Item>("select * from items");
    return Results.Ok(items);
});

app.MapPost("/items", async ([FromBody] ItemDTOPost itemDTOPost, IDbConnection db) =>
{
    var query = "insert into items (Name,IsComplete) values (@Name,false) ";
    var result = await db.ExecuteAsync(query, new { Name = itemDTOPost.Name });

    return Results.Ok(new { Message = "successFul Post" });
});

app.MapPut("/items/{id}", async (int id, [FromBody] ItemDTOPut itemDTOPut, IDbConnection db) =>
{
    if (id <= 0)
    {
        return Results.BadRequest(new { Message = "ID is required" });
    }

    var query = "UPDATE items SET IsComplete = @IsComplete WHERE Id = @Id";
    var result = await db.ExecuteAsync(query, new { Id = id, itemDTOPut.IsComplete });

    if (result > 0)
    {
        return Results.Ok(new { Message = "Item updated successfully" });
    }
    else
    {
        return Results.NotFound(new { Message = "Item not found" });
    }
});

app.MapDelete("/items/{id}", async (int id, IDbConnection db) =>
{
    var query = "Delete from items WHERE Id = @Id";
    var result = await db.ExecuteAsync(query, new { Id = id });

    if (result > 0)
    {
        return Results.Ok(new { Message = "Item deleted successfully" });
    }
    else
    {
        return Results.NotFound(new { Message = "Item not found" });
    }
});

app.MapGet("/", () => "Hello World!");

app.Run();

public record ItemDTOPost(string Name);//add
public record ItemDTOPut(string Name, bool IsComplete);//update








//tamar
// using Dapper;
// using Microsoft.AspNetCore.Mvc;
// using MySql.Data.MySqlClient;
// using System.Data;
// using TodoApi;

// var builder = WebApplication.CreateBuilder(args);

// //swagger
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// //cors
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()    // מתיר לכל מקור לגשת
//               .AllowAnyMethod()    // מתיר לכל שיטה (GET, POST, PUT, DELETE וכו') לעבוד
//               .AllowAnyHeader();   // מתיר לכל כותרת להיות נשלחת
//     });
// });

// // הגדרת החיבור למסד נתונים כ- service
// builder.Services.AddScoped<IDbConnection>((services) =>
//     new MySqlConnection(builder.Configuration.GetConnectionString("practicodeSQL")));


// var app = builder.Build();

// //מאפשר גישה לכל המתודות שלי
// app.UseCors("AllowAll");

// // הפעלת Swagger בהתאם לסביבה
// // if (app.Environment.IsDevelopment())
// // {
//     app.UseSwagger(); // יוצר את המסמכים של Swagger
//     app.UseSwaggerUI(options => // יוצר את הממשק של Swagger UI
//     {
//         options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
//         options.RoutePrefix = string.Empty; // מציג את Swagger ב-root של האפליקציה
//     });
// // }

// //swagger
// app.UseSwagger();
// app.UseSwaggerUI();

// // מחרוזת חיבור
// var connectionString = builder.Configuration.GetConnectionString("ToDoDB");

// // קבלת כל הפריטים
// app.MapGet("/items", async (IDbConnection db) =>
// {
//     var items = await db.QueryAsync<Item>("SELECT * FROM items");
//     return Results.Json(items);
// });

// // הוספת פריט חדש
// app.MapPost("/items", async (IDbConnection db, [FromBody] ItemDto itemDto) =>
// {
//     string Name = itemDto.Name;
//     try
//     {
//         if (string.IsNullOrWhiteSpace(Name))
//             return Results.BadRequest(new { message = "Name is required." });

//         var sql = @"
//         INSERT INTO items (Name, IsComplete) 
//         VALUES (@Name, false);
//         SELECT LAST_INSERT_ID();";

//         var id = await db.QuerySingleAsync<int>(sql, new { Name });
//         return Results.Ok(new { Id = id, Message = "Item added successfully." });
//     }
//     catch (Exception ex)
//     {

//         return Results.BadRequest(new { message = ex.Message });
//     }
// });

// // עדכון פריט קיים
// app.MapPut("/items/{id}", async (IDbConnection db, int id, [FromBody] ItemDto itemDto) =>
// {

//     if (id == 0 || itemDto == null)
//         return Results.BadRequest(new { message = "Invalid data." });
  
//     var sql = @"
//         UPDATE items 
//         SET IsComplete = @IsComplete 
//         WHERE Id = @id";

//     var result = await db.ExecuteAsync(sql, new {  Id = id , itemDto.IsComplete});

//     if (result > 0)
//     {
//         return Results.Ok(new { message = "Item updated successfully." });
//     }
//     else
//     {
//         return Results.NotFound(new { message = "Item not found." });
//     }
// });

// //מחיקת פריט
// app.MapDelete("/items/{id}", async (IDbConnection db, int id) =>
// {
//     if (id <= 0)
//         return Results.BadRequest(new { message = "Invalid id." });

//     var sql = @"DELETE FROM items WHERE Id = @id;";

//     var result = await db.ExecuteAsync(sql, new { Id = id });

//     if (result > 0)
//     {
//         return Results.Ok(new { message = "Item deleted successfully." });
//     }
//     else
//     {
//         return Results.NotFound(new { message = "Item not found." });
//     }
// });

// // ברירת מחדל
// app.MapGet("/", () => "Task List API is runnig!");
// app.Run();

// //מיפויים
// public record ItemDto(string Name, bool IsComplete);





//naomi
// using Microsoft.EntityFrameworkCore;
// using TodoApi;
// var builder = WebApplication.CreateBuilder(args);

// // הגדרת חיבור למסד נתונים
// builder.Services.AddDbContext<ToDoDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
//     ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))));
// // הגדרת CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowSpecificOrigin", policy =>
//     {
//         policy.AllowAnyOrigin()
//               .AllowAnyHeader()
//               .AllowAnyMethod();
//     });
// });

// // הוסף את Swagger לפני יצירת ה-build של האפליקציה
// builder.Services.AddSwaggerGen();  // הוספת Swagger
// builder.Services.AddEndpointsApiExplorer(); // יוצר את ה-EndPoints עבור Swagger

// var app = builder.Build();

// // קביעת אם אנחנו בסביבת פיתוח
// app.UseCors("AllowSpecificOrigin");

// // if (app.Environment.IsDevelopment())
// // {
// app.UseSwagger();  // מפעיל את Swagger
// app.UseSwaggerUI();  // מפעיל את UI של Swagger
// // }


// app.MapGet("/", () => "Hello World!");


// app.MapGet("/api/items", async (ToDoDbContext dbContext) =>
// {
//     var items = await dbContext.Items.ToListAsync();
//     return Results.Ok(items);
   
// });

// app.MapPost("/api/items", async (ToDoDbContext dbContext, Item newItem) =>
// {
//     dbContext.Items.Add(newItem);
//     await dbContext.SaveChangesAsync();

//     return Results.Created($"/api/items/{newItem.Id}", newItem);
// });

// app.MapPut("/api/items/{id}", async (ToDoDbContext dbContext, int id, bool IsComplete) =>
// {

//     var existingItem = await dbContext.Items.FindAsync(id);

//     if (existingItem == null)
//     {
//         return Results.NotFound();
//     }

//     existingItem.IsComplete = IsComplete;
//     await dbContext.SaveChangesAsync();

//     return Results.Ok(existingItem);

// });

// app.MapDelete("/api/items/{id}", async (ToDoDbContext dbContext, int id) =>
// {

//     var existingItem = await dbContext.Items.FindAsync(id);
//     if (existingItem == null)
//     {
//         return Results.NotFound();
//     }
//     dbContext.Items.Remove(existingItem);
//     await dbContext.SaveChangesAsync();
//     return Results.Ok(existingItem + " deleted");

// });

// app.Run();