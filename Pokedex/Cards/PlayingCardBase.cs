extern alias ShimDrawing;
using ShimDrawing::System.Drawing;
using System.IO;
using Xamarin.Forms;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Utils.Model;
using Pokedex.Model;
using System.Threading.Tasks;
using Utils.Command;

namespace Pokedex.Cards
{
    [XmlRoot(ElementName = "root", Namespace = "")]
    [DataContract]
    public class PlayingCardBase : NotifyPropertyChangedBase
    {
        protected string _loadingImage = "";

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

        private Task cropTask;
        private bool _isPlaceholder;
        private ImageSource _croppedImage;
        public ImageSource CroppedImage
        {
            get
            {
                if (_croppedImage == null || _isPlaceholder)
                {
                    if (File.Exists(Path.ChangeExtension(_imagePath, "card")))
                    {
                        _croppedImage = ImageSource.FromFile(Path.ChangeExtension(_imagePath, "card"));
                    }
                    else if (cropTask == null || cropTask.IsCompleted)
                    {
                        cropTask = Task.Run(_CropImage);
                    }
                }

                return _croppedImage;
            }

            set { _UpdateField(ref _croppedImage, value); }
        }

        private string _name;

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
            set { _UpdateField(ref _imagePath, value); }
        }

        protected PlayingCardBase()
        {

        }

        public PlayingCardBase(string imagePath, string name)
        {
            ImagePath = imagePath;
            Name = name;
        }

        public virtual PlayingCardBase Clone()
        {
            return new PlayingCardBase(ImagePath, Name);
        }

        private void _CropImage()
        {
            _isPlaceholder = true;
            CroppedImage = ImageSource.FromStream(() => Util.Files.GetResourceStream(_loadingImage));

            using (Bitmap img = (Bitmap)ShimDrawing::System.Drawing.Image.FromStream(File.OpenRead(ImagePath)))
            using (Bitmap cropped = ImageProcessor.FindPlayingCard(img))
            {
                if (cropped != null)
                {
                    using (Stream writer = File.OpenWrite(Path.ChangeExtension(ImagePath, "card")))
                    {
                        cropped.Save(writer, ShimDrawing::System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    CroppedImage = null;
                }
            }

            _isPlaceholder = false;
        }
    }
}
