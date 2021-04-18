using System.Threading.Tasks;
using Utils.Model;
using Xamarin.Forms;

namespace CameraTest.Model
{
    internal class ViewModelBase : NotifyPropertyChangedBase
    {
        internal INavigation Navigation;
        private Page _page;
        public ViewModelBase (INavigation navigation, Page page)
        {
            Navigation = navigation;
            _page = page;
        }

        internal async Task DisplayAlert (string title, string message, string cancel)
        {
            await _page.DisplayAlert(title, message, cancel);
        }

        internal async Task<bool> DisplayAlert(string title, string message, string cancel, string accept)
        {
            return await _page.DisplayAlert(title, message, accept, cancel);
        }
    }
}