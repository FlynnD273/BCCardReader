using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Utils.Command;
using Utils.Model;
using Xamarin.Forms;

namespace CameraTest.Model
{
    class CameraModel : ViewModelBase
    {
        private readonly string dbPath = "cardsdb.xml";
        private DataSet _cardData;
        public DataSet CardData
        {
            get { return _cardData; }
            set { _UpdateField(ref _cardData, value); }
        }

        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlayingCard[]));

        public ObservableCollection<PlayingCard> Cards { get; private set; } = new ObservableCollection<PlayingCard>();

        //private ImageSource _cameraImage;
        //public ImageSource CameraImage
        //{
        //    get { return _cameraImage; }
        //    set { _UpdateField(ref _cameraImage, value); }
        //}

        //private ImageSource _displayImage;
        //public ImageSource DisplayImage
        //{
        //    get { return _displayImage; }
        //    set { _UpdateField(ref _displayImage, value); }
        //}


        public DelegateCommand TakeImageCommand { get; }
        public DelegateCommand PickImageCommand { get; }
        public DelegateCommand CreateCardCommand { get; }

        public CameraModel(INavigation navigation, Page page) : base(navigation, page)
        {
            //TakeImageCommand = new DelegateCommand(_TakeImage);
            //PickImageCommand = new DelegateCommand(_PickImage);
            CreateCardCommand = new DelegateCommand(_CreateCard);

            Cards = new ObservableCollection<PlayingCard>();

            if (File.Exists(dbPath))
            {
                foreach (PlayingCard c in (PlayingCard[])xmlSerializer.Deserialize(new StreamReader(dbPath)))
                {
                    Cards.Add(c);
                }
            }
        }

        private async void _CreateCard()
        {
            await DisplayAlert("Alert", "Taking image!", "OK");

            MediaFile photo = null;
            try
            {
                photo = await _TakeImage();
                await DisplayAlert("Alert", "Image taken!", "OK");
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "OK");
            }

            if (photo == null)
            {
                await DisplayAlert("Error", "Photo was null", "OK");
                return;
            }

            string path = "img/" + Guid.NewGuid().ToString();
            File.Move(photo.Path, path);
            var creatorPage = new CardCreatorPage(new PlayingCard(CardType.Normal, path, ""));

            await Navigation.PushModalAsync(creatorPage);

            Cards.Add(creatorPage.Card);
            xmlSerializer.Serialize(new StreamWriter(dbPath), Cards);
        }

        private async Task<MediaFile> _TakeImage()
        {
            MediaFile f = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
            {
                DefaultCamera = CameraDevice.Rear,
                SaveToAlbum = true
            });
            await DisplayAlert("Alert", "Image taken!", "OK");
            return f;
        }

        //private async void _PickImage()
        //{
        //    if (!CrossMedia.Current.IsPickPhotoSupported)
        //    {
        //        //await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
        //        return;
        //    }
        //    var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
        //    {
        //        PhotoSize = PhotoSize.Medium
        //    });


        //    if (file == null)
        //        return;

        //    CameraImage = ImageSource.FromStream(() =>
        //    {
        //        var stream = file.GetStream();
        //        file.Dispose();
        //        return stream;
        //    });
        //}
    }
}
