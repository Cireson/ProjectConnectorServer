﻿#pragma checksum "..\..\..\Forms\PWAWizardPage.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2CD34731C424BFCA7BF2F07279DB1BD9A57108D7"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Classes;
using Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms;
using Microsoft.EnterpriseManagement.UI.WpfWizardFramework;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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


namespace Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF.Forms {
    
    
    /// <summary>
    /// PWAWizardPage
    /// </summary>
    public partial class PWAWizardPage : Microsoft.EnterpriseManagement.UI.WpfWizardFramework.WizardRegularPageBase, System.Windows.Markup.IComponentConnector {
        
        
        #line 36 "..\..\..\Forms\PWAWizardPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtProjectURL;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\Forms\PWAWizardPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid credentialsPanel;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\Forms\PWAWizardPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnTestConnection;
        
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
            System.Uri resourceLocater = new System.Uri("/Microsoft.EnterpriseManagement.ServiceManager.ProjectServer.WPF;component/forms/" +
                    "pwawizardpage.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Forms\PWAWizardPage.xaml"
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
            this.txtProjectURL = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.credentialsPanel = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.btnTestConnection = ((System.Windows.Controls.Button)(target));
            
            #line 42 "..\..\..\Forms\PWAWizardPage.xaml"
            this.btnTestConnection.Click += new System.Windows.RoutedEventHandler(this.btnTestConnection_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
