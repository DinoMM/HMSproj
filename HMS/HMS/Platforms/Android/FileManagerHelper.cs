using Android.Media;
using Android.OS;
using Android.Content;
using Android.Net;
using Java.IO;

namespace HMS.Platforms.Android
{
    public static class FileManagerHelper
    {
        public static void NotifyFileAdded(string filePath)
        {
            try
            {
                var context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? Microsoft.Maui.ApplicationModel.Platform.AppContext;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    // Notify for files on Android 10+ using MediaScannerConnection
                    MediaScannerConnection.ScanFile(context, new[] { filePath }, null, null);
                }
                else
                {
                    // Notify for older Android versions
                    var uri = global::Android.Net.Uri.FromFile(new Java.IO.File(filePath));
                    var intent = new Intent(Intent.ActionMediaScannerScanFile, uri);
                    context.SendBroadcast(intent);
                }
            }
            catch (Exception ex) { }
        }
    }
}
