using Microsoft.AspNetCore.Http;

namespace Ecom.core.Services
{
    public interface IImageManagementService
    {
        /// <summary>
        /// Validates and stores the files under <c>wwwroot/Images/{folder}</c> using generated file names.
        /// </summary>
        /// <returns>Web-root-relative paths of the stored images.</returns>
        /// <exception cref="Exceptions.BadRequestException">A file has a disallowed type or is too large.</exception>
        Task<List<string>> AddImageAsync(IFormFileCollection files, string folder);

        /// <summary>Best-effort delete of a previously stored image. Paths outside the web root are ignored.</summary>
        void DeleteImage(string src);
    }
}
