using Ecom.core.Interfaces;
using Ecom.core.Services;
using Ecom.infrastructure.Data;
using Ecom.infrastructure.Repositories;
using Ecom.infrastructure.Repositories.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.infrastructure
{
    public static class InfrastructureRegisteration
    {
        public static IServiceCollection infrastructureConfigration(this IServiceCollection services, IConfiguration configration)
        {
            services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
            //services.AddScoped<ICategoryRepository,CategoryRepository>();
            //services.AddScoped<IProductRepository,ProductRepository>();
            //services.AddScoped<IPhotoRepository,PhotoRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IImageManagmentService, ImageManagmentService>();
            // Register IFileProvider for ImageManagmentService
            //services.AddSingleton<IFileProvider>(provider =>
            //{


            //    // Fallback for non-web applications or design-time
            //    var contentRoot = Directory.GetCurrentDirectory();
            //    var wwwrootPath = Path.Combine(contentRoot, "wwwroot");

            //    if (!Directory.Exists(wwwrootPath))
            //    {
            //        Directory.CreateDirectory(wwwrootPath);
            //    }

            //    return new PhysicalFileProvider(wwwrootPath);
            //});
            services.AddSingleton<IFileProvider>(
                new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),"wwwroot"))
                );
            services.AddDbContext<AppDbContext>( op =>
            {
                op.UseSqlServer(configration.GetConnectionString("Ecom"));
            });

            return services;
        }
    }
}
