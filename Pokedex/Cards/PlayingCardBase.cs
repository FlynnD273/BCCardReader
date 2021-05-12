extern alias ShimDrawing;
using ShimDrawing::System.Drawing;
using System.IO;
using Xamarin.Forms;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Utils.Model;
using System.Threading.Tasks;
using Tesseract;
using XLabs.Ioc;
using System.Globalization;
using Pokedex.Util;

namespace Pokedex.Cards
{
    [XmlRoot(ElementName = "root", Namespace = "")]
    [DataContract]
    public class PlayingCardBase : NotifyPropertyChangedBase
    {
        protected virtual string PlaceHolderResourcePath => "";

        private static ITesseractApi _tesseractApi;
        protected static ITesseractApi TesseractApi
        {
            get
            {
                if (_tesseractApi == null)
                {
                    _tesseractApi = Resolver.Resolve<ITesseractApi>();
                }

                return _tesseractApi;
            }
        }

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

        private bool _isPlaceHolderBacking;
        private bool _isPlaceholder 
        {
            get
            {
                if (_IsCroppingDone())
                {
                    _isPlaceHolderBacking = false;
                }
               return _isPlaceHolderBacking;
            }

            set
            {
                _isPlaceHolderBacking = value;
            }
        }

        private ImageSource _croppedImage;
        public ImageSource CroppedImage
        {
            get
            {
                if (_croppedImage == null || _isPlaceholder)
                {
                    if (IsCropped && File.Exists(Path.ChangeExtension(_imagePath, "card")))
                    {
                        _croppedImage = ImageSource.FromFile(Path.ChangeExtension(_imagePath, "card"));
                    }
                    else if (_IsCroppingDone() && IsCropped)
                    {
                        cropTask = Task.Run(_CropImage);
                    }
                    else if (!IsCropped)
                    {
                        _croppedImage = Image;
                    }
                }

                return _croppedImage;
            }

            set { _UpdateField(ref _croppedImage, value); }
        }

        private bool _IsCroppingDone () => cropTask == null || cropTask.IsCompleted || cropTask.IsFaulted || cropTask.IsCanceled;

        private bool _isCropped = true;

        [DataMember]
        public bool IsCropped
        {
            get { return _isCropped; }
            set { _UpdateField(ref _isCropped, value, o => CroppedImage = null); }
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

        private async void _CropImage()
        {
            _isPlaceholder = true;
            CroppedImage = ImageSource.FromStream(() => Util.Files.GetResourceStream(PlaceHolderResourcePath));

            using (Bitmap img = (Bitmap)ShimDrawing::System.Drawing.Image.FromStream(File.OpenRead(ImagePath)))
            using (Bitmap cropped = ImageProcessor.FindPlayingCard(img))
            {
                if (cropped != null)
                {
                    using (Stream writer = File.OpenWrite(Path.ChangeExtension(ImagePath, "card")))
                    {
                        cropped.Save(writer, ShimDrawing::System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    await _GetText(cropped);
                }
            }

            _isPlaceholder = false;
            CroppedImage = null;
        }

        private async Task _GetText(Bitmap cropped)
        {
            if (!TesseractApi.Initialized)
            {
                await TesseractApi.Init("eng");
            }

            TesseractApi.SetRectangle(new Tesseract.Rectangle((int)(cropped.Width * 0.24), 10, (int)(cropped.Width * 0.35), (int)(cropped.Height * 0.065)));
            TesseractApi.SetWhitelist("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
            if (await TesseractApi.SetImage(File.OpenRead(Path.ChangeExtension(ImagePath, "card"))))
            {
                string s = TesseractApi.Text;

                if (Name == "" || Name == "NO TEXT DETECTED")
                {
                    if (s != "")
                    {
                        if (s != "" && s[0].ToString() == s[0].ToString().ToLower())
                        {
                            s = s.Substring(1, s.Length - 1);
                        }
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                        s = textInfo.ToTitleCase(s.ToLower());
                    }
                    else
                    {
                        s = "NO TEXT DETECTED";
                    }

                    Name = s;
                }
            }
        }
    }
}
