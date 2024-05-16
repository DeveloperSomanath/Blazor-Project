using Tailor.Domain.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Tailor.Domain.ViewModels
{
#pragma warning disable CS8618
    public class ShoppingCartViewModel
    {
        [ValidateNever]
        public IEnumerable<ShoppingCart> ShoppingCarts { get; set; }

        [ValidateNever]
        public OrderHeader OrderHeader { get; set; }


    }
}
