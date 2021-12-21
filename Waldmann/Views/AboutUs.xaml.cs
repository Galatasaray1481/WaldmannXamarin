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
	public partial class AboutUs : ContentPage
	{
        public static String Cash;
        public AboutUs ()
		{
            InitializeComponent ();
            LoadCurrentData();
            BTLogo.Source = ImageSource.FromResource("BurgerTasters.Images.BurgerTastersLogo.png");
        }
        public void LoadCurrentData()
        {
            NameValueCollection currdatainfo = new NameValueCollection();
            // string url = "https://realburgertasters.000webhostapp.com/ShowCurrData.php/?user_ID=" + ID;
            xCash.Text = Cash;
        }
    }
}