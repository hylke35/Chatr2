﻿#pragma checksum "C:\Users\Hemran\Desktop\Chatr2\Test2\Test2\VideoLobby.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7183839AA6BCB72D29F49B864AE132A0"
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
    partial class VideoLobby : 
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
            case 2: // VideoLobby.xaml line 12
                {
                    this.linkBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 3: // VideoLobby.xaml line 13
                {
                    this.addButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.addButton).Click += this.addButton_Click;
                }
                break;
            case 4: // VideoLobby.xaml line 14
                {
                    this.userList = (global::Windows.UI.Xaml.Controls.ListBox)(target);
                }
                break;
            case 5: // VideoLobby.xaml line 15
                {
                    this.videoList = (global::Windows.UI.Xaml.Controls.ListBox)(target);
                    ((global::Windows.UI.Xaml.Controls.ListBox)this.videoList).DoubleTapped += this.videoList_DoubleTapped;
                }
                break;
            case 6: // VideoLobby.xaml line 18
                {
                    this.readyButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.readyButton).Click += this.readyButton_Click;
                }
                break;
            case 7: // VideoLobby.xaml line 19
                {
                    global::Windows.UI.Xaml.Controls.TextBlock element7 = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBlock)element7).SelectionChanged += this.TextBlock_SelectionChanged;
                }
                break;
            case 8: // VideoLobby.xaml line 20
                {
                    this.lobbyCodeLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 9: // VideoLobby.xaml line 21
                {
                    this.leaveButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.leaveButton).Click += this.leaveButton_Click;
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

