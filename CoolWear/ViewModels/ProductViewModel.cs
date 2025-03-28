using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoolWear.ViewModels;

public class ProductViewModel(IUnitOfWork unitOfWork)
{
    public FullObservableCollection<Product>? Products { get; private set; }
    public ICommand? LoadCommand { get; private set; }
    public ICommand? DeleteCommand { get; private set; }

    // Call this method explicitly (e.g., via a Command)
    public async Task LoadProductsAsync()
    {
        try
        {
            var products = await unitOfWork.Products.GetAllAsync();
            Products = [.. products];
            LoadCommand = new AsyncRelayCommand(LoadProductsAsync);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load products: {ex.Message}");
        }
    }

    private void DeleteProduct()
    {
        // Delete logic
    }

    private bool CanDeleteProduct() => true; // Logic to enable/disable button
}