using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace MainToDoList.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "MAIN_TODO_ALARM" })]
    public class AlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var message = intent.GetStringExtra("message") ?? "تسک شما رسید!";

            var builder = new NotificationCompat.Builder(context, "reminders")
                .SetContentTitle("یادآوری تسک")
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.notif_icon) // استفاده از آیکون سفارشی
                .SetPriority((int)NotificationPriority.High)
                .SetAutoCancel(true);

            var notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Notify(1001, builder.Build());
        }
    }
}