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
	public partial class OrderOverview : ContentPage
	{
        public int ID = Login.userID;
        public int PID = 1;
        public List<String> orderName = new List<string>();
        public List<String> cost = new List<string>();
        WebClient request = new WebClient();
        public static String Cash;

        public OrderOverview ()
		{
			InitializeComponent ();
            LoadData();
            LoadCurrentData();
        }

        public void LoadCurrentData()
        {

            xCash.Text = Cash;
        }

        public void LoadData()
        {

        }
    }
}