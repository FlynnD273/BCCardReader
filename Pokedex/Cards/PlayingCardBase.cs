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
using Tesseract;
using XLabs.Ioc;
using AForge.Imaging;
using System;

namespace Pokedex.Cards
{
    [XmlRoot(ElementName = "root", Namespace = "")]
    [DataContract]
    public class PlayingCardBase : NotifyPropertyChangedBase
    {
        protected string _loadingImage = "";

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

        private async void _CropImage()
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

                    //int y = 0;
                    //UnmanagedImage im = UnmanagedImage.FromManagedImage(cropped);
                    //ShimDrawing::System.Drawing.Color startCol = im.GetPixel(im.Width / 2, 5);
                    //int threshold = 20;

                    //for (y = 0; y < im.Height / 2; y++)
                    //{
                    //    ShimDrawing::System.Drawing.Color col = im.GetPixel(im.Width / 2, y);
                    //    if (Math.Abs(col.R - startCol.R) > threshold || Math.Abs(col.G - startCol.G) > threshold || Math.Abs(col.B - startCol.B) > threshold)
                    //    {
                    //        break;
                    //    }
                    //}

                    if (!TesseractApi.Initialized)
                    {
                        await TesseractApi.Init("eng");
                    }

                    TesseractApi.SetRectangle(new Tesseract.Rectangle((int)(cropped.Width * 0.24), 10, (int)(cropped.Width * 0.4), (int)(cropped.Height * 0.064)));
                    TesseractApi.SetWhitelist("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                    if (await TesseractApi.SetImage(File.OpenRead(Path.ChangeExtension(ImagePath, "card"))))
                    {
                        string s = TesseractApi.Text;
                        if (Name == "")
                        {
                            Name = s;
                        }
                    }
                }
            }

            _isPlaceholder = false;
        }
    }
}
