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
using System.Reflection;
using System.Threading.Tasks;
using AForge.Imaging.Filters;
using Pokedex.Model;
using Bitmap = ShimDrawing::System.Drawing.Bitmap;

namespace Pokedex.Util
{
    static class Files
    {
        public static Stream GetResourceStream(string resource)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
        }

        public static async Task<string> SaveNewPhoto(FileResult photo)
        {
            //Save in {App Directory}\img\{GUID}.jpg
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "img", Guid.NewGuid().ToString() + ".jpg");

            //Canceled
            if (photo == null)
            {
                return null;
            }

            //Create img folder if needed
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            //Save the file to folder
            using (Stream stream = await photo.OpenReadAsync())
            using (Bitmap b = (Bitmap)ShimDrawing::System.Drawing.Image.FromStream(stream))
            using (Stream img = File.OpenWrite(path))
            {
                FiltersSequence f = new FiltersSequence();
                f.Add(new ResizeBilinear(800, (int)(800.0 * b.Height / b.Width)));

                if (b.Height < b.Width) f.Add(new RotateBilinear(-90));

                f.Apply(ImageProcessor.Format(b)).Save(img, ShimDrawing::System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            return path;
        }
    }
}
