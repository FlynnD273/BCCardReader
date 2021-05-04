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
    //[Serializable]
    [DataContract]
    public class PokemonCard : PlayingCardBase
    {
        private PokemonCardType _type;

        [DataMember]
        public PokemonCardType Type
        {
            get { return _type; }
            set { _UpdateField(ref _type, value); }
        }

        PokemonCard() : base()
        {
            _loadingImage = "Pokedex.Images.pokeshake.gif";
        }

        public PokemonCard(PokemonCardType type, string imagePath, string name) : base(imagePath, name)
        {
            _loadingImage = "Pokedex.Images.pokeshake.gif";

            Type = type;
        }

        public override PlayingCardBase Clone()
        {
            return new PokemonCard(Type, ImagePath, Name);
        }
    }

    public enum PokemonCardType
    {
        Grass,
        Fire,
        Water,
        Lightning,
        Psychic,
        Fighting,
        Darkness,
        Metal,
        Colorless,
        Fairy,
        Dragon,
    }
}
