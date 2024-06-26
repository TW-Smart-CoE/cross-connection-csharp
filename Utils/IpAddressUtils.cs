﻿using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace CConn
{
    public static class IpAddressUtils
    {
        public static uint IpStringToUInt(string ipAddress)
        {
            var address = IPAddress.Parse(ipAddress);
            byte[] bytes = address.GetAddressBytes();

            return ByteUtils.GetUInt(bytes, 0);
        }

        public static string UIntToIpString(uint ipAddress)
        {
            byte[] bytes = new byte[4];
            ByteUtils.PutUInt(bytes, 0, ipAddress);

            return new IPAddress(bytes).ToString();
        }

        public static IPAddress GetLocalIpAddress()
        {
            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // PC: NetworkInterfaceType.Wireless80211, NetworkInterfaceType.Ethernet
                // Android: 0
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                    netInterface.NetworkInterfaceType == 0)
                {
                    foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (!addrInfo.Address.ToString().StartsWith("169.254"))
                            {
                                return addrInfo.Address;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
