using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Waldmann
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PrivateChat : ContentPage
    {
        public List<String> name = new List<string>();
        public List<String> msgContentL = new List<string>();
        public int i = 0;
        public int countmsg = 0;

        public PrivateChat()
        {
            InitializeComponent();
            LoadData();
        }


        public void LoadData()
        {
            List<UserMsgInfo> item2 = new List<UserMsgInfo>();
            UserMsgInfo msg1 = new UserMsgInfo(1, "Jana", "Jägerin", 1, "Hallo", 2);
            UserMsgInfo msg2 = new UserMsgInfo(1, "Flora", "Wildbretkäuferin", 1, "Guten Tag", 2);
            item2.Add(msg1);
            item2.Add(msg2);
            item2.ForEach((item) => name.Add(item.name + " " + item.nachname));
            item2.ForEach((item) => msgContentL.Add(item.msgContent));
            ListView1.ItemsSource = item2;
        }

        private void sendMsg_click(object Sender, EventArgs e)
        {

        }
    }
}