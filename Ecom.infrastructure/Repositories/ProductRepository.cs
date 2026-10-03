using AutoMapper;
using Ecom.core.DTO;
using Ecom.core.Entities.Product;
using Ecom.core.Exceptions;
using Ecom.core.Interfaces;
using Ecom.core.Services;
using Ecom.infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Ecom.infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly IMapper _mapper;
        private readonly IImageManagementService _imageService;

        public ProductRepository(AppDbContext context, IMapper mapper, IImageManagementService imageService) : base(context)
        {
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<Product> AddAsync(AddProductDTO addProductDTO)
        {
            await EnsureCategoryExistsAsync(addProductDTO.CategoryId);

            var product = _mapper.Map<Product>(addProductDTO);
            var imagePaths = await SaveImagesAsync(addProductDTO.Photos, addProductDTO.Name);
            try
            {
                product.Photos = imagePaths.Select(path => new Photo { ImageName = path }).ToList();
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
            }
            catch
            {
                DeleteImages(imagePaths);
                throw;
            }
            return product;
        }

        public async Task UpdateAsync(int id, UpdateProductDTO updateProductDTO)
        {
            var product = await _context.Products.Include(p => p.Photos).FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new NotFoundException($"Product with id {id} was not found.");
            await EnsureCategoryExistsAsync(updateProductDTO.CategoryId);

            _mapper.Map(updateProductDTO, product);

            var newImagePaths = await SaveImagesAsync(updateProductDTO.Photos, updateProductDTO.Name);
            var oldImagePaths = new List<string>();
            if (newImagePaths.Count > 0)
            {
                oldImagePaths = product.Photos.Select(p => p.ImageName).ToList();
                _context.Photos.RemoveRange(product.Photos);
                foreach (var path in newImagePaths)
                {
                    product.Photos.Add(new Photo { ImageName = path });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                DeleteImages(newImagePaths);
                throw;
            }
            DeleteImages(oldImagePaths);
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.Include(p => p.Photos).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null)
                return false;

            var imagePaths = product.Photos.Select(p => p.ImageName).ToList();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            DeleteImages(imagePaths);
            return true;
        }

        private async Task EnsureCategoryExistsAsync(int categoryId)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
                throw new BadRequestException($"Category with id {categoryId} does not exist.");
        }

        private async Task<List<string>> SaveImagesAsync(IFormFileCollection? files, string productName)
        {
            if (files is null || files.Count == 0)
                return new List<string>();
            return await _imageService.AddImageAsync(files, productName);
        }

        private void DeleteImages(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                _imageService.DeleteImage(path);
            }
        }
    }
}
