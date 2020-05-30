using Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;

namespace Mobile.ViewModels
{
    class FeedViewModel : BaseViewModel
    {
        public bool isRefreshing;

        public bool IsRefreshing
        {
            get => isRefreshing;
            set => SetProperty(ref isRefreshing, value);
        }

        public ICommand RefreshCommand { get; }

        public ObservableCollection<Tree> Trees { get; private set; }

        public FeedViewModel()
        {
            Trees = new ObservableCollection<Tree>();

            RefreshCommand = new Command(async () => await RefreshFeed());
            RefreshCommand.Execute(null);
        }

        public async Task RefreshFeed()
        {
            try
            {
                IsRefreshing = true;
                await App.TreeService.Synchronize();

                Trees.Clear();

                foreach (Tree tree in await App.Database.GetTreesAsync())
                {
                    Trees.Add(tree);
                }
            }
            catch
            {
                await DisplayAlertAsync("Something went wrong while communicating with the server.");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

    }
}
