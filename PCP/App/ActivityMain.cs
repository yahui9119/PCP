using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net;
using System.Net.Sockets;
namespace App
{
    [Activity(Label = "PCP", MainLauncher = true, Icon = "@drawable/icon")]
    public class ActivityMain : Activity
    {
        
        UdpClient client = new UdpClient();
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("60.191.34.90"),10086);
        protected override void OnCreate(Bundle bundle)
        {
                
            base.OnCreate(bundle);
            //Fullscreen
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            byte[] bytes= {0x23,0x23};
            client.Send(bytes, bytes.Length, endpoint);
            // Get our button from the layout resource,
            // and attach an event to it


        }
    }
}

