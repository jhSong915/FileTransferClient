using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileTransferClient
{
    public class FTPClient
    {
        public object _ftp_ip { get; private set; }
        public object _ftp_port { get; private set; }
        public object _ftp_userId { get; private set; }
        public object _ftp_password { get; private set; }

        public FTPClient(object ftp_ip, object ftp_port, object ftp_userId, object ftp_password)
        {
            _ftp_ip = ftp_ip;
            _ftp_port = ftp_port;
            _ftp_userId = ftp_userId;
            _ftp_password = ftp_password;
        }

        public bool ConnectToServer()
        {
            string url = $"ftp://{_ftp_ip}:{_ftp_port}/";
            try
			{
                FtpWebRequest _ftpWebRequest = (FtpWebRequest)WebRequest.Create(url);
                _ftpWebRequest.Credentials = new NetworkCredential((string)_ftp_userId, (string)_ftp_password);
                _ftpWebRequest.KeepAlive = false;
                _ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                _ftpWebRequest.UsePassive = true;

                using (FtpWebResponse _response =(FtpWebResponse)_ftpWebRequest.GetResponse())
                {
                    return true;
                }
            }
			catch (Exception ex)
			{
                MessageBox.Show($"Failed to Connect Server {_ftp_ip}:{_ftp_port} - {ex.Message}");
                return false;
			}
        }

        public bool UploadFile(string localFilePath, string remoteFilePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(localFilePath);
                string url = $"FTP://{this._ftp_ip}:{this._ftp_port}/{remoteFilePath}";

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential((string)this._ftp_userId, (string)this._ftp_password);

                using (FileStream fileStream = fileInfo.OpenRead())
                using (Stream requestStream = request.GetRequestStream())
                {
                    fileStream.CopyTo(requestStream);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) { }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload file {remoteFilePath}: {ex.Message}");
                return false;
            }
        }

        public bool DownloadFile(string remoteFilePath, string localFilePath)
        {
            try
            {
                string url = $"FTP://{this._ftp_ip}:{this._ftp_port}/{remoteFilePath}".Replace("\\", "/");

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential((string)this._ftp_userId, (string)this._ftp_password);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    responseStream.CopyTo(fileStream);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download file {remoteFilePath}: {ex.Message}");
                return false;
            }
        }
    }
}
