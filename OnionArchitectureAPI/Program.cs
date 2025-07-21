using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.DbContextLayer;
using Microsoft.Extensions.Logging;
using DomainLayer.Model;
using ServiceLayer.Service.Implementation;
using ServiceLayer.Service.Interface;
using Azure.Core;
using OnionArchitectureAPI.Services.Print;

var builder = WebApplication.CreateBuilder(args);

// Load secrets from Azure Key Vault if NOT in development
if (!builder.Environment.IsDevelopment())
{
    var kvUri = builder.Configuration["KeyVault:Uri"];
    if (!string.IsNullOrEmpty(kvUri))
    {
        TokenCredential credential = new DefaultAzureCredential();
        builder.Configuration.AddAzureKeyVault(new Uri(kvUri), credential);
    }
}

// Resolve connection string
string connectionString = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : builder.Configuration["DbConn-JTPLCore"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string not configured.");
}

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register application services
builder.Services.AddScoped<IUser, UserService>();
builder.Services.AddScoped<ICity, CityService>();
builder.Services.AddScoped<IEmployee, EmployeeService>();
builder.Services.AddScoped<Ilogin, LoginService>();
builder.Services.AddScoped<ICredential, CredentialServices>();
builder.Services.AddScoped<ITicketBooking, TicketBookingServices>();
builder.Services.AddScoped<Itb_Booking, tb_BookingServices>();
builder.Services.AddScoped<IGSTDetails, GSTDetailsServices>();
builder.Services.AddScoped<IAdmin, AdminService>();
builder.Services.AddScoped<ICP_GstDetail<CP_GSTModel>, CP_GSTService>();
builder.Services.AddScoped<IBooking<Booking>, BookingService>();
builder.Services.AddScoped<IRefundRequest<RefundRequest>, RefundRequestService>();
builder.Services.AddScoped<IPrintTicket<Printticket>, PrintTicketService>();
builder.Services.AddSingleton<PdfTicketService>();

// Core ASP.NET and AWS Lambda services
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Logging
CustomFontResolver.Register();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.Services.BuildServiceProvider()
    .GetRequiredService<ILogger<Program>>()
    .LogInformation("Connection string prefix: {prefix}", connectionString?.Substring(0, Math.Min(10, connectionString.Length)) + "...");

// App pipeline
var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
