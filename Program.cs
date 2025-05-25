using HealthApp.EF.Reposiotries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Product.Core;
using Product.EF;
using Product.EF.Repositoies;
using SharedLiberary.Core.Interfaces;
using SharedLiberary.Models.UserManagment;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
IConfiguration config = builder.Configuration;

// Add services to the container.
builder.Services.Configure<UserJWT>(config.GetSection("JWT"));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ProductDbContext>();
builder.Services.AddScoped<IAutherRepository, AutherReposity>();
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlite(config.GetConnectionString("ProductDB")));

var trqOrigins = "productOrig";
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));


builder.Services.AddControllers();
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata=false;
        o.SaveToken=false;
        o.TokenValidationParameters=new TokenValidationParameters
        {
            ValidateIssuerSigningKey=true,
            ValidateIssuer=true,
            ValidateAudience=true,
            ValidateLifetime=true,
            ValidIssuer=config["JWT:Issuer"],
            ValidAudience=config["JWT:Audience"],
            IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"])),
            ClockSkew=TimeSpan.Zero
        };
    });

builder.Services.AddAutoMapper(typeof(ProductMap));
builder.Services.AddTransient(typeof(IProductUnIts), typeof(ProductUnit));

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

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(trqOrigins);
app.MapControllers();

app.Run();
