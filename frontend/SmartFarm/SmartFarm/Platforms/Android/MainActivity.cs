using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Plugin.Firebase;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Core.Platforms.Android;
using SmartFarm.Services;

namespace SmartFarm
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                CrossFirebase.Initialize(this);
                
                CrossFirebaseCloudMessaging.Current.NotificationReceived += (s, e) =>
                {
                    Console.WriteLine($"📩 NotificationReceived: {e.Notification?.Title} - {e.Notification?.Body}");
                    ShowNotification(e.Notification?.Title, e.Notification?.Body);
                };
                CrossFirebaseCloudMessaging.Current.TokenChanged += (s, e) =>
                {
                    Console.WriteLine($"🎯 Token changed: {e.Token}");
                };

                var deviceToken = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

                Console.WriteLine("Firebase initialized successfully with device token is: " + deviceToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Firebase initialization failed: {ex.Message}");
            }
        }

        void ShowNotification(string title, string body)
        {
            var channelId = "high_priority_channel";
            var channelName = "Thông báo quan trọng";
            var channelDescription = "Thông báo hiển thị xổ xuống màn hình";
            var notificationManager = NotificationManagerCompat.From(this);

            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable);


            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High)
                {
                    Description = channelDescription
                };
              
                notificationManager.CreateNotificationChannel(channel);
            }

            var builder = new NotificationCompat.Builder(this, channelId)
                          .SetContentTitle(title)
                          .SetContentText(body)
                          .SetSmallIcon(Resource.Drawable.icon_app) // icon app
                          .SetAutoCancel(true)
                          .SetPriority((int)NotificationPriority.High)
                          .SetContentIntent(pendingIntent)
                          .SetDefaults((int) NotificationDefaults.All);

            notificationManager.Notify(1000, builder.Build());
        }

    }
}
