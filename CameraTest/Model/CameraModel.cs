using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using Utils.Command;
using Utils.Model;
using Xamarin.Forms;

namespace CameraTest.Model
{
    class CameraModel : NotifyPropertyChangedBase
    {
        private ImageSource _cameraImage;
        public ImageSource CameraImage
        {
            get { return _cameraImage; }
            set { _UpdateField(ref _cameraImage, value); }
        }

        public DelegateCommand TakeImageCommand { get; }
        public DelegateCommand PickImageCommand { get; }

        public CameraModel()
        {
            TakeImageCommand = new DelegateCommand(_TakeImage);
            PickImageCommand = new DelegateCommand(_PickImage);
        }

        private async void _TakeImage()
        {
            try
            {
                var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
                {
                    DefaultCamera = CameraDevice.Rear,
                    Directory = "Test",
                    SaveToAlbum = true
                });

                if (photo != null)
                    CameraImage = ImageSource.FromStream(() => { return photo.GetStream(); });

            }
            catch (Exception ex)
            {
                //await DisplayAlert("Error", ex.Message.ToString(), "Ok");
            }
        }

        private async void _PickImage()
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                //await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                return;
            }
            var file = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
            });


            if (file == null)
                return;

            CameraImage = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }
    }
}
