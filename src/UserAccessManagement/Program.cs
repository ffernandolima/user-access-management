using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using UserAccessManagement.Configurations;
using UserAccessManagement.Middlewares;

namespace UserAccessManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationServices(builder.Configuration);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
            });

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Services.CreateDatabase();
            app.UseStatusCodePages();
            app.Run();
        }
    }
}
