using AutoMapper;
using Ecom.core.Interfaces;
using Ecom.core.Services;
using Ecom.infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageManagmentService _imageManagmentService;
        public ICategoryRepository CategoryRepository { get; }
        public IPhotoRepository PhotoRepository { get; }
        public IProductRepository ProductRepository { get; }
        public UnitOfWork(AppDbContext context, IMapper mapper, IImageManagmentService imageManagmentService)
        {
            _context = context;
            CategoryRepository = new CategoryRepository(_context);
            PhotoRepository = new PhotoRepository(_context);
            _mapper = mapper;
            _imageManagmentService = imageManagmentService;
            ProductRepository = new ProductRepository(_context, _mapper, _imageManagmentService);

        }

    }
}
