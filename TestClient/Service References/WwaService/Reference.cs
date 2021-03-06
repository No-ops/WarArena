﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestClient.WwaService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Statistics", Namespace="http://schemas.datacontract.org/2004/07/WwaWebServer")]
    [System.SerializableAttribute()]
    public partial class Statistics : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LastPlayerLoggedInField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PlayerWithMostGoldField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PlayerWithMostHealthField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int TotalNumberOfPlayersField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LastPlayerLoggedIn {
            get {
                return this.LastPlayerLoggedInField;
            }
            set {
                if ((object.ReferenceEquals(this.LastPlayerLoggedInField, value) != true)) {
                    this.LastPlayerLoggedInField = value;
                    this.RaisePropertyChanged("LastPlayerLoggedIn");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PlayerWithMostGold {
            get {
                return this.PlayerWithMostGoldField;
            }
            set {
                if ((object.ReferenceEquals(this.PlayerWithMostGoldField, value) != true)) {
                    this.PlayerWithMostGoldField = value;
                    this.RaisePropertyChanged("PlayerWithMostGold");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PlayerWithMostHealth {
            get {
                return this.PlayerWithMostHealthField;
            }
            set {
                if ((object.ReferenceEquals(this.PlayerWithMostHealthField, value) != true)) {
                    this.PlayerWithMostHealthField = value;
                    this.RaisePropertyChanged("PlayerWithMostHealth");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int TotalNumberOfPlayers {
            get {
                return this.TotalNumberOfPlayersField;
            }
            set {
                if ((this.TotalNumberOfPlayersField.Equals(value) != true)) {
                    this.TotalNumberOfPlayersField = value;
                    this.RaisePropertyChanged("TotalNumberOfPlayers");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="WwaService.IWwaWebServer")]
    public interface IWwaWebServer {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWwaWebServer/GetStats", ReplyAction="http://tempuri.org/IWwaWebServer/GetStatsResponse")]
        TestClient.WwaService.Statistics GetStats();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWwaWebServer/GetStats", ReplyAction="http://tempuri.org/IWwaWebServer/GetStatsResponse")]
        System.Threading.Tasks.Task<TestClient.WwaService.Statistics> GetStatsAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWwaWebServerChannel : TestClient.WwaService.IWwaWebServer, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WwaWebServerClient : System.ServiceModel.ClientBase<TestClient.WwaService.IWwaWebServer>, TestClient.WwaService.IWwaWebServer {
        
        public WwaWebServerClient() {
        }
        
        public WwaWebServerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WwaWebServerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WwaWebServerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WwaWebServerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public TestClient.WwaService.Statistics GetStats() {
            return base.Channel.GetStats();
        }
        
        public System.Threading.Tasks.Task<TestClient.WwaService.Statistics> GetStatsAsync() {
            return base.Channel.GetStatsAsync();
        }
    }
}
