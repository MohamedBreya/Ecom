namespace Ecom.core.Entities.Product
{
    public class Photo : BaseEntity<int>
    {
        /// <summary>Path relative to the web root, e.g. <c>Images/phone/3f2c.jpg</c>.</summary>
        public string ImageName { get; set; }
        public int ProductId { get; set; }
    }
}
