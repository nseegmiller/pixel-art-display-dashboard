using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pixel_Art_Display_Dashboard
{
    public class Divoom
    {
        private static readonly HttpClient client = new();

        public string? IPAddress { get; set; }

        const string HTTP = "http://";
        const string POST = "/post";
        
        const string TEST_COMMAND = "Device/GetWeatherInfo";
        const string PLAY_GIF_COMMAND = "Device/PlayTFGif";
        const string SEND_FILE_COMMAND = "Draw/SendHttpGif";
        const string RESET_PIC_ID = "Draw/ResetHttpGifId";
        const string GET_GIF_ID = "Draw/GetHttpGifId";
        const string PLAY_BUZZER = "Device/PlayBuzzer";
        const string REBOOT = "Device/SysReboot";
        
        const string GET_DEVICES_PATH = "https://app.divoom-gz.com/Device/ReturnSameLANDevice";

        public static async Task<List<Device>> GetPanels()
        {
            HttpResponseMessage response = await client.PostAsync(GET_DEVICES_PATH, null);
            var result = await response.Content.ReadFromJsonAsync<DeviceResponse>();
            return result?.DeviceList ?? [];
        }

        private async Task<string> SendPayload(StringContent payload)
        {
            if (string.IsNullOrEmpty(IPAddress)) throw new InvalidOperationException("IP Address hasn't been set");
            string path = HTTP + IPAddress + POST;
            HttpResponseMessage response = await client.PostAsync(path, payload);
            string content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public Task TestConnection()
        {
            StringContent payload = DivoomHelpers.MakePayload(new
            {
                Command = TEST_COMMAND
            });
            return SendPayload(payload);
        }

        public Task SendRemoteImage(string URL)
        {
            StringContent payload = DivoomHelpers.MakePayload(new
            {
                Command = PLAY_GIF_COMMAND,
                FileType = 2,
                FileName = URL
            });
            return SendPayload(payload);
        }

        public Task ResetPicId()
        {
            StringContent payload = DivoomHelpers.MakePayload(new
            {
                Command = RESET_PIC_ID
            });
            return SendPayload(payload);
        }

        public async Task<Task> SendImage(string filePath)
        {
            await ResetPicId();
            using (Bitmap gif = new(filePath))
            {
                if (!DivoomHelpers.IsValidImageSize(gif))
                {
                    HttpResponseMessage response = new(HttpStatusCode.NotAcceptable)
                    {
                        Content = DivoomHelpers.CreateResponseContent($"Image must be 16x16, 32x32, or 64x64. This is: {gif.Width}x{gif.Height}")
                    };
                    return Task.CompletedTask;
                }
                string[] base64Frames = DivoomHelpers.ConvertImageToBase64(gif);
                for (int i = 0; i < base64Frames.Length; i++)
                {
                    int frameTime = DivoomHelpers.GetFrameTime(gif, i);
                    StringContent payload = DivoomHelpers.MakePayload(new
                    {
                        Command = SEND_FILE_COMMAND,
                        PicNum = base64Frames.Length,
                        PicWidth = gif.Width,
                        PicOffset = i,
                        PicID = 1,
                        PicSpeed = frameTime,
                        PicData = base64Frames[i]
                    });
                    await SendPayload(payload);
                }
            }
            return Task.CompletedTask;
        }

        public Task PlayBuzzer()
        {
            StringContent payload = DivoomHelpers.MakePayload(new
            {
                Command = PLAY_BUZZER,
                ActiveTimeInCycle = 500,
                OffTimeInCycle = 500,
                PlayTotalTime = 3000
            });
            return SendPayload(payload);
        }

        public Task Reboot()
        {
            StringContent payload = DivoomHelpers.MakePayload(new
            {
                Command = REBOOT
            });
            return SendPayload(payload);
        }
    }
}
