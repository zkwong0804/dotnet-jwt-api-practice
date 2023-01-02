using JwtApiPracitice.Auth;
using JwtApiPracitice.Persistence;
using Microsoft.EntityFrameworkCore;
using JwtApiPracitice.Settings;
using Microsoft.Net.Http.Headers;

namespace JwtApiPracitice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string corsPolicyName = "MyPolicy";
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<StudentDbContext>(opt => {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("db"));
            });
            builder.Services.AddIdentity<StudentUser, StudentRole>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 1;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<StudentDbContext>();
            var rawJwtSettings = builder.Configuration.GetSection("Jwt");
            builder.Services.Configure<JwtSettings>(rawJwtSettings);
            builder.Services.AddAuth(rawJwtSettings.Get<JwtSettings>());
            //builder.Services.AddCors(opt => {
            //    opt.AddPolicy(name: corsPolicyName, policy => {
            //        policy.WithOrigins("http://localhost:3000/");
            //    });
            //});
            builder.Services.AddCors();

            // Add services to the container.
            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseCors(opt => {
                opt.WithOrigins("http://localhost:3000")
                .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization);
            });
            app.UseHttpsRedirection();

            app.UseAuth();

            app.MapControllers();

            app.Run();
        }
    }
}