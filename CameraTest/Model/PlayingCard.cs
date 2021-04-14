using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Utils.Model;
using Xamarin.Forms;

namespace CameraTest.Model
{
    //[XmlRoot(ElementName = "root", Namespace = "")]
    //[Serializable]
    public class PlayingCard : NotifyPropertyChangedBase
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
        //            _thumbnail = ImageSource.FromFile(_imagePath);
        //        }

        //        return _thumbnail;
        //    }
        //}


        private CardType _type;

        [XmlElement("Type")]
        public CardType Type
        {
            get { return _type; }
            set { _UpdateField(ref _type, value); }
        }

        private string _name;

        [XmlElement("Name")]
        public string Name
        {
            get { return _name; }
            set { _UpdateField(ref _name, value); }
        }

        [XmlElement("ImagePath")]
        private string _imagePath { get; }

        PlayingCard()
        {

        }

        public PlayingCard (CardType type, string imagePath, string name)
        {
            Type = type;
            _imagePath = imagePath;
            Name = name;
        }
    }

    [Serializable]
    public enum CardType
    {
        Normal,
        Grass,
        Electric,
        Water,
        Fire,
        Rock,
        Dragon,
        Fairy,
    }
}
