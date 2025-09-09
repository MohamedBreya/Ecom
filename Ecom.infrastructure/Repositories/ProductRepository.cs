using AutoMapper;
using Ecom.core.DTO;
using Ecom.core.Entities.Product;
using Ecom.core.Interfaces;
using Ecom.core.Services;
using Ecom.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.infrastructure.Repositories
{
    internal class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly AppDbContext context;
        private readonly IMapper mapper;
        private readonly IImageManagmentService imageManagmentService;
        public ProductRepository(AppDbContext context, IMapper mapper, IImageManagmentService imageManagmentService) : base(context)
        {
            this.context = context;
            this.mapper = mapper;
            this.imageManagmentService = imageManagmentService;
        }

        public async Task<bool> AddAsync(AddProductDTO addProductDTO)
        {
            if(addProductDTO is null)
            {
                return false;
            }
            var product=mapper.Map<Product>(addProductDTO);
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
            var ImagePath = await imageManagmentService.AddImageAsync(addProductDTO.Photo, addProductDTO.Name);
            var photo = ImagePath.Select(path => new Photo
            {
                ImageName = path,
                ProductId= product.Id,

            }).ToList();
            await context.AddRangeAsync(photo);
            await context.SaveChangesAsync();
            return true;
        }

        

        public async Task<bool> UpdateAsync(UpdateProductDTO updateProductDTO)
        {
            if(updateProductDTO is null)
                { return false; }
            var FindProduct = await context.Products.Include(m => m.Category)
                .Include(m=>m.Photos).FirstOrDefaultAsync(m=>m.Id == updateProductDTO.Id);
            if(FindProduct is null)
                { return false; }
            mapper.Map(updateProductDTO, FindProduct);
            var FindPhoto= await context.Photos.Where(m=>m.ProductId==updateProductDTO.Id).ToListAsync();
            foreach(var item in FindPhoto)
            {
                imageManagmentService.DeleteImageAsync(item.ImageName);
            }
            context.Photos.RemoveRange(FindPhoto);
            var ImagePath=await imageManagmentService.AddImageAsync(updateProductDTO.Photo,updateProductDTO.Name);
            var photo = ImagePath.Select(path => new Photo
            {
                ImageName = path,
                ProductId = updateProductDTO.Id
            }).ToList();

            await context.Photos.AddRangeAsync(photo);

            await context.SaveChangesAsync();
            return true;

        }

        public async Task DeleteAsync(Product product)
        {
            var photo = await context.Photos.Where(m=>m.ProductId==product.Id).ToListAsync();
            foreach (var item in photo)
            {
                imageManagmentService.DeleteImageAsync(item.ImageName);
            }
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }
}
