using Tailor.Domain.Entities;

namespace Tailor.Domain.ViewModels
{
    public class OrderViewModel
    {
        public OrderHeader OrderHeader { get; set; } = new();
        public IEnumerable<OrderDetail>? OrderDetail { get; set; }
    }
}
