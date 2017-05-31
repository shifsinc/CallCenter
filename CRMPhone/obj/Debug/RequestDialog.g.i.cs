﻿#pragma checksum "..\..\RequestDialog.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "91FF30EAFCA2BAC84D3044C70E0FD221"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using CRMPhone;
using CRMPhone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace CRMPhone {
    
    
    /// <summary>
    /// RequestDialog
    /// </summary>
    public partial class RequestDialog : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 15 "..\..\RequestDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid RequestGrid;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\RequestDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox ContactListBox;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\RequestDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox RequestList;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/CRMPhone;component/requestdialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\RequestDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.RequestGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            
            #line 27 "..\..\RequestDialog.xaml"
            ((System.Windows.Controls.ComboBox)(target)).PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.tabItem_PreviewKeyDown);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 29 "..\..\RequestDialog.xaml"
            ((System.Windows.Controls.ComboBox)(target)).PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.tabItem_PreviewKeyDown);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 31 "..\..\RequestDialog.xaml"
            ((System.Windows.Controls.ComboBox)(target)).PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.tabItem_PreviewKeyDown);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 33 "..\..\RequestDialog.xaml"
            ((System.Windows.Controls.ComboBox)(target)).PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.tabItem_PreviewKeyDown);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ContactListBox = ((System.Windows.Controls.ListBox)(target));
            return;
            case 8:
            this.RequestList = ((System.Windows.Controls.ListBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            System.Windows.EventSetter eventSetter;
            switch (connectionId)
            {
            case 7:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.UIElement.PreviewMouseUpEvent;
            
            #line 55 "..\..\RequestDialog.xaml"
            eventSetter.Handler = new System.Windows.Input.MouseButtonEventHandler(this.SelectCurrentContact);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            case 9:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.UIElement.PreviewMouseUpEvent;
            
            #line 108 "..\..\RequestDialog.xaml"
            eventSetter.Handler = new System.Windows.Input.MouseButtonEventHandler(this.SelectCurrentRequest);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            }
        }
    }
}

