
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using SocialMediaApp.DAL;
using SocialMediaApp.DAL.context;
using SocialMediaApp.DAL.Models;
using SocialMediaAppp.BL;
using System.Security.Claims;
using System.Text;

namespace SocialMediaApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            //configer DbContext 
            var ConnectionString = builder.Configuration.GetConnectionString("MyConnectionString");
            builder.Services.AddDbContext<SocialMediAppContext>(option =>
            {
                option.UseSqlServer(ConnectionString);
            });


            // register all servicers 
            builder.Services.RegisterDAL();
            builder.Services.RegisterBL();


            //configer identity Service 
            builder.Services.AddIdentity<AppUser, IdentityRole>(
                option =>
                {
                    option.Password.RequiredLength = 6;
                }
                ).AddEntityFrameworkStores<SocialMediAppContext>();



            //add auth middleware 
            object value = builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = "MyDefault";
                option.DefaultChallengeScheme = "MyDefault";
            }).AddJwtBearer("MyDefault", option => {

                // generta the secret key for the token
                var userSKey = builder.Configuration.GetValue<string>("TokenSecret");
                var KeyByites = Encoding.ASCII.GetBytes(userSKey);
                var Key = new SymmetricSecurityKey(KeyByites);

                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = Key
                };
            });


          
            builder.Services.AddCors(option =>
            {
                option.AddPolicy("allowall" , b =>
                {
                    b.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("allowall");


            var folderPath = Path.Combine(builder.Environment.ContentRootPath, "Uploades");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var UploadeFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploades");


            //server this folder 
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(UploadeFolderPath),
                RequestPath = "/Uploades"
            });

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
