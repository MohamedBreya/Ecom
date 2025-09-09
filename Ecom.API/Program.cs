
using Ecom.core.Services;
using Ecom.infrastructure;
using Ecom.infrastructure.Repositories.Service;
using Microsoft.Extensions.DependencyInjection;


namespace Ecom.API
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
            //builder.Services.AddSingleton<IImageManagmentService, ImageManagmentService>();
            builder.Services.infrastructureConfigration(builder.Configuration);
            //builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
