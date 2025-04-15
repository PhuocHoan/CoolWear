using System;
using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.ViewModels;

public class SellViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DispatcherQueue _dispatcherQueue;

    private ObservableCollection<Product>? _filteredProducts;
    private ObservableCollection<ProductCategory>? _categories;
    private ProductCategory? _selectedCategory;
    private string? _searchTerm;
    private bool _isLoading;

    private ObservableCollection<Product>? _selectedProducts = new();
    private Product? _selectedProduct;
    public bool IsProductSelected => SelectedProduct != null;



    public ObservableCollection<Product>? FilteredProducts
    {
        get => _filteredProducts;
        private set => SetProperty(ref _filteredProducts, value);
    }

    public ObservableCollection<ProductCategory>? Categories
    {
        get => _categories;
        private set => SetProperty(ref _categories, value);
    }

    public ProductCategory? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public string? SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public ObservableCollection<Product>? SelectedProducts
    {
        get => _selectedProducts;
        private set => SetProperty(ref _selectedProducts, value);
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public SellViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        FilteredProducts = new();
        Categories = new();


        PropertyChanged += OnPropertyChanged;
    }

    protected async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsLoading) return;

        if (e.PropertyName == nameof(SelectedCategory) || e.PropertyName == nameof(SearchTerm))
        {
            await LoadProductsAsync();
        }
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        IsLoading = true;

        try
        {
            var categoryList = await _unitOfWork.ProductCategories.GetAllAsync();
            var sorted = categoryList.OrderBy(c => c.CategoryName).ToList();

            Debug.WriteLine($"Categories loaded: {string.Join(", ", sorted.Select(c => c.CategoryName))}");

            _dispatcherQueue.TryEnqueue(() =>
            {
                Categories?.Clear();
                foreach (var category in sorted)
                {
                    Categories?.Add(category);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading categories: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadProductsAsync()
    {
        IsLoading = true;

        try
        {
            var spec = new ProductSpecification(
                searchTerm: SearchTerm,
                categoryId: SelectedCategory?.CategoryId,
                includeDetails: true
            );

            var products = await _unitOfWork.Products.GetAsync(spec);

            _dispatcherQueue.TryEnqueue(() =>
            {
                FilteredProducts?.Clear();
                foreach (var product in products)
                {
                    FilteredProducts?.Add(product);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading products: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

}
