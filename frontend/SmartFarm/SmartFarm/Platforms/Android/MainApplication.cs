using Android.App;
using Android.Runtime;
using Microsoft.Maui.Platform;
using Plugin.Firebase.Core.Platforms.Android;
namespace SmartFarm
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
        public override void OnCreate()
        {
            base.OnCreate();
        }

    }
}
