using CoolWear.Data;
using CoolWear.Model;
using CoolWear.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CoolWear.ViewModel;

public class ProductViewModel : BaseViewModel<Product>
{
    private readonly UnitOfWork _unitOfWork;

    public ProductViewModel(UnitOfWork unitOfWork)
        : base(unitOfWork.ProductService, unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Product>> GetProductsByCategory(int categoryId)
    {
        var products = await _dataService.FindAsync(p =>
            // Using reflection to access CategoryId property since we can't directly access it in a generic way
            (int)p.GetType().GetProperty("CategoryId").GetValue(p) == categoryId);
        return products.ToList();
    }

    public async Task<List<ProductColorLink>> GetProductColors(int productId)
    {
        var colorLinks = await _unitOfWork.ColorLinkService.FindAsync(pcl => pcl.ProductId == productId);
        return colorLinks.ToList();
    }

    public async Task<List<ProductSizeLink>> GetProductSizes(int productId)
    {
        var sizeLinks = await _unitOfWork.SizeLinkService.FindAsync(psl => psl.ProductId == productId);
        return sizeLinks.ToList();
    }

    public async Task AddProductColor(int productId, int colorId)
    {
        var link = new ProductColorLink
        {
            ProductId = productId,
            ColorId = colorId
        };

        await _unitOfWork.ColorLinkService.AddAsync(link);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddProductSize(int productId, int sizeId)
    {
        var link = new ProductSizeLink
        {
            ProductId = productId,
            SizeId = sizeId
        };

        await _unitOfWork.SizeLinkService.AddAsync(link);
        await _unitOfWork.SaveChangesAsync();
    }
}
