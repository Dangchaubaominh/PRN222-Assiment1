using Microsoft.EntityFrameworkCore;
using RagChatbot.DAL.Data;
using RagChatbot.DAL.Repositories.Interfaces;
using RagChatbot.DAL.Repositories.Implements;
using RagChatbot.BLL.Services.Interfaces;
using RagChatbot.BLL.Services.Implements;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// Register DAL Repositories
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IBenchmarkRepository, BenchmarkRepository>();

// Register BLL Services
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IBenchmarkService, BenchmarkService>();

var app = builder.Build();

// Seed Default Database Admin Account and Demo Data
using (var scope = app.Services.CreateScope())
{
    var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Ensure DB is created and migrations are applied
    try
    {
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration Error: {ex.Message}");
    }

    accountService.SeedAdminAccount();

    if (!dbContext.Subjects.Any())
    {
        dbContext.Subjects.Add(new RagChatbot.DAL.Entities.Subject
        {
            Id = Guid.NewGuid(),
            Code = "PRN222",
            Name = "Lập trình mạng C#",
            CreatedAt = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); // Enable Authentication
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
