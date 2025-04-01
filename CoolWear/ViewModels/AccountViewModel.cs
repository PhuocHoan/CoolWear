using CoolWear.Models;
using CoolWear.Services;
using CoolWear.Utilities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using System.Diagnostics;

namespace CoolWear.ViewModels
{
    public partial class AccountViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private StoreOwner? _storeOwner;

        // Add backing fields for properties
        private string _ownerName = "";
        private string _email = "";
        private string _phone = "";
        private string _address = "";

        // Use properties with SetProperty for change notification
        public string OwnerName
        {
            get => _ownerName;
            set => SetProperty(ref _ownerName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public AccountViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            // Load StoreOwner data on ViewModel initialization
            GetStoreOwnerAsync(unitOfWork);
        }

        private async Task GetStoreOwnerAsync(IUnitOfWork unitOfWork)
        {
            var owners = await unitOfWork.StoreOwners.GetAllAsync();
            if (owners.Any())
            {
                _storeOwner = owners.First(); // Assuming there is only one store owner
                Debug.WriteLine($"Store owner: {_storeOwner.OwnerName}");

                // Set properties from StoreOwner data
                OwnerName = _storeOwner.OwnerName ?? string.Empty;
                Email = _storeOwner.Email ?? string.Empty;
                Phone = _storeOwner.Phone ?? string.Empty;
                Address = _storeOwner.Address ?? string.Empty;
            }
            else
            {
                Debug.WriteLine("No store owners found");
            }
        }
        public async Task LoadAccountInfoAsync()
        {
            await GetStoreOwnerAsync(_unitOfWork);
        }

        // Method to handle possible login validation (if needed for your case)
        public bool CanLogin() => !string.IsNullOrEmpty(OwnerName) && !string.IsNullOrEmpty(Email);
    }
}
