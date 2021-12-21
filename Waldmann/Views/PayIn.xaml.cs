using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Waldmann.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PayIn : ContentPage
	{
        public int ID = Login.userID;
        WebClient request = new WebClient();
        public static String Cash;

        string patternmoney = "^[0-9]+(.[0-9]{1,2})?$";

        public PayIn ()
		{
			InitializeComponent ();
            LoadCurrentData();
            BTPayIn.Source = ImageSource.FromResource("BurgerTasters.Images.circle_euro.png");
        }

        private async void payIn_click(object Sender, EventArgs e)
        {
            await Navigation.PushAsync(new PayIn());
        }
        public void LoadCurrentData()
        {
            xCash.Text = Cash;
        }
    }
}