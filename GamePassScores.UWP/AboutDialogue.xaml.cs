using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace GamePassScores.UWP
{
    public sealed partial class AboutDialogue : ContentDialog
    {
        public AboutDialogue()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Key);
            switch (e.Key)
            {
                case Windows.System.VirtualKey.GamepadView:
                    this.Hide();
                    break;
                    //test
            }
        }
    }
}
