using backendTinTuc.Repositories;
using backendTinTuc.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// MongoDb settings and services configuration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(nameof(MongoDbSettings)));

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<AccountRepository>();
builder.Services.AddSingleton<INewsRepository, NewsRepository>();
builder.Services.AddSingleton<CommentRepository>();
builder.Services.AddSingleton<CrawlingData>();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Additional service configuration
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:3000")
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials(); // Allow credentials
                      });
});
builder.Services.AddMvc();

// JWT authentication configuration
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy =>
        policy.RequireRole("User"));
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin"));
});

// SignalR service registration
builder.Services.AddSignalR();

var app = builder.Build();

// MongoDB connection check and crawling
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var database = services.GetRequiredService<IMongoDatabase>();
        database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();
        Console.WriteLine("MongoDB connection is healthy");

        // Optional: Crawling data
        // var crawler = new CrawlingData(database);
        // crawler.StartCrawling();
        // if (crawler.IsCrawlingSuccessful)
        // {
        //     Console.WriteLine("Crawling completed successfully.");
        // }
        // else
        // {
        //     Console.WriteLine("Crawling failed.");
        // }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MongoDB connection failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chathub").RequireCors(MyAllowSpecificOrigins);
});

app.Run();
