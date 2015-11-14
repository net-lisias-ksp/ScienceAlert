using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon.Logging;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
    class CommandLoadConfiguration : Command
    {
        private readonly ILog _log;

        public CommandLoadConfiguration(ILog log)
        {
            if (log == null) throw new ArgumentNullException("log");
            _log = log;
        }


        public override void Execute()
        {
            _log.Normal("LoadConfiguration Command executed");
        }
    }
}
