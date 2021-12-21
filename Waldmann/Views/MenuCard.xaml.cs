using System;
using System.Collections.Generic;
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
	public partial class MenuCard : ContentPage
	{
        public static String Cash;
        WebClient request = new WebClient();
        public int ID = Login.userID;

        public MenuCard ()
		{
            InitializeComponent ();
            FilterIcon.Source = ImageSource.FromResource("Waldmann.Images.filtericon.png");

            // pork
            porkFlesh.Source = ImageSource.FromResource("Waldmann.Images.schweinesteak_kaufen_cut.png");
            porkHeart.Source = ImageSource.FromResource("Waldmann.Images.redcircle.png");
            porkRV.Source = ImageSource.FromResource("Waldmann.Images.fourStars.png");

            //deer
            deerFlesh.Source = ImageSource.FromResource("Waldmann.Images.Hirschsteak_cutted.png");
            deerHeart.Source = ImageSource.FromResource("Waldmann.Images.redcircle.png");
            deerRV.Source = ImageSource.FromResource("Waldmann.Images.fourStars.png");


            //pork minute
            porkMinute.Source = ImageSource.FromResource("Waldmann.Images.WildschweinRuecken.png");
            porkMinuteHeart.Source = ImageSource.FromResource("Waldmann.Images.redcircle.png");
            porkMinRV.Source = ImageSource.FromResource("Waldmann.Images.fourStars.png");

            //deer steak
            deerSteak.Source = ImageSource.FromResource("Waldmann.Images.Hirschsteak_Keule.png");
            deerSteakHeart.Source = ImageSource.FromResource("Waldmann.Images.redcircle.png");
            deerSteakRV.Source = ImageSource.FromResource("Waldmann.Images.fourStars.png");

            //roe deer steak
            roeDeer.Source = ImageSource.FromResource("Waldmann.Images.Rehsteak_Keule.png");
            roeDeerHeart.Source = ImageSource.FromResource("Waldmann.Images.redcircle.png");
            roeDeerRV.Source = ImageSource.FromResource("Waldmann.Images.fourStars.png");

            // wild pork steak
            wildPork.Source = ImageSource.FromResource("Waldmann.Images.Wildschweinsteak_Keule.png");
            wildPorkHeart.Source = ImageSource.FromResource("Waldmann.Images.redcircle.png");
            wildPorkRV.Source = ImageSource.FromResource("Waldmann.Images.fourStars.png");
        }

        private async void houseTasteTapped(object sender, EventArgs args)
        {
            try
            {
                /*
                HouseTaste pyin = new HouseTaste();
                await Navigation.PushAsync(new HouseTaste());
                */
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void porkHamTasteTapped(object sender, EventArgs args)
        {
            try
            {
                /*
                PorkHamTaste pyin = new PorkHamTaste();
                await Navigation.PushAsync(new PorkHamTaste());
                */
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void chickentastejrTapped(object sender, EventArgs args)
        {
            try
            {
                /*
                ChickenTasteJr chickenTasteJr = new ChickenTasteJr();
                await Navigation.PushAsync(new ChickenTasteJr());
                */
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async void chickenTasterTapped(object sender, EventArgs args)
        {
            try
            {
                /*
                ChickenTaster chickenTaster = new ChickenTaster();
                await Navigation.PushAsync(new ChickenTaster());
                */
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}