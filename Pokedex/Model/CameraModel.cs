using Pokedex.Converter;
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

namespace Pokedex.Model
{
    class CameraModel : ViewModelBase
    {
        private readonly string workingPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string dbPath;

        private DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ObservableCollection<PokemonCard>));

        public ObservableCollection<PokemonCard> Cards { get; private set; } = new ObservableCollection<PokemonCard>();

        public DelegateCommand TakeImageCommand { get; }
        public DelegateCommand PickImageCommand { get; }
        public DelegateCommand CreateCardCommand { get; }

        public CameraModel(INavigation navigation, Page page) : base(navigation, page)
        {
            CreateCardCommand = new DelegateCommand(_CreateCard);

            dbPath = Path.Combine(workingPath, "cardsdb.xml");

            //Read database
            if (File.Exists(dbPath))
            {
                try
                {
                    Cards = (ObservableCollection<PokemonCard>)xmlSerializer.ReadObject(new XmlTextReader(dbPath));
                }
                catch (SerializationException e)
                {
                    _DbError();
                }
            }
            else
            {
                Cards = new ObservableCollection<PokemonCard>();
            }

            if (Cards.Count > 0)
            {
                PokemonCard c = Cards[0];
                Cards.Clear();

                for (int i = 0; i < 11; i++)
                {
                    Cards.Add(new PokemonCard((CardType)i, c.ImagePath, i.ToString()));
                }

                Cards.Remove(c);

                _UpdateDatabase();
            }
        }

        private async void _DbError()
        {
            await DisplayAlert("Error", "There was an error reading the database. If you add a card, all previous card data will be lost.", "OK");
            Cards = new ObservableCollection<PokemonCard>();
        }

        private async void _CreateCard()
        {
            //Take photo and save it locally
            string path = await _SaveNewPhoto(await MediaPicker.CapturePhotoAsync());

            //Edit the card
            var creatorPage = new CardCreatorPage(new PokemonCard(CardType.Colorless, path, ""), Navigation);

            await Navigation.PushModalAsync(creatorPage);


            if (await creatorPage.WaitAsync)
            {
                //Add the edited card
                Cards.Add(creatorPage.Card);

                _UpdateDatabase();
            }
        }

        public async void EditCard(PokemonCard card)
        {
            PokemonCard old = card.Clone();
            //Edit the card
            var creatorPage = new CardCreatorPage(card, Navigation) { CanDelete = true };

            await Navigation.PushModalAsync(creatorPage);


            if (await creatorPage.WaitAsync)
            {
                if (creatorPage.Delete)
                {
                    Cards.Remove(card);
                }

                _UpdateDatabase();
            }
            else
            {
                Cards[Cards.IndexOf(card)] = old;
            }
        }

        private void _UpdateDatabase()
        {
            //Update XML file
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }

            using (var fs = new FileStream(dbPath, FileMode.Create))
            {
                xmlSerializer.WriteObject(fs, Cards);
            }
        }

        private async Task<string> _SaveNewPhoto(FileResult photo)
        {
            try
            {
                //Save in {App Directory}\img\{GUID}.jpg
                string path = Path.Combine(workingPath, "img", Guid.NewGuid().ToString() + ".jpg");

                //Canceled
                if (photo == null)
                {
                    return null;
                }

                //Create img folder if needed
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                //Save the file to folder
                var newFile = path;
                using (var stream = await photo.OpenReadAsync())
                using (var newStream = File.OpenWrite(newFile))
                    await stream.CopyToAsync(newStream);

                return path;
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "OK");
                return null;
            }
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
