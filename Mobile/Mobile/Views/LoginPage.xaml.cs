using Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : BasePage
    {
        
        public LoginPage()
        {
            InitializeComponent();

            LoginViewModel viewModel = (LoginViewModel) this.BindingContext;

            viewModel.PropertyChanged += (sender, args) => {
                if (!args.PropertyName.Equals("IsAuthenticated"))
                {
                    return;
                }
                
                if (viewModel.IsAuthenticated)
                {
                    Navigation.PopModalAsync();
                }
            };
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

    }
}