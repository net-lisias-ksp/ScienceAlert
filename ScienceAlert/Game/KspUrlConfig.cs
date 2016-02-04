using System;

namespace ScienceAlert.Game
{
    public class KspUrlConfig : IUrlConfig
    {
        private readonly UrlDir.UrlConfig _config;

        public KspUrlConfig(UrlDir.UrlConfig config)
        {
            if (config == null) throw new ArgumentNullException("config");
            _config = config;
        }

        public string Url
        {
            get { return _config.url; }
        }

        public ConfigNode Config
        {
            get { return _config.config; }
        }
    }
}
