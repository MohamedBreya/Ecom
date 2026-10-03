using Ecom.API.Extensions;
using Ecom.API.Middleware;
using Ecom.infrastructure;

namespace Ecom.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApiBehavior();
            builder.Services.AddApiRateLimiting(builder.Configuration);
            builder.Services.AddFrontendCors(builder.Configuration);

            var app = builder.Build();

            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStatusCodePagesWithReExecute("/errors/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors(ApiServiceCollectionExtensions.CorsPolicyName);
            app.UseRateLimiter();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
