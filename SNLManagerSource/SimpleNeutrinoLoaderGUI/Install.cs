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
				var ftpList = await client.GetListing(ftpPath);
				Thread.Sleep(200);
				var ftpList2 = await client.GetListing(ftpPath); // Try twice because the launchELF ftp server is strange
				Thread.Sleep(200);
				foreach (var item in ftpList)
				{
					returnList += $" {item.Name} ";
				}
				foreach (var item in ftpList2)
				{
					if (!returnList.Contains(item.ToString()))
					{
						returnList += $" {item.Name} ";
					}
				}
				return returnList;
			}
			catch
			{
				Thread.Sleep(200);
		    	return "";
			}
        }

        static async Task<bool> DirectoryExists(AsyncFtpClient client, string directoryPath)
        {
            try
            {
                await client.GetListing(directoryPath);
                Thread.Sleep(200);
                await client.GetListing(directoryPath); // Try twice because the launchELF ftp server is strange
                Thread.Sleep(200);
                return true;
            }
            catch
            {
                Thread.Sleep(200);
                return false;
            }
        }
    }
}
