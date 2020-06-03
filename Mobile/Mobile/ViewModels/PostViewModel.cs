using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Mobile.ViewModels
{
    class PostViewModel : BaseViewModel
    {
        public PostViewModel()
        {
            IsBusy = true;
        }

        private string _imagePath;

        public string ImagePath {
            get
            {
                return _imagePath;
            }
            set
            {
                _imagePath = value;
                IsBusy = false;
            }
        }
    }
}
