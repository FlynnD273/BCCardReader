extern alias ShimDrawing;

using Pokedex.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Utils.Command;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Pokedex.Cards;
using Pokedex.Util;
using Xamarin.Essentials;

namespace Pokedex.Model
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardCreatorPage : ContentPage
    {
        public PokemonCard Card { get; }

        private TaskCompletionSource<bool> _tcs;
        public Task<bool> WaitAsync => _tcs.Task;


        public static readonly BindableProperty CanDeleteProperty =
  BindableProperty.Create("CanDelete", typeof(bool), typeof(CardCreatorPage), null);
        public bool CanDelete
        {
            get { return (bool)GetValue(CanDeleteProperty); }
            set { SetValue(CanDeleteProperty, value); }
        }

        public List<string> CardTypes
        {
            get
            {
                return Enum.GetNames(typeof(PokemonCardType)).Select(b => b.SplitCamelCase()).ToList();
            }
        }

        public bool Delete { get; private set; }

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand TakePhotoCommand { get; }

        public CardCreatorPage(PokemonCard card, INavigation navigation, bool canDelete)
        {
            InitializeComponent();
            ConfirmCommand = new DelegateCommand(_Confirm);
            DeleteCommand = new DelegateCommand(_Delete);
            TakePhotoCommand = new DelegateCommand(_TakePhoto);

            Card = card;
            BindingContext = this;
            _tcs = new TaskCompletionSource<bool>();

            CanDelete = canDelete;
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

        private void _Delete()
        {
            Delete = true;
            _Confirm();
        }

        private async void _TakePhoto()
        {
            string newPath = await Files.SaveNewPhoto(await MediaPicker.CapturePhotoAsync());

            if (newPath == null) return;

            Card.ImagePath = newPath;
            Card.CroppedImage = null;
        }
    }
}