using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections;
using System;

namespace ADFSExporter
{

    public static class Exporter
    {
        public static string Export(string adfsURL)
        {
            System.ServiceModel.WSHttpBinding binding = new System.ServiceModel.WSHttpBinding();
            binding.MaxReceivedMessageSize = 20971520;
            binding.ReaderQuotas.MaxStringContentLength = 20971520;
            binding.Security.Mode = System.ServiceModel.SecurityMode.Message; // This one line stops us being able to use .NET 6.0 >O

            PolicyStoreReadOnlyTransferClient client = new PolicyStoreReadOnlyTransferClient(binding, new System.ServiceModel.EndpointAddress(adfsURL));
            var result = client.GetState("ServiceSettings");
            client.Close();

            foreach (var property in result.PropertySets[0])
            {
                if (property.Name == "ServiceSettingsData")
                {
                    return property.Values[0] as string;
                }
            }

            return null;
        }
    }
    // DataContract definitions
    public interface IValueList : IList, ICollection, IEnumerable
    {
    }

    [DataContract(Name = "SearchResult", Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class SearchResultData
    {
        [DataMember]
        public PropertySetDataList PropertySets
        {
            get { return this._propertySetList; }
            set { this._propertySetList = value; }
        }
        private PropertySetDataList _propertySetList = new PropertySetDataList();
    }
    [CollectionDataContract(Name = "PropertySets", ItemName = "PropertySet", Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class PropertySetDataList : List<PropertySetData> { }
    [CollectionDataContract(Name = "PropertySet", ItemName = "Property", Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class PropertySetData : List<PropertyData> { }
    [CollectionDataContract(Name = "Values{0}", ItemName = "Value{0}", Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class PropertyValueList<T> : List<T>, IValueList, IList, ICollection, IEnumerable { }
    [DataContract(Name = "Property", Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    [KnownType(typeof(PropertyValueList<string>))]
    [KnownType(typeof(PropertyValueList<PropertySetData>))]
    public class PropertyData
    {
        public PropertyData() { }
        public PropertyData(string name) { this._name = name; }
        [DataMember(EmitDefaultValue = false, IsRequired = true)]
        public string Name { get { return this._name; } set { this._name = value; } }
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public IValueList Values { get { return this._values; } set { this._values = value; } }
        private string _name;
        private IValueList _values = new PropertyValueList<string>();
    }
    public enum SyncItemState
    {
        NotProcessed,
        Processing,
        Processed
    }
    [CollectionDataContract(Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class ServiceStateSummary : List<ServiceStateItem> { }
    [DataContract(Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class ServiceStateItem
    {
        public ServiceStateItem(string serviceObjectType, long serialNumber, int schemaVersionNumber, DateTime lastUpdateTime)
        {
            this._serviceObjectType = serviceObjectType;
            this._serialNumber = serialNumber;
            this._schemaVersionNumber = schemaVersionNumber;
            this._lastUpdateTime = lastUpdateTime;
            this.NeedsUpdate = false;
        }
        [DataMember]
        public string ServiceObjectType
        {
            get { return this._serviceObjectType; }
            set { this._serviceObjectType = value; }
        }
        [DataMember]
        public long SerialNumber
        {
            get { return this._serialNumber; }
            set { this._serialNumber = value; }
        }
        [DataMember]
        public int SchemaVersionNumber
        {
            get { return this._schemaVersionNumber; }
            set { this._schemaVersionNumber = value; }
        }
        [DataMember]
        public DateTime LastUpdateTime
        {
            get { return this._lastUpdateTime; }
            set { this._lastUpdateTime = value; }
        }
        public bool SyncComplete
        {
            get { return this._syncCompleted; }
            set { this._syncCompleted = value; }
        }
        public bool NeedsUpdate { get; set; }
        public SyncItemState ProcessingState
        {
            get { return this._processingState; }
            set { this._processingState = value; }
        }
        private string _serviceObjectType;
        private long _serialNumber;
        private int _schemaVersionNumber;
        private DateTime _lastUpdateTime;
        private SyncItemState _processingState;
        private bool _syncCompleted;
    }
    public enum FarmBehavior
    {
        Unsupported = -1,
        None,
        Win2012R2,
        Threshold,
        Win2016,
        Win2019
    }
    [DataContract(Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public enum FilterOperation
    {
        [EnumMember]
        And,
        [EnumMember]
        Or
    }
    [DataContract(Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public enum SimpleOperation
    {
        [EnumMember]
        Equals,
        [EnumMember]
        StartsWith,
        [EnumMember]
        EndsWith,
        [EnumMember]
        Contains,
        [EnumMember]
        NotEquals,
        [EnumMember]
        ScopeAppliesTo
    }
    [DataContract(Name = "If", Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class SimpleConditionData
    {
        public SimpleConditionData() { }
        public SimpleConditionData(string property, SimpleOperation operation, string value)
        {
            this._property = property;
            this._value = value;
            this._op = operation;
        }
        [DataMember(EmitDefaultValue = true, IsRequired = true, Order = 0)]
        public string Property { get { return this._property; } set { this._property = value; } }
        [DataMember(EmitDefaultValue = true, IsRequired = true, Order = 1)]
        public SimpleOperation Operation { get { return this._op; } set { this._op = value; } }
        [DataMember(EmitDefaultValue = true, IsRequired = true, Order = 2)]
        public string Value { get { return this._value; } set { this._value = value; } }
        private SimpleOperation _op;
        private string _property;
        private string _value;
    }
    [CollectionDataContract(ItemName = "If", Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class ConditionList : List<SimpleConditionData> { }
    [DataContract(Name = "Filter", Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore")]
    public class FilterData
    {
        public FilterData()
        {
        }
        public FilterData(FilterOperation operation) { this._bool = operation; }
        [DataMember(Name = "Conditions")]
        public ConditionList Conditions { get { return this._conditions; } set { this._conditions = value; } }
        [DataMember(Name = "Operation")]
        public FilterOperation Operation { get { return this._bool; } set { this._bool = value; } }
        private FilterOperation _bool;
        private ConditionList _conditions = new ConditionList();
    }

    // PolicyStoreReadOnlyTransfer definitions
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore", ConfigurationName = "AADInternals.IPolicyStoreReadOnlyTransfer")]
    public interface IPolicyStoreReadOnlyTransfer
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore/IPolicyStoreReadOnlyTransfer/GetState", ReplyAction = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore/IPolicyStoreReadOnlyTransfer/GetStateResponse")]
        SearchResultData GetState(string serviceObjectType, string mask = null, FilterData filter = null, int clientVersionNumber = 1);
        [System.ServiceModel.OperationContractAttribute(Action = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore/IPolicyStoreReadOnlyTransfer/GetHeaders", ReplyAction = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore/IPolicyStoreReadOnlyTransfer/GetHeadersResponse")]
        ServiceStateSummary GetHeaders();
        [System.ServiceModel.OperationContractAttribute(Action = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore/IPolicyStoreReadOnlyTransfer/GetFarmBehavior", ReplyAction = "http://schemas.microsoft.com/ws/2009/12/identityserver/protocols/policystore/IPolicyStoreReadOnlyTransfer/GetFarmBehaviorResponse")]
        FarmBehavior GetFarmBehavior();
    }

    public interface IPolicyStoreReadOnlyTransferChannel : ADFSExporter.IPolicyStoreReadOnlyTransfer, System.ServiceModel.IClientChannel
    {
    }
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    public partial class PolicyStoreReadOnlyTransferClient : System.ServiceModel.ClientBase<ADFSExporter.IPolicyStoreReadOnlyTransfer>, ADFSExporter.IPolicyStoreReadOnlyTransfer
    {
        public PolicyStoreReadOnlyTransferClient()
        {
        }
        public PolicyStoreReadOnlyTransferClient(string endpointConfigurationName) :
                base(endpointConfigurationName)
        {
        }
        public PolicyStoreReadOnlyTransferClient(string endpointConfigurationName, string remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }
        public PolicyStoreReadOnlyTransferClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }
        public PolicyStoreReadOnlyTransferClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
        {
        }
        public SearchResultData GetState(string serviceObjectType, string mask = null, FilterData filter = null, int clientVersionNumber = 1)
        {
            return base.Channel.GetState(serviceObjectType, mask, filter, clientVersionNumber);
        }
        public ServiceStateSummary GetHeaders()
        {
            return base.Channel.GetHeaders();
        }
        public FarmBehavior GetFarmBehavior()
        {
            return base.Channel.GetFarmBehavior();
        }
    }
}

