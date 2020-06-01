using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;

namespace Mobile.Droid
{
    [Activity(Label = "One More Tree", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        protected override void OnResume()
        {
            base.OnResume();

            Xamarin.Essentials.Platform.OnResume();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
        [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "onemoretree")]
        public class WebAuthenticationCallbackActivity : Xamarin.Essentials.WebAuthenticatorCallbackActivity
        {
        }
    }
}