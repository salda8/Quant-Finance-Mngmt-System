﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ServerGui.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.5.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("127.0.0.1")]
        public string ibClientHost {
            get {
                return ((string)(this["ibClientHost"]));
            }
            set {
                this["ibClientHost"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4001")]
        public int ibClientPort {
            get {
                return ((int)(this["ibClientPort"]));
            }
            set {
                this["ibClientPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string instrumentsGridLayout {
            get {
                return ((string)(this["instrumentsGridLayout"]));
            }
            set {
                this["instrumentsGridLayout"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5557")]
        public int RealTimeDataServerPublishPort {
            get {
                return ((int)(this["RealTimeDataServerPublishPort"]));
            }
            set {
                this["RealTimeDataServerPublishPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5556")]
        public int RealTimeDataServerRequestPort {
            get {
                return ((int)(this["RealTimeDataServerRequestPort"]));
            }
            set {
                this["RealTimeDataServerRequestPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5558")]
        public int instrumentServerPort {
            get {
                return ((int)(this["instrumentServerPort"]));
            }
            set {
                this["instrumentServerPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5555")]
        public int HistoricalServerPort {
            get {
                return ((int)(this["HistoricalServerPort"]));
            }
            set {
                this["HistoricalServerPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Logs\\")]
        public string logDirectory {
            get {
                return ((string)(this["logDirectory"]));
            }
            set {
                this["logDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string sqlServerHost {
            get {
                return ((string)(this["sqlServerHost"]));
            }
            set {
                this["sqlServerHost"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool sqlServerUseWindowsAuthentication {
            get {
                return ((bool)(this["sqlServerUseWindowsAuthentication"]));
            }
            set {
                this["sqlServerUseWindowsAuthentication"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string sqlServerUsername {
            get {
                return ((string)(this["sqlServerUsername"]));
            }
            set {
                this["sqlServerUsername"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string sqlServerPassword {
            get {
                return ((string)(this["sqlServerPassword"]));
            }
            set {
                this["sqlServerPassword"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int histClientIBID {
            get {
                return ((int)(this["histClientIBID"]));
            }
            set {
                this["histClientIBID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int rtdClientIBID {
            get {
                return ((int)(this["rtdClientIBID"]));
            }
            set {
                this["rtdClientIBID"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5554")]
        public int MessagesServerPullPort {
            get {
                return ((int)(this["MessagesServerPullPort"]));
            }
            set {
                this["MessagesServerPullPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5553")]
        public int MessagesServerPushPort {
            get {
                return ((int)(this["MessagesServerPushPort"]));
            }
            set {
                this["MessagesServerPushPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5552")]
        public int EquityUpdateServerRouterPort {
            get {
                return ((int)(this["EquityUpdateServerRouterPort"]));
            }
            set {
                this["EquityUpdateServerRouterPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string databaseType {
            get {
                return ((string)(this["databaseType"]));
            }
            set {
                this["databaseType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("data")]
        public string dataDatabaseName {
            get {
                return ((string)(this["dataDatabaseName"]));
            }
            set {
                this["dataDatabaseName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("server")]
        public string allPurposeDatabaseName {
            get {
                return ((string)(this["allPurposeDatabaseName"]));
            }
            set {
                this["allPurposeDatabaseName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5558")]
        public int InstrumetnUpdateRequestSocketPort {
            get {
                return ((int)(this["InstrumetnUpdateRequestSocketPort"]));
            }
            set {
                this["InstrumetnUpdateRequestSocketPort"] = value;
            }
        }
    }
}
