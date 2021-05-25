using Pokedex.Cards;
using Pokedex.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Utils.Model;
using Xamarin.Forms;

namespace Pokedex.Cards
{
    [XmlRoot(ElementName = "root", Namespace = "")]
    [DataContract]
    public class PokemonCard : PlayingCardBase
    {
        protected override string PlaceHolderResourcePath => "Pokedex.Images.pokeshake.gif";

        private PokemonCardType _type;

        [DataMember]
        public PokemonCardType Type
        {
            get { return _type; }
            set { _UpdateField(ref _type, value); }
        }

        private int? _healthPoints;

        [DataMember]
        public int? HealthPoints
        {
            get { return _healthPoints; }
            set { _UpdateField(ref _healthPoints, value); }
        }


        PokemonCard() : base() { }

        public PokemonCard(PokemonCardType type, string imagePath, string name, int? Hp) : base(imagePath, name)
        {
            Type = type;
            HealthPoints = Hp;
        }

        public PokemonCard(string imagePath) : this(PokemonCardType.Colorless, imagePath, null, null) { }

        public override PlayingCardBase Clone()
        {
            return new PokemonCard(Type, ImagePath, Name, HealthPoints);
        }
    }

    public enum PokemonCardType
    {
        Colorless,
        Darkness,
        Dragon,
        Fairy,
        Fighting,
        Fire,
        Grass,
        Lightning,
        Metal,
        Psychic,
        Water,
    }
}
