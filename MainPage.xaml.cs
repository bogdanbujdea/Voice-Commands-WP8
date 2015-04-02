using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Inneractive.Ad;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Phone.Speech.Synthesis;
using Windows.Phone.Speech.VoiceCommands;
using Windows.Phone.Devices.Power;
using AppPromo;
using Microsoft.Phone.Tasks;

namespace VoiceShortcuts
{
    public partial class MainPage : PhoneApplicationPage
    {
        private VibrateController _vc;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            _vc = VibrateController.Default;
            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
            Loaded += MainPageLoaded;

        }

        async void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await VoiceCommandService.InstallCommandSetsFromFileAsync(new Uri("ms-appx:///ShortcutCommands.xml"));
                Battery battery = Battery.GetDefault();
                TextBatteryPercent.Text = battery.RemainingChargePercent + "%";
                if (battery.RemainingDischargeTime.Hours < 1)
                    TextBatteryTime.Text = "less than 1 hour";
                else
                    TextBatteryTime.Text = battery.RemainingDischargeTime.Hours + "hours";
                Dictionary<InneractiveAd.IaOptionalParams, string> optionalParams;
                optionalParams = new Dictionary<InneractiveAd.IaOptionalParams, string>();
                optionalParams.Add(InneractiveAd.IaOptionalParams.Key_Distribution_Id, "659");
                if (!InneractiveAd.DisplayAd("Student_VoiceShortcuts_WP", InneractiveAd.IaAdType.IaAdType_Banner, NaxAdGrid, 30, optionalParams))
                {
                    MessageBox.Show("This application is free but requires an internet connection. Please configure your connectivity settings and re-try.");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error occured: " + exception.Message);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //Cand navigam de la o pagina la alta, putem trimite niste string-uri ca parametri
            //Acesti parametri ii putem lua din NavigationContext.QueryString, care este un dictionar de tip <string,string>
            //Pe noi ne intereseaza sa aflam daca exista cheia "voiceCommandName"
            //Daca exista, luam valoarea asociata acestei chei, si verificam daca este una dintre comenzile noastre
            if (NavigationContext.QueryString.ContainsKey("voiceCommandName"))
            {
                //Cel mai simplu ar fi sa intrati in Debug si sa va uitati la ce contine
                //Dar va puteti imagina ca arata cam asa: QueryString["voiceCommandName"] = "MakeCoffee", sau oricare dintre comenzile date de voi
                string command = NavigationContext.QueryString["voiceCommandName"];
                //dupa atribuire, in variabila command va fi string-ul "MakeCoffee"
                switch (command) //e mai usor cu un switch sa verificam toate comenzile
                {
                    case "Open Wifi":
                        TaskOpener.OpenWifiTask();
                        break;
                    case "Open Cellular":
                        //in QueryString["number"] va fi un numar salvat ca un string, dar nu ca un sir de cifre ci ca un sir de litere
                        //adica putem avea "five", "ten", "one", etc. 
                        //din acest motiv, trebuie sa transformam string-uri ca mai sus, in cifre, si facem acest lucru cu functia StringToNumber definita de noi
                        TaskOpener.OpenCellularTask();
                        break;
                    case "Open Airplane":
                        TaskOpener.OpenAirplaneTask();
                        break;
                    case "Open Bluetooth":
                        TaskOpener.OpenBluetoothTask();
                        break;
                    case "Get Battery Level":
                        GetBatteryLevel();
                        break;
                    case "Get Battery Time":
                        GetBatteryTime();
                        break;
                }
            }
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        private void GetBatteryLevel()
        {
            Battery battery = Battery.GetDefault();
            SpeechSynthesizer ss = new SpeechSynthesizer();
            ss.SpeakTextAsync(String.Format("The battery level is {0} percent",
                                            battery.RemainingChargePercent.ToString()));
        }

        private void GetBatteryTime()
        {
            Battery battery = Battery.GetDefault();
            SpeechSynthesizer ss = new SpeechSynthesizer();
            ss.SpeakTextAsync(String.Format("You have {0} hours, {1} minutes and {2} seconds until full discharge",
                                            battery.RemainingDischargeTime.Hours, battery.RemainingDischargeTime.Minutes, battery.RemainingDischargeTime.Seconds));
        }

        private void WifiTap(object sender, RoutedEventArgs e)
        {
            _vc.Start(TimeSpan.FromMilliseconds(100));
            TaskOpener.OpenWifiTask();
        }

        private void AirPlaneModeTap(object sender, RoutedEventArgs e)
        {
            _vc.Start(TimeSpan.FromMilliseconds(100));
            TaskOpener.OpenAirplaneTask();
        }

        private void CellularTap(object sender, RoutedEventArgs e)
        {
            _vc.Start(TimeSpan.FromMilliseconds(100));
            TaskOpener.OpenCellularTask();
        }

        private void BluetoothTap(object sender, RoutedEventArgs e)
        {
            _vc.Start(TimeSpan.FromMilliseconds(100));
            TaskOpener.OpenBluetoothTask();
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton helpButton = new ApplicationBarIconButton(new Uri("/Assets/questionmark.png", UriKind.Relative));
            helpButton.Text = "Help";
            helpButton.Click += HelpTap;
            ApplicationBar.Buttons.Add(helpButton);

        }

        void HelpTap(object sender, EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/HelpPage.xaml", UriKind.RelativeOrAbsolute));
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't navigate to help page, error: " + exception.Message);
            }
        }

        private void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            Application.Current.Terminate();
        }

        private void OnTryReminderCompleted(object sender, RateReminderResult e)
        {
            if (e.Runs == 5)
            {
                var reschedule = RescheduleRating(e.RatingShown);
                if (reschedule)
                {
                    RateReminder.ResetCounters();
                    RateReminder.RunsBeforeReminder = 5;
                }
            }
        }

        private bool RescheduleRating(bool ratingShown)
        {
            if (ratingShown == false)
            {
                var messageBoxResult = MessageBox.Show(
                    "Would you like to send us some feedback through email?", "feedback",
                    MessageBoxButton.OKCancel);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    EmailComposeTask task = new EmailComposeTask();
                    task.To = "bujdeabogdan@gmail.com";
                    task.Subject = "Voice Shortcuts Feedback";
                    task.Show();
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}