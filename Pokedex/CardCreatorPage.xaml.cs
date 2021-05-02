extern alias ShimDrawing;

using Pokedex.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using ShimDrawing::System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Utils.Command;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Image = ShimDrawing::System.Drawing.Image;

namespace Pokedex.Model
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardCreatorPage : ContentPage, INotifyPropertyChanged
    {
        //private Stopwatch refresh = new Stopwatch();
        public PokemonCard Card { get; }

        private TaskCompletionSource<bool> _tcs;
        public Task<bool> WaitAsync => _tcs.Task;

        private bool _canDelete;
        public bool CanDelete
        {
            get { return _canDelete; }
            set { _UpdateField(ref _canDelete, value); }
        }

        private void _CropImage()
        {
            using (Bitmap img = (Bitmap)Image.FromStream(File.OpenRead(Card.ImagePath)))
            using (Bitmap cropped = ImageProcessor.FindPlayingCard(img))
            {
                if (cropped != null)
                {
                    using (Stream writer = File.OpenWrite(Path.ChangeExtension(Card.ImagePath, "card")))
                    {
                        cropped.Save(writer, ShimDrawing::System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    Card.CroppedImage = null;
                }
            }
        }

        public List<string> CardTypes
        {
            get
            {
                return Enum.GetNames(typeof(CardType)).Select(b => b.SplitCamelCase()).ToList();
            }
        }

        public bool Delete { get; private set; }

        public DelegateCommand ConfirmCommand { get; }
        public DelegateCommand GreenscreenImageCommand { get; }
        public DelegateCommand CropImageCommand { get; }


        public DelegateCommand DeleteCommand { get; }


        public CardCreatorPage(PokemonCard card, INavigation navigation)
        {
            InitializeComponent();
            ConfirmCommand = new DelegateCommand(_Confirm);
            DeleteCommand = new DelegateCommand(_Delete);
            GreenscreenImageCommand = new DelegateCommand(_GreenscreenImage);
            CropImageCommand = new DelegateCommand(_CropImage);

            Card = card;
            BindingContext = this;
            _tcs = new TaskCompletionSource<bool>();
            DisplayImage = ImageSource.FromFile(Path.ChangeExtension(Card.ImagePath, "card"));
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void _UpdateField<T>(ref T field, T newValue,
            Action<T> onChangedCallback = null,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return;
            }

            T oldValue = field;

            field = newValue;
            onChangedCallback?.Invoke(oldValue);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}