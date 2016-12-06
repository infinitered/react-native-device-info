using ReactNative.Bridge;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RNDeviceInfo
{
    public class RNDeviceInfoModule : ReactContextNativeModuleBase
    {
        public RNDeviceInfoModule(ReactContext reactContext)
            : base(reactContext)
        {
        }

        public override string Name
        {
            get
            {
                return "RNDeviceInfo";
            }
        }

        private bool IsEmulator(string model)
        {
            Regex rgx = new Regex("(?i:virtual)");
            return rgx.IsMatch(model);
        }

        [DllImport("user32")]
        protected static extern int GetSystemMetrics(int nIndex);

        private bool IsTablet()
        {
            const int SM_TABLETPC = 86;
            return GetSystemMetrics(SM_TABLETPC) != 0;
        }

        public override IReadOnlyDictionary<string, object> Constants
        {

            get
            {
                Dictionary<string, object> constants = new Dictionary<string, object>();

                constants["appVersion"] = "not available";
                constants["buildVersion"] = "not available";
                constants["buildNumber"] = 0;

                String packageName = Assembly.GetExecutingAssembly().GetName().ToString();
                Version version = Assembly.GetExecutingAssembly().GetName().Version;

                try
                {
                    constants["appVersion"] = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                    constants["buildNumber"] = version.Build.ToString();
                    constants["buildVersion"] = version.Build.ToString();
                }
                catch
                {
                }

                String deviceName = "not available";
                String manufacturer = "not available";
                String device_id = "not available";
                String model = "not available";
                String hardwareVersion = "not available";
                String unique_id = "not available";
                String osVersion = "not available";
                String os = "not available";

                CultureInfo culture = CultureInfo.CurrentCulture;

                try
                {
                    deviceName = Environment.MachineName;
                    ManagementClass management = new ManagementClass("Win32_ComputerSystem");
                    foreach (ManagementBaseObject deviceInfo in management.GetInstances())
                    {
                        manufacturer = (string)deviceInfo["Manufacturer"];
                        model = (string)deviceInfo["Model"];
                        device_id = (string)deviceInfo["SystemSKUNumber"];
                    }
                    
                    ManagementClass product = new ManagementClass("Win32_ComputerSystemProduct");
                    foreach (ManagementBaseObject deviceInfo in product.GetInstances())
                    {
                        hardwareVersion = (string)deviceInfo["Version"];
                        unique_id = (string)deviceInfo["UUID"];
                    }

                    ManagementClass operatingSystem = new ManagementClass("Win32_OperatingSystem");
                    foreach (ManagementBaseObject deviceInfo in operatingSystem.GetInstances())
                    {
                        os = (string)deviceInfo["Caption"];
                        osVersion = (string)deviceInfo["Version"];
                    }
                }
                catch
                {
                }

                constants["instanceId"] = unique_id;
                constants["deviceName"] = deviceName;
                constants["systemName"] = os;
                constants["systemVersion"] = osVersion;
                constants["model"] = model;
                constants["brand"] = manufacturer;
                constants["deviceId"] = hardwareVersion;
                constants["deviceLocale"] = culture.Name;
                constants["deviceCountry"] = culture.EnglishName;
                constants["uniqueId"] = device_id;
                constants["systemManufacturer"] = manufacturer;
                constants["bundleId"] = packageName;
                constants["userAgent"] = "not available";
                constants["timezone"] = TimeZoneInfo.Local.Id;
                constants["isEmulator"] = IsEmulator(model);
                constants["isTablet"] = IsTablet();

                return constants;
            }
        }

        public List<UsbDeviceInfo> GetDevices()
        {
            var devices = new List<UsbDeviceInfo>();
            ManagementClass management = new ManagementClass("Win32_USBControllerDevice");
            foreach (ManagementBaseObject device in management.GetInstances())
            {
                var dependent = (ManagementBaseObject)device.GetPropertyValue("Dependent");
                devices.Add(new UsbDeviceInfo(
                    (string)dependent["DeviceID"],
                    (string)dependent["PNPDeviceID"],
                    (string)dependent["Description"],
                    (string)dependent["Name"]
                    ));
            }
            return devices;
        }
    }

    public class UsbDeviceInfo
    {
        public UsbDeviceInfo(string deviceID, string pnpDeviceID, string description, string name)
        {
            this.DeviceId = deviceID;
            this.PnpDeviceId = pnpDeviceID;
            this.Description = description;
            this.Name = name;
        }
        public string DeviceId { get; private set; }
        public string PnpDeviceId { get; private set; }
        public string Description { get; private set; }
        public string Name { get; private set; }
    }
}
