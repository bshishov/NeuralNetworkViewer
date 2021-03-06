﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace NeuralNetworkTestUI.Logging
{
    class NLogLogger : ILog
    {
        private readonly NLog.Logger _innerLogger;

        public NLogLogger(Type type)
        {
            _innerLogger = NLog.LogManager.GetLogger(type.FullName);
        }

        public void Info(string format, params object[] args)
        {
            _innerLogger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            _innerLogger.Warn(format, args);
        }

        public void Error(Exception exception)
        {
            _innerLogger.ErrorException(exception.Message, exception);
        }
    }
}
