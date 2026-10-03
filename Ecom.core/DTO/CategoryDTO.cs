using System.ComponentModel.DataAnnotations;

namespace Ecom.core.DTO
{
    /// <summary>Request body used to create or update a category.</summary>
    public record CategoryDTO
    {
        [Required, StringLength(30)]
        public string Name { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }
    }

    public record CategoryResponseDTO(int Id, string Name, string Description);
}
