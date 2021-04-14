using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Command;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CameraTest.Model
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardCreatorPage : ContentPage
    {
        public PlayingCard Card { get; }

        public DelegateCommand ConfirmCommand { get; }

        public CardCreatorPage(PlayingCard card)
        {
            InitializeComponent();
            ConfirmCommand = new DelegateCommand(_Confirm);
            Card = card;
        }

        private async void _Confirm()
        {
            await Navigation.PopModalAsync();
        }
    }
}