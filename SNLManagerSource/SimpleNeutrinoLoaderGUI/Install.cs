using FluentFTP;

namespace SimpleNeutrinoLoaderGUI
{
    internal class Install
    {
        public static async Task<string> GetStorageDevices(AsyncFtpClient client)
        {
            string returnString = "";
            string tempDir = await GetDir(client, "/mc/");

            if (tempDir.Contains('0'))
            {
                returnString += "mc0";
            }
            if (tempDir.Contains('1'))
            {
                returnString += "mc1";
            }
            if (await DirectoryExists(client, "/mass/0/"))
            {
                returnString += "mass";
            }
            return returnString;
        }

        static async Task<string> GetDir(AsyncFtpClient client, string ftpPath)
        {
            try
            {
                string returnList = "";
                for (int i = 0; i < 2; i++)
                {
                    var ftpList = await client.GetListing(ftpPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                    await client.Disconnect();
                    await client.Connect();
                    foreach (var item in ftpList)
                    {
                        if (!returnList.Contains(item.ToString()))
                        {
                            returnList += $" {item.Name} ";
                        }
                    }
                }
                return returnList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to list {ftpPath} via FTP.\n{ex.Message}\n{ex.InnerException}");
                await client.Disconnect();
                await client.Connect();
                return "";
            }
        }

        static async Task<bool> DirectoryExists(AsyncFtpClient client, string directoryPath)
        {
            try
            {
                await client.GetListing(directoryPath); // for compatibility with ps2ftpd, reconnect every time FtpDataStream is used
                await client.Disconnect();
                await client.Connect();
                await client.GetListing(directoryPath);
                await client.Disconnect();
                await client.Connect();
                return true;
            }
            catch
            {
                await client.Disconnect();
                await client.Connect();
                return false;
            }
        }
    }
}
