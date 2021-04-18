using Pokedex.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Utils.Model;
using Xamarin.Forms;

namespace Pokedex.Model
{
    [XmlRoot(ElementName = "root", Namespace = "")]
    //[Serializable]
    [DataContract]
    public class PokemonCard : NotifyPropertyChangedBase
    {
        private ImageSource _image;

        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = ImageSource.FromFile(_imagePath);
                }

                return _image;
            }
        }

        //private ImageSource _thumbnail;
        //public ImageSource Thumbnail
        //{
        //    get
        //    {
        //        if (_thumbnail == null)
        //        {
        //            _thumbnail = ImageSource.FromFile(Path.ChangeExtension(_imagePath, "thumb"));
        //        }

        //        return _thumbnail;
        //    }
        //}


        private CardType _type;

        //[XmlElement("Type")]
        [DataMember]
        public CardType Type
        {
            get { return _type; }
            set { _UpdateField(ref _type, value); }
        }

        private string _name;

        //[XmlElement("Name")]
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _UpdateField(ref _name, value); }
        }

        private string _imagePath;

        [DataMember]
        public string ImagePath
        {
            get { return _imagePath; }
            private set { _UpdateField(ref _imagePath, value); }
        }


        PokemonCard()
        {

        }

        public PokemonCard(CardType type, string imagePath, string name)
        {
            Type = type;
            ImagePath = imagePath;
            Name = name;
        }

        public PokemonCard Clone()
        {
            return new PokemonCard(Type, ImagePath, Name);
        }
    }

    public enum CardType
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
