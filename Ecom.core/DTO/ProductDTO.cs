using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ecom.core.DTO
{
    public record ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<PhotoDTO> Photos { get; set; }
    }

    public record PhotoDTO
    {
        public int Id { get; set; }

        /// <summary>Path relative to the API root, e.g. <c>Images/phone/3f2c.jpg</c>.</summary>
        public string ImageName { get; set; }
    }

    /// <summary>Multipart form used to create a product.</summary>
    public record AddProductDTO
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; }

        [Range(typeof(decimal), "0.01", "9999999999999999")]
        public decimal NewPrice { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999")]
        public decimal OldPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        /// <summary>Optional images (jpg, png, gif, webp; max 5 MB each).</summary>
        public IFormFileCollection Photos { get; set; }
    }

    /// <summary>
    /// Multipart form used to update a product. When <see cref="AddProductDTO.Photos"/> is supplied the existing
    /// photos are replaced; when it is omitted the existing photos are kept.
    /// </summary>
    public record UpdateProductDTO : AddProductDTO;
}
