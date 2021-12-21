using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waldmann.MenuItems;
using Xamarin.Forms;
using Waldmann.Views;

namespace Waldmann
{
    public partial class MainPage : MasterDetailPage
    {
        public List<MasterPageItem> menuList { get; set; }

        public MainPage()
        {

            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
            menuList = new List<MasterPageItem>();

            // Bauen hier unsere Elemente für die Navigationsbar
            // Hier definieren wir den Titel, icon und die Seite welche wir öffnen wollen
            var page1 = new MasterPageItem() { Title = "Wildbretkauf", Icon = "itemIcon1.png", TargetType = typeof(MenuCard) };
            var page2 = new MasterPageItem() { Title = "Begehungsscheine kaufen", Icon = "itemIcon3.png", TargetType = typeof(PayIn) };
            var page3 = new MasterPageItem() { Title = "Events u. Schießstände", Icon = "itemIcon5.png", TargetType = typeof(OrderOverview) };
            var page4 = new MasterPageItem() { Title = "Profil bearbeiten", Icon = "itemIcon2.png", TargetType = typeof(EditProfile) };
            var page5 = new MasterPageItem() { Title = "Jäger finden", Icon = "itemIcon5.png", TargetType = typeof(OrderOverview) };
            var page6 = new MasterPageItem() { Title = "Chats", Icon = "itemIcon6.png", TargetType = typeof(PrivateChat) };
            var page7 = new MasterPageItem() { Title = "Jagdrecht einsehen", Icon = "itemIcon7.png", TargetType = typeof(AboutUs) };
            var page8 = new MasterPageItem() { Title = "Warenkorb", Icon = "itemIcon4.png", TargetType = typeof(Cart) };

            // hier addieren wir unsere Elemente zur menuList
            menuList.Add(page1);
            menuList.Add(page2);
            menuList.Add(page3);
            menuList.Add(page4);
            menuList.Add(page5);
            menuList.Add(page6);
            menuList.Add(page7);
            menuList.Add(page8);

            // Setzen unsere menuList als ItemSource für ListView in MainPage.xaml
            navigationDrawerList.ItemsSource = menuList;

            // Initiale navigation, Startseitenaufruf
            Detail = new NavigationPage((Page)Activator.CreateInstance(typeof(BottomNavBar)));
        }

        // Event welche die Auswahl unserer Elemente und deren Seitenaufrufe managed
        private void OnMenuItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

            var item = (MasterPageItem)e.SelectedItem;
            Type page = item.TargetType;

            Detail = new NavigationPage((Page)Activator.CreateInstance(page));
            IsPresented = false;
        }
    }
}
