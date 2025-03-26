using CoolWear.Data;
using CoolWear.Model;
using CoolWear.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CoolWear.ViewModel;

/// <summary>
/// Base generic ViewModel that provides common CRUD operations for entities
/// </summary>
/// <typeparam name="T">Entity type that implements INotifyPropertyChanged</typeparam>
public class BaseViewModel<T> where T : class, INotifyPropertyChanged
{
    public FullObservableCollection<T> Items { get; set; }
    protected readonly IDataService<T> _dataService;
    protected readonly UnitOfWork _unitOfWork;

    public BaseViewModel(IDataService<T> dataService, UnitOfWork unitOfWork)
    {
        _dataService = dataService;
        _unitOfWork = unitOfWork;
        InitializeCollection();
    }

    private async void InitializeCollection()
    {
        var items = await _dataService.GetAllAsync();
        Items = new FullObservableCollection<T>(items);
    }

    public virtual async Task Add(T entity)
    {
        await _dataService.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        Items.Add(entity);
    }

    public virtual async Task Remove(T entity)
    {
        await _dataService.DeleteAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        Items.Remove(entity);
    }

    public virtual async Task<T?> FindById(int id)
    {
        return await _dataService.GetByIdAsync(id);
    }

    public virtual async Task Update(T entity)
    {
        await _dataService.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
