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
using System.Collections.Generic;

namespace Pokedex.Model
{
    class CameraModel : ViewModelBase
    {
        private readonly string workingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private readonly string dbPath;

        private DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(ObservableCollection<PokemonCard>));

        private ObservableCollection<PokemonCard> _cards { get; } = new ObservableCollection<PokemonCard>();

        public Func<PokemonCard, string> Sort { get => _sortDict[SortBy[_sortIndex]]; }

        private Dictionary<string, Func<PokemonCard, string>> _sortDict = new Dictionary<string, Func<PokemonCard, string>>();

        public string[] SortBy { get => _sortDict.Keys.ToArray(); }

        private int _sortIndex;
        public int SortIndex
        {
            get { return _sortIndex; }
            set { _UpdateField(ref _sortIndex, value, _UpdateSort); }
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
            _sortDict.Add("Name", o => o.Name);
            _sortDict.Add("Type", o => o.Type.ToString());


            dbPath = Path.Combine(workingPath, "cardsdb.xml");

            //Read database
            if (File.Exists(dbPath))
            {
                try
                {
                    _cards = (ObservableCollection<PokemonCard>)xmlSerializer.ReadObject(new XmlTextReader(dbPath));
                    _UpdateSort();
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
        }

        private void _UpdateSort(int o = default)
        {
            if (_cards.Count > 0)
            {
                if (SortBy[SortIndex] == "Type")
                {
                    SortedCards = _cards.OrderBy(Sort).ThenBy(obj => obj.Name).ToArray();
                }
                else
                {
                    SortedCards = _cards.OrderBy(Sort).ToArray();
                }
            }
            else
            {
                SortedCards = new PokemonCard[0];
            }
        }

        private async void _DbError()
        {
            await DisplayAlert("Error", "There was an error reading the database. If you add a card, all previous card data will be lost.", "OK");
            _cards.Clear();
            _UpdateSort();
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
                await DisplayAlert("ERROR", e.Message, "OK");
            }

            if (path == null)
            {
                return;
            }

            //Edit the card
            var creatorPage = new PokemonCardCreatorPage(new PokemonCard(path), Navigation, canDelete: false);

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
                var index = _cards.IndexOf(card);
                if (index > 0)
                {
                    _cards[index] = old;
                }
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
            _UpdateSort();
        }
    }
}
