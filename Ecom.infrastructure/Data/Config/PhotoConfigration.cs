using Ecom.core.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.infrastructure.Data.Config
{
    public class PhotoConfigration : IEntityTypeConfiguration<Photo>
    {
        public void Configure(EntityTypeBuilder<Photo> builder)
        {
            builder.HasData(new Photo
            {
                Id = 5,
                ImageName = "Photo Test",
                ProductId = 1,
            });
        }
    }
}
