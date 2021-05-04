extern alias ShimDrawing;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Utils.Command;
using Xamarin.Essentials;
using Xamarin.Forms;
using Pokedex.Cards;
using Pokedex.Util;

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

            //if (Cards.Count > 0)
            //{
            //    PokemonCard c = Cards[0];
            //    Cards.Clear();

            //    for (int i = 0; i < 11; i++)
            //    {
            //        Cards.Add(new PokemonCard((CardType)i, c.ImagePath, i.ToString()));
            //    }

            //    Cards.Remove(c);

            //    _UpdateDatabase();
            //}
        }

        private async void _DbError()
        {
            await DisplayAlert("Error", "There was an error reading the database. If you add a card, all previous card data will be lost.", "OK");
            Cards = new ObservableCollection<PokemonCard>();
        }

        private async void _CreateCard()
        {
            //Take photo and save it locally
            string path = null;
            try
            {
                path = await Files.SaveNewPhoto(await MediaPicker.CapturePhotoAsync());
            }
            catch (IOException e)
            {
                DisplayAlert("ERROR", e.Message, "OK");
            }

            if (path == null)
            {
                return;
            }

            //Edit the card
            var creatorPage = new CardCreatorPage(new PokemonCard(PokemonCardType.Colorless, path, ""), Navigation, canDelete: false);

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
            PokemonCard old = (PokemonCard)card.Clone();
            //Edit the card
            var creatorPage = new CardCreatorPage(card, Navigation, canDelete: true);

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
    }
}
