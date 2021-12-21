using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Waldmann.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Cart : ContentPage
    {

        public int ID = Login.userID;
        public int i = 0;
        public int PID = 0;
        public string xyz;
        public List <String> orderName = new List<string>();
        public List<String> cost = new List<string>();
        WebClient request = new WebClient();
        public static String Cash;


        public Cart ()
		{
			InitializeComponent ();
            LoadCurrentData();
            LoadData();
           
        }
        public void LoadCurrentData()
        {

            xCash.Text = Cash;
        }

        public void LoadData()
        {        

            

        }

        private async void payOrder_click(object Sender, EventArgs e)
        {

            Cart edit = new Cart();
                await DisplayAlert("Bestellung", "Vielen Dank für ihre Bestellung.", "OK");
                await Navigation.PushAsync(new Cart());
        }

        public void ListView1_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }


        private async void cancelOrder_click(object Sender, EventArgs e)
        {
            Cart edit = new Cart();
            await Navigation.PushAsync(new Cart());
        }

    }
}