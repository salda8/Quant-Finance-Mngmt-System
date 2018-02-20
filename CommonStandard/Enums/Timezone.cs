using System.ComponentModel;

namespace Common.Enums
{
    public enum Timezone : byte
    {
      
        [Description("Time on exchange")]
        Exchange = 0,
        
        [Description("UTC time")]
        Utc = 1,
       
        [Description("Local time")]
        Local = 2
    }
}