using CoolWear.Services;
using CoolWear.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace CoolWear.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ProductsPage : Page
{
    // Public property so x:Bind can see it
    public ProductViewModel ViewModel { get; } = ServiceManager.GetKeyedSingleton<ProductViewModel>();

    public ProductsPage()
    {
        InitializeComponent();

        // Optionally, set DataContext (if you want normal {Binding} usage)
        DataContext = ViewModel;

        // Load data after the page is constructed
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e) => await ViewModel.LoadProductsAsync();
}
