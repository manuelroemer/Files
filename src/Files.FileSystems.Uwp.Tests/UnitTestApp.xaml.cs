namespace Files.FileSystems.Uwp.Tests
{
    using System;
    using System.Diagnostics;
    using Microsoft.VisualStudio.TestPlatform.TestExecutor;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += (sender, e) => 
                    throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
                Window.Current.Content = rootFrame;
            }
            
            DebugSettings.EnableFrameRateCounter = Debugger.IsAttached;
            UnitTestClient.CreateDefaultUI();
            Window.Current.Activate();
            UnitTestClient.Run(e.Arguments);
        }
    }
}
