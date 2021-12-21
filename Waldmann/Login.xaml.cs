using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net;
using Xamarin.Forms;

namespace Waldmann
{
    public partial class Login : ContentPage
    {
        public static String Username, Password;
        public static int userID;
        
        public Login()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            WMLogo.Source = ImageSource.FromResource("Waldmann.Images.WaldmannLogo.png");
        }



        private async void login_click(object Sender, EventArgs e)
        {
      
            MainPage mainpage = new MainPage();
            await Navigation.PushAsync(new MainPage());

        }
        private async void registerHere_click(object Sender, EventArgs e)
        {
            await Navigation.PushAsync(new Register());
        }

    }
}
