using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRS.Application.Abstractions
{
    public interface IFlespiService
    {
        Task<string> GetChannelInfoAsync(int channelId);
        Task<string> GetAllDevicesAsync();

        Task<string> GetDeviceInfoAsync(int deviceId);

        Task<string> GetLatestMessagesAsync(int deviceId, int count = 10);
    }
}
