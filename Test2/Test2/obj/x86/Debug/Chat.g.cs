﻿#pragma checksum "C:\Users\Hemran\Desktop\Chatr2\Test2\Test2\Chat.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "225EF79A4E0F3BAF2BF9336E2FF88539"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Test2
{
    partial class Chat : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // Chat.xaml line 123
                {
                    this.send = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.send).Click += this.send_Click;
                }
                break;
            case 3: // Chat.xaml line 134
                {
                    this.text = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 4: // Chat.xaml line 87
                {
                    this.lv = (global::Windows.UI.Xaml.Controls.ListView)(target);
                }
                break;
            case 7: // Chat.xaml line 64
                {
                    this.activeLobbies = (global::Windows.UI.Xaml.Controls.ListBox)(target);
                }
                break;
            case 8: // Chat.xaml line 34
                {
                    this.joinLobby = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.joinLobby).Click += this.joinLobby_Click;
                }
                break;
            case 9: // Chat.xaml line 45
                {
                    this.createLobby = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.createLobby).Click += this.createLobby_Click;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

