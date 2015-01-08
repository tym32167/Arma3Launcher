using System.Xml.Serialization;

namespace Arma3LauncherWPF.Config
{
    public class ProfileBase
    {
        public ProfileBase()
        {
            
        }

        public string ProfileName { get; set; }
        public ServerAddress ServerAddress { get; set; }
    }

    public class ServerAddress
    {
        [XmlAttribute]
        public string IP { get; set; }
        [XmlAttribute]
        public string Port { get; set; }
        [XmlAttribute]
        public string Password { get; set; }
    }
}