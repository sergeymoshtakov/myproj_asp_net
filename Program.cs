using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using myproj.Data;
using myproj.Models;

namespace myproj
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Настройка базы данных
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true
                    };
                });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapPost("/login", async (AppDbContext db, Person loginData) =>
            {
                // находим пользователя 
                Person? person = await db.People.FindAsync(loginData.Email);
                if (person == null || person.Password != loginData.Password)
                    return Results.Unauthorized();

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, person.Email) };
                // создаем JWT-токен
                var jwt = new JwtSecurityToken(
                        issuer: AuthOptions.ISSUER,
                        audience: AuthOptions.AUDIENCE,
                        claims: claims,
                        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                // формируем ответ
                var response = new
                {
                    access_token = encodedJwt,
                    username = person.Email
                };

                return Results.Json(response);
            });

            app.MapPost("/register", async (AppDbContext db, Person registerData) =>
            {
                // Проверяем, существует ли пользователь с таким email
                if (await db.People.FindAsync(registerData.Email) != null)
                {
                    return Results.BadRequest(new { message = "Пользователь с таким email уже существует" });
                }

                // Добавляем нового пользователя
                db.People.Add(new Person(registerData.Email, registerData.Password));
                await db.SaveChangesAsync();
                return Results.Ok(new { message = "Регистрация прошла успешно" });
            });

            app.Map("/data", [Authorize] () => new { message = "Hello World!" });

            app.Run();
        }

        public class AuthOptions
        {
            public const string ISSUER = "MyAuthServer"; // издатель токена
            public const string AUDIENCE = "MyAuthClient"; // потребитель токена
            const string KEY = "mysupersecret_secretsecretsecretkey!123";   // ключ для шифрации
            public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }
    }
}
