using System;

#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
#endif

namespace MainToDoList.Helpers
{
    public static class AlarmScheduler
    {
        public static void ScheduleAlarm(string message, DateTime time)
        {
#if ANDROID
            var context = Android.App.Application.Context;

            var intent = new Intent(context, typeof(MainToDoList.Platforms.Android.AlarmReceiver));
            intent.SetAction("MAIN_TODO_ALARM");
            intent.PutExtra("message", message);

            var pendingIntent = PendingIntent.GetBroadcast(
                context,
                0,
                intent,
                PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
            );

            var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
            var triggerTime = new DateTimeOffset(time).ToUnixTimeMilliseconds();

            // استفاده از SetExactAndAllowWhileIdle برای اطمینان از اجرای آلارم حتی در حالت Doze
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerTime, pendingIntent);
#endif
        }
    }
}