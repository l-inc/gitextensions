﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18034
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GitUI.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Hotkeys {
            get {
                return ((string)(this["Hotkeys"]));
            }
            set {
                this["Hotkeys"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("215")]
        public int FormBrowse_FileTreeSplitContainer_SplitterDistance {
            get {
                return ((int)(this["FormBrowse_FileTreeSplitContainer_SplitterDistance"]));
            }
            set {
                this["FormBrowse_FileTreeSplitContainer_SplitterDistance"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("215")]
        public int FormBrowse_DiffSplitContainer_SplitterDistance {
            get {
                return ((int)(this["FormBrowse_DiffSplitContainer_SplitterDistance"]));
            }
            set {
                this["FormBrowse_DiffSplitContainer_SplitterDistance"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int Dashboard_MainSplitContainer_SplitterDistance {
            get {
                return ((int)(this["Dashboard_MainSplitContainer_SplitterDistance"]));
            }
            set {
                this["Dashboard_MainSplitContainer_SplitterDistance"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int Dashboard_CommonSplitContainer_SplitterDistance {
            get {
                return ((int)(this["Dashboard_CommonSplitContainer_SplitterDistance"]));
            }
            set {
                this["Dashboard_CommonSplitContainer_SplitterDistance"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("213")]
        public int FormBrowse_MainSplitContainer_SplitterDistance {
            get {
                return ((int)(this["FormBrowse_MainSplitContainer_SplitterDistance"]));
            }
            set {
                this["FormBrowse_MainSplitContainer_SplitterDistance"] = value;
            }
        }
    }
}
