using Microsoft.Phone.Tasks;

namespace VoiceShortcuts
{
    public class TaskOpener
    {
        public static void OpenWifiTask()
        {
            var task = new ConnectionSettingsTask();
            task.ConnectionSettingsType = ConnectionSettingsType.WiFi;
            task.Show();            
        }

        public static void OpenCellularTask()
        {
            var task = new ConnectionSettingsTask();
            task.ConnectionSettingsType = ConnectionSettingsType.Cellular;
            task.Show();
        }

        public static void OpenBluetoothTask()
        {
            var task = new ConnectionSettingsTask();
            task.ConnectionSettingsType = ConnectionSettingsType.Bluetooth;
            task.Show();
        }

        public static void OpenAirplaneTask()
        {
            var task = new ConnectionSettingsTask();
            task.ConnectionSettingsType = ConnectionSettingsType.AirplaneMode;
            task.Show();
        }
    }
}