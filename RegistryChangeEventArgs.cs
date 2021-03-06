﻿using System;

namespace RegistryEnforcer
{
    public class RegistryChangeEventArgs : EventArgs
    {
        #region Fields
        private bool _stop;
        private Exception _exception;
        private RegistryChangeMonitor _monitor;
        #endregion

        #region Constructor
        public RegistryChangeEventArgs(RegistryChangeMonitor monitor)
        {
            this._monitor = monitor;
        }
        #endregion

        #region Properties
        public RegistryChangeMonitor Monitor
        {
            get { return this._monitor; }
        }

        public Exception Exception
        {
            get { return this._exception; }
            set { this._exception = value; }
        }

        public bool Stop
        {
            get { return this._stop; }
            set { this._stop = value; }
        }
        #endregion
    }
}
