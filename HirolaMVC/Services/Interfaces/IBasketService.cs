using HirolaMVC.ViewModels;

namespace HirolaMVC.Services.Interfaces
{
    public interface IBasketService
    {
        Task<List<BasketItemVM>> GetBasketAsync();
    }
}
