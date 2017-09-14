﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RequestServiceImpl.WcfSaveService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="WcfSaveService.ISaveService")]
    public interface ISaveService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISaveService/DownloadFile", ReplyAction="http://tempuri.org/ISaveService/DownloadFileResponse")]
        byte[] DownloadFile(int requestId, string fileName);
        
        // CODEGEN: Generating message contract since the wrapper name (FileUploadRequest) of message FileUploadRequest does not match the default value (UploadFile)
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISaveService/UploadFile", ReplyAction="http://tempuri.org/ISaveService/UploadFileResponse")]
        RequestServiceImpl.WcfSaveService.FileUploadResponse UploadFile(RequestServiceImpl.WcfSaveService.FileUploadRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="FileUploadRequest", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class FileUploadRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public string FileName;
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public long RequestId;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public System.IO.Stream FileStream;
        
        public FileUploadRequest() {
        }
        
        public FileUploadRequest(string FileName, long RequestId, System.IO.Stream FileStream) {
            this.FileName = FileName;
            this.RequestId = RequestId;
            this.FileStream = FileStream;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="FileUploadResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class FileUploadResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public string RetFileName;
        
        public FileUploadResponse() {
        }
        
        public FileUploadResponse(string RetFileName) {
            this.RetFileName = RetFileName;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ISaveServiceChannel : RequestServiceImpl.WcfSaveService.ISaveService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SaveServiceClient : System.ServiceModel.ClientBase<RequestServiceImpl.WcfSaveService.ISaveService>, RequestServiceImpl.WcfSaveService.ISaveService {
        
        public SaveServiceClient() {
        }
        
        public SaveServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public SaveServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SaveServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SaveServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public byte[] DownloadFile(int requestId, string fileName) {
            return base.Channel.DownloadFile(requestId, fileName);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        RequestServiceImpl.WcfSaveService.FileUploadResponse RequestServiceImpl.WcfSaveService.ISaveService.UploadFile(RequestServiceImpl.WcfSaveService.FileUploadRequest request) {
            return base.Channel.UploadFile(request);
        }
        
        public string UploadFile(string FileName, long RequestId, System.IO.Stream FileStream) {
            RequestServiceImpl.WcfSaveService.FileUploadRequest inValue = new RequestServiceImpl.WcfSaveService.FileUploadRequest();
            inValue.FileName = FileName;
            inValue.RequestId = RequestId;
            inValue.FileStream = FileStream;
            RequestServiceImpl.WcfSaveService.FileUploadResponse retVal = ((RequestServiceImpl.WcfSaveService.ISaveService)(this)).UploadFile(inValue);
            return retVal.RetFileName;
        }
    }
}
