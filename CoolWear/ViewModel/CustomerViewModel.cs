using CoolWear.Data;
using CoolWear.Model;
using CoolWear.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.ViewModel;

public class CustomerViewModel : BaseViewModel<Customer>
{
    private readonly UnitOfWork _unitOfWork;

    public CustomerViewModel(UnitOfWork unitOfWork)
        : base(unitOfWork.CustomerService, unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Order>> GetCustomerOrders(int customerId)
    {
        var orders = await _unitOfWork.OrderService.FindAsync(o => o.CustomerId == customerId);
        return orders.ToList();
    }

    // Search customers by name
    public async Task<List<Customer>> SearchCustomersByName(string name)
    {
        var customers = await _dataService.FindAsync(c => c.CustomerName.Contains(name));
        return customers.ToList();
    }
}
