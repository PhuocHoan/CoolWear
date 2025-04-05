using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.ViewModels;

public partial class CustomerViewModel : ViewModelBase
{
    private readonly IUnitOfWork _unitOfWork;
    public FullObservableCollection<Customer> FilteredCustomers { get; } = [];
    private string _searchTerm = "";
    public string SearchTerm
    {
        get => _searchTerm;
        set { if (SetProperty(ref _searchTerm, value)) LoadCustomersAsync(); }
    }
    public AsyncRelayCommand AddCustomerCommand { get; }
    public AsyncRelayCommand<Customer> EditCustomerCommand { get; }

    public CustomerViewModel(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
        EditCustomerCommand = new AsyncRelayCommand<Customer>(EditCustomerAsync);
    }

    public async Task LoadCustomersAsync()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        FilteredCustomers.Clear();
        foreach (var c in customers.Where(c => c.CustomerName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
            FilteredCustomers.Add(c);
    }

    private async Task AddCustomerAsync() { /* Implement */ }
    private async Task EditCustomerAsync(Customer customer) { /* Implement */ }
}