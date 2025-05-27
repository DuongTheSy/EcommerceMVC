namespace EcommerceMVC.ViewModels
{
    public class HangHoaVM
    {
        public int MaHh { get; set; }
        public string TenHh { get; set; } = null!;
        public string? Hinh { get; set; }
        public double DonGia { get; set; }
        public string? MoTa { get; set; }
        public string TenLoai { get; set; } = null!;
    }
}
