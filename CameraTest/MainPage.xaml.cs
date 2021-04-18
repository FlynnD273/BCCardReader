using CameraTest.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CameraTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new CameraModel(Navigation, this);
        }
        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((CameraModel)BindingContext).EditCard((PokemonCard)e.Item);
        }
    }
}
