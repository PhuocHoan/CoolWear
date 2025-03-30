using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private FullObservableCollection<Product>? _products;
    private FullObservableCollection<ProductCategory>? _categories;
    private FullObservableCollection<ProductSize>? _sizes;
    private FullObservableCollection<ProductColor>? _colors;

    private ProductCategory? _selectedCategory;
    private bool _inStockOnly;
    private ProductSize? _selectedSize;
    private ProductColor? _selectedColor;

    public FullObservableCollection<Product>? Products
    {
        get => _products;
        private set => SetProperty(ref _products, value);
    }

    public FullObservableCollection<ProductCategory>? Categories
    {
        get => _categories;
        private set => SetProperty(ref _categories, value);
    }

    public FullObservableCollection<ProductSize>? Sizes
    {
        get => _sizes;
        private set => SetProperty(ref _sizes, value);
    }

    public FullObservableCollection<ProductColor>? Colors
    {
        get => _colors;
        private set => SetProperty(ref _colors, value);
    }

    public ProductCategory? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                //ApplyFilters();
            }
        }
    }

    public bool InStockOnly
    {
        get => _inStockOnly;
        set
        {
            if (SetProperty(ref _inStockOnly, value))
            {
                //ApplyFilters();
            }
        }
    }

    public ProductColor? SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (SetProperty(ref _selectedColor, value))
            {
                //ApplyFilters();
            }
        }
    }

    public ICommand ToggleSizeCommand { get; private set; }

    public ProductViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        ToggleSizeCommand = new AsyncRelayCommand<int>(ToggleSizeSelectionAsync);
    }

    private async Task ToggleSizeSelectionAsync(int sizeId)
    {
        if (_selectedSizes.ContainsKey(sizeId))
        {
            _selectedSizes[sizeId] = !_selectedSizes[sizeId];
        }
        else
        {
            _selectedSizes[sizeId] = true;
        }

        //ApplyFilters();
        await Task.CompletedTask; // Ensure method returns a Task
    }

    public bool IsSizeSelected(int sizeId) => _selectedSizes.TryGetValue(sizeId, out bool isSelected) && isSelected;

    public async Task LoadProductsAsync()
    {
        try
        {
            // Load products with related category info
            var spec = new ProductSpecification().IncludeCategory().IncludeColors().IncludeSizes();
            var products = await _unitOfWork.Products.GetAsync(spec);
            Products = [.. products];

            // Load categories
            var categories = Products.Select(p => p.Category).Distinct().ToList();
            Categories = [.. categories];

            // Load distinct colors from products
            var distinctColors = Products
                .SelectMany(p => p.ProductVariants.Select(v => v.Color))
                .Distinct()
                .OrderBy(c => c.ColorName)
                .ToList();
            Colors = [.. distinctColors];

            // Load distinct sizes from products
            var distinctSizes = Products
                .SelectMany(p => p.ProductVariants.Select(v => v.Size))
                .Distinct()
                .OrderBy(s => s.SizeName)
                .ToList();
            Sizes = [.. distinctSizes];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load products: {ex.Message}");
        }
    }

    public async Task AddProductAsync(Product product)
    {
        try
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            Products?.Add(product);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to add product: {ex.Message}");
        }
    }

    //public async Task UpdateProductAsync(Product product)
    //{
    //    try
    //    {
    //        await _unitOfWork.Products.UpdateAsync(product);
    //        await _unitOfWork.SaveChangesAsync();
    //        var existingProduct = Products?.FirstOrDefault(p => p.ProductId == product.ProductId);
    //        if (existingProduct != null)
    //        {
    //            var index = Products.IndexOf(existingProduct);
    //            Products[index] = product;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Failed to update product: {ex.Message}");
    //    }
    //}

    //public async Task DeleteProductAsync(Product product)
    //{
    //    try
    //    {
    //        await _unitOfWork.Products.DeleteAsync(product);
    //        await _unitOfWork.SaveChangesAsync();
    //        Products?.Remove(product);
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Failed to delete product: {ex.Message}");
    //    }
    //}
}
