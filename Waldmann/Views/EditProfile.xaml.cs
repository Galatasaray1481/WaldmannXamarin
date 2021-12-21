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
    public partial class EditProfile : ContentPage
    {



        public EditProfile ()
        {
            InitializeComponent();
            LoadCurrentData();
            BTEditProfile.Source = ImageSource.FromResource("BurgerTasters.Images.circle_profile.png");
        }

        public void LoadCurrentData()
        {

            xCash.Text = "0.00";
        }

        private async void aender_click(object Sender, EventArgs e)
        {
            EditProfile edit = new EditProfile();
            await Navigation.PushAsync(new EditProfile());
        }

        private async void loeschen_click(object Sender, EventArgs e)
        {
            await Navigation.PushAsync(new Login());
        }
    }
}