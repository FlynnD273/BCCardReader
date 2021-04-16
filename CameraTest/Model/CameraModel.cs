using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Utils.Command;
using Utils.Model;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CameraTest.Model
{
    class CameraModel : ViewModelBase
    {
        private readonly string workingPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string dbPath;

        private DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ObservableCollection<PlayingCard>));

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

            dbPath = Path.Combine(workingPath, "cardsdb.xml");

            //Read database
            if (File.Exists(dbPath))
            {
                string s = File.ReadAllText(dbPath);
                Cards = (ObservableCollection<PlayingCard>)xmlSerializer.ReadObject(new XmlTextReader(dbPath));
            }
            else
            {
                Cards = new ObservableCollection<PlayingCard>();
            }
        }

        private async void _CreateCard()
        {
            //await DisplayAlert("Alert", "Taking image!", "OK");
            string path = Path.Combine(workingPath, "img", Guid.NewGuid().ToString() + ".jpg");
            try
            {
                await _SavePhotoToFile(await MediaPicker.CapturePhotoAsync(), path);
                //await DisplayAlert("Alert", "Image taken!", "OK");
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "OK");
                return;
            }

            var creatorPage = new CardCreatorPage(new PlayingCard(CardType.Normal, path, ""), Navigation);

            await Navigation.PushModalAsync(creatorPage);


            if (await creatorPage.WaitAsync)
            {
                Cards.Add(creatorPage.Card);

                //File.Delete(dbPath);
                using (var fs = new FileStream(dbPath, FileMode.Create))
                {
                    xmlSerializer.WriteObject(fs, Cards);
                }
            }
        }

        //private async Task<MediaFile> _TakeImage()
        //{
        //    return await MediaPicker.CapturePhotoAsync();
        //    //return f;
        //}

        private async Task _SavePhotoToFile (FileResult photo, string path)
        {
            // canceled
            if (photo == null)
            {
                return;
            }
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            // save the file into local storage
            var newFile = path;
            using (var stream = await photo.OpenReadAsync())
            using (var newStream = File.OpenWrite(newFile))
                await stream.CopyToAsync(newStream);

            //File.Copy(path, Path.ChangeExtension(path, "thumb"));

            //var img = System.Drawing.Image.FromFile(path);
            //img.GetThumbnailImage(120, 120, null, IntPtr.Zero).Save(Path.ChangeExtension(path, "thumb"));
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
