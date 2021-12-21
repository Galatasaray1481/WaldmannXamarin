using BottomBar.XamarinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Waldmann
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BottomNavBar : BottomBarPage
    {
        public BottomNavBar()
        {
            InitializeComponent();
            FilterIcon.Source = ImageSource.FromResource("Waldmann.Images.filtericon_cut.png");
        }
    }
}