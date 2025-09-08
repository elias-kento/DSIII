namespace MauiAppMinhasCompras.Views;

public partial class RelatorioCategorias : ContentPage
{
    public RelatorioCategorias()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        try
        {
            var dados = await App.Db.GetTotalsByCategory();
            cv_totais.ItemsSource = dados;

            double totalGeral = dados.Sum(d => d.Total);
            lbl_total_geral.Text = $"Total geral: {totalGeral:C}";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        await CarregarAsync();
    }
}
