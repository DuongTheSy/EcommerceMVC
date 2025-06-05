using EcommerceMVC.Data;
using EcommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using EcommerceMVC.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace EcommerceMVC.Controllers
{
    public class CartController : Controller
    {
		private readonly PaypalClient _paypalClient;
		private readonly Hshop2023Context db;
        public CartController(Hshop2023Context context, PaypalClient paypalClient)
        {
            _paypalClient = paypalClient;
            db = context;
        }
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

        public IActionResult Index()
        {
            return View(Cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = "Không tìm thấy sản phẩm này!";
                    return Redirect("/404");
                }
                item = new CartItem()
                {
                    MaHh = hangHoa.MaHh,
                    TenHh = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }
            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            return RedirectToAction("Index");
        }
        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var gioHang = Cart;
            if ((gioHang.Count == 0))
            {
                return Redirect("/");
            }
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(Cart);
        }

		[Authorize]
        [HttpPost]
		public IActionResult Checkout(CheckoutVM model)
		{
			if (ModelState.IsValid)
			{
				var customerID = HttpContext.User.Claims.SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID).Value;
                var khachHang = new KhachHang();
                if (model.GiongKhachHang)
                {
                    khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerID);
                }

                var hoaDon = new HoaDon
                {
					MaKh = customerID,
					HoTen = model.HoTen ?? khachHang.HoTen,
					DiaChi = model.DiaChi ?? khachHang.DiaChi,
					DienThoai = model.DienThoai ?? khachHang.DienThoai,
					NgayDat = DateTime.Now,
					CachThanhToan = "COD",
					CachVanChuyen = "GRAB",
					MaTrangThai = 0,
					GhiChu = model.GhiChu
				};

                db.Database.BeginTransaction();
                try
                {
                    db.Database.CommitTransaction();
                    db.Add(hoaDon);
                    db.SaveChanges();
					var cthds = new List<ChiTietHd>();
                    foreach(var item in Cart)
                    {
                        cthds.Add(new ChiTietHd
						{
							MaHd = hoaDon.MaHd,
							MaHh = item.MaHh,
							SoLuong = item.SoLuong,
							DonGia = item.DonGia,
                            GiamGia = 0,

						});
                    }
                    db.AddRange(cthds);
                    db.SaveChanges();
					HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());
                    return View("Success");
				}
                catch
                {
                    db.Database.RollbackTransaction();
                }



			}
			return View(Cart);
		}
	}
}
