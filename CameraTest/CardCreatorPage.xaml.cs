using CameraTest.Extension;
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

        private TaskCompletionSource<bool> _tcs;
        public Task<bool> WaitAsync => _tcs.Task;

        public DelegateCommand ConfirmCommand { get; }

        public List<string> CardTypes
        {
            get
            {
                return Enum.GetNames(typeof(CardType)).Select(b => b.SplitCamelCase()).ToList();
            }
        }

        public CardCreatorPage(PlayingCard card, INavigation navigation)
        {
            InitializeComponent();
            ConfirmCommand = new DelegateCommand(_Confirm);
            Card = card;
            BindingContext = this;
            _tcs = new TaskCompletionSource<bool>();
        }

        private async void _Confirm()
        {
            //await Shell.Current.GoToAsync("..");
            _tcs.SetResult(true);
            await Navigation.PopModalAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            _tcs.SetResult(false);
            return base.OnBackButtonPressed();
        }
    }
}