using Mobile.Models;
using System.Collections;
using System.Collections.ObjectModel;

namespace Mobile.ViewModels
{
    class MapViewModel : BaseViewModel
    {
        public MapViewModel()
        {
            _trees = new ObservableCollection<Tree>();
        }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            _trees.Clear();

            foreach (Tree tree in await App.Database.GetTreesAsync())
            {
                _trees.Add(tree);
            }
        }

        private readonly ObservableCollection<Tree> _trees;

        public IEnumerable Trees { get => _trees; }
    }
}
