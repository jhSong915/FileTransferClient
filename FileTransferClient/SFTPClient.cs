using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace FileTransferClient
{
    public class SFTPClient
    {
        public SftpClient _sftp_conn { get; private set; }
        public object _sftp_ip { get; private set; }
        public object _sftp_port { get; private set; }
        public object _sftp_userId { get; private set; }
        public object _sftp_password { get; private set; }

        public SFTPClient(object sftp_ip, object sftp_port, object sftp_userId, object sftp_password)
        {
            _sftp_ip = sftp_ip;
            _sftp_port = sftp_port;
            _sftp_userId = sftp_userId;
            _sftp_password = sftp_password;
            ConnectToServer();
        }
        
        ~SFTPClient() {
            try
            {
                if ( _sftp_conn != null )
                {
                    _sftp_conn.Disconnect();
                    _sftp_conn.Dispose();
                }
            }
            finally
            {
                _sftp_ip = null;
                _sftp_port = null;
                _sftp_userId = null;
                _sftp_password = null;
                _sftp_conn = null;
            }
        }

        public bool ConnectToServer()
        {
            try
            {
                _sftp_conn = new SftpClient((string)_sftp_ip, (int)_sftp_port, (string)_sftp_userId, (string)_sftp_password);
                _sftp_conn.Connect();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to Connect Server {_sftp_ip}:{_sftp_port} - {ex.Message}");
                return false;
            }
        }

        public void Download(string remotePath, string localPath, string orifilename)
        {
            DirectoryInfo di = new DirectoryInfo(localPath);
            if (di.Exists == false)
            {
                Directory.CreateDirectory(localPath);
            }
                
            IEnumerable<SftpFile> files = (IEnumerable<SftpFile>)_sftp_conn.ListDirectory(remotePath);
            foreach (SftpFile file in files)
            {
                if (file.Name.Equals(orifilename))
                {
                    if ((file.Name != ".") && (file.Name != ".."))
                    {
                        string sourceFilePath = remotePath + "/" + file.Name;
                        string destFilePath = Path.Combine(localPath, file.Name);
                        string dateFilePath = localPath + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + file.Name.Split('.')[0]; 

                        using (Stream fileStream = File.Create(destFilePath))
                        {
                            _sftp_conn.DownloadFile(sourceFilePath, fileStream);
                        }
                    }
                }
            }
        }

        public void Upload(string localPath, string remotePath, string orifilename)
        {
            IEnumerable<FileSystemInfo> infos = new DirectoryInfo(localPath).EnumerateFileSystemInfos();
            foreach (FileSystemInfo info in infos)
            {
                using (Stream fileStream = new FileStream(info.FullName, FileMode.Open))
                {
                    if (!_sftp_conn.Exists(remotePath))
                    {
                        _sftp_conn.CreateDirectory(remotePath);
                    }
                    _sftp_conn.UploadFile(fileStream, remotePath + "/" + info.Name);
                }
            }
        }
    }
}
