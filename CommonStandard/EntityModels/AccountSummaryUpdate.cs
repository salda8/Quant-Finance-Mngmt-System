using ProtoBuf;

namespace Common.EntityModels
{
    [ProtoContract]
    public class AccountSummaryUpdate
    {
        public AccountSummaryUpdate()
        {
            
        }

        public AccountSummaryUpdate(int id, string key, string value)
        {
            AccountID = id;
            Key = key;
            Value = value;
        }
        [ProtoMember(1)]
        public int AccountID { get; set; }
        [ProtoMember(2)]
        public string Key { get; set; }
        [ProtoMember(3)]
        public string Value { get; set; }
    }
}