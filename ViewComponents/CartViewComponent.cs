using EcommerceMVC.Helpers;
using EcommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceMVC.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var carts = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

            return View("CartPanel",new CartModel
            {
                Quantity = carts.Sum(p => p.SoLuong),
                Total = carts.Sum(p => p.ThanhTien)
            });
        }
    }
}
