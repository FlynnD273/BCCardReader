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
using System.Linq;

namespace Pokedex.Model
{
    class CameraModel : ViewModelBase
    {
        private readonly string workingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private readonly string dbPath;

        private DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ObservableCollection<PokemonCard>));

        private ObservableCollection<PokemonCard> _cards { get; } = new ObservableCollection<PokemonCard>();

        private Func<PokemonCard, string> _sort;
        public Func<PokemonCard, string> Sort
        {
            get { return _sort; }
            set { _UpdateField(ref _sort, value, o => _UpdateCards()); }
        }

        private void _UpdateCards()
        {
            if (_cards.Count > 0)
            {
                SortedCards = _cards.OrderBy(Sort).ToArray();
            }
            else
            {
                SortedCards = new PokemonCard[0];
            }
        }

        private PokemonCard[] _sortedCards;
        public PokemonCard[] SortedCards
        {
            get => _sortedCards;
            set { _UpdateField(ref _sortedCards, value); }
        }

        public DelegateCommand TakeImageCommand { get; }
        public DelegateCommand PickImageCommand { get; }
        public DelegateCommand CreateCardCommand { get; }

        public CameraModel(INavigation navigation, Page page) : base(navigation, page)
        {
            CreateCardCommand = new DelegateCommand(_CreateCard);

            //_cards.CollectionChanged += (o, a) => _UpdateCards();
            Sort = o => o.Name;

            dbPath = Path.Combine(workingPath, "cardsdb.xml");

            //Read database
            if (File.Exists(dbPath))
            {
                try
                {
                    _cards = (ObservableCollection<PokemonCard>)xmlSerializer.ReadObject(new XmlTextReader(dbPath));
                    _UpdateCards();
                }
                catch (SerializationException e)
                {
                    _DbError();
                }
            }
            else
            {
                _cards.Clear();
                _UpdateDatabase();
            }

            //if (Cards.Count > 0)
            //{
            //    PlayingCardBase c = Cards[0];
            //    Cards.Clear();

            //    for (int i = 0; i < 11; i++)
            //    {
            //        Cards.Add(new PlayingCardBase((CardType)i, c.ImagePath, i.ToString()));
            //    }

            //    Cards.Remove(c);

            //    _UpdateDatabase();
            //}
        }

        private async void _DbError()
        {
            await DisplayAlert("Error", "There was an error reading the database. If you add a card, all previous card data will be lost.", "OK");
            _cards.Clear();
            _UpdateCards();
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
            var creatorPage = new PokemonCardCreatorPage(new PokemonCard(PokemonCardType.Colorless, path, ""), Navigation, canDelete: false);

            await Navigation.PushModalAsync(creatorPage);


            if (await creatorPage.WaitAsync)
            {
                //Add the edited card
                _cards.Add(creatorPage.Card);

                _UpdateDatabase();
            }
        }

        public async void EditCard(PokemonCard card)
        {
            PokemonCard old = (PokemonCard)card.Clone();
            //Edit the card
            var creatorPage = new PokemonCardCreatorPage(card, Navigation, canDelete: true);

            await Navigation.PushModalAsync(creatorPage);


            if (await creatorPage.WaitAsync)
            {
                if (creatorPage.Delete)
                {
                    _cards.Remove(card);
                }

                _UpdateDatabase();
            }
            else
            {
                _cards[_cards.IndexOf(card)] = old;
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
                xmlSerializer.WriteObject(fs, _cards);
            }
            _UpdateCards();
        }
    }
}
