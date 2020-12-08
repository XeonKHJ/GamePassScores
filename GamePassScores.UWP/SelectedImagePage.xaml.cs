using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace GamePassScores.UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SelectedImagePage : Page
    {
        public SelectedImagePage()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<ScreenshotViewModel> ScreenshotViewModels { set; get; } = new ObservableCollection<ScreenshotViewModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ScreenshotViewModels.Clear();

            var screenViewModelsAndSelectedModel = e.Parameter as Tuple<ObservableCollection<ScreenshotViewModel>, ScreenshotViewModel>;
            
            if(screenViewModelsAndSelectedModel != null)
            {
                var screenViewModels = screenViewModelsAndSelectedModel.Item1;

                if (screenViewModels != null)
                {
                    foreach (var s in screenViewModels)
                    {
                        ScreenshotViewModels.Add(s);
                    }
                }

                var selectedModel = screenViewModelsAndSelectedModel.Item2;
                if(selectedModel != null)
                {
                    ScreenshotsView.SelectedItem = selectedModel;
                }
            }

        }

        
        private void NavigationBackButton_Click(object sender, RoutedEventArgs e)
        {

            //ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ScreenshotBackwardConnectedAnimation", ScreenshotImage);
            Frame.GoBack();
        }
    }
}
