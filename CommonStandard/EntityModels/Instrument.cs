using Common.Enums;
using Common.Interfaces;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DataAccess.EntityConfigs;

namespace Common.EntityModels
{
    [ProtoContract]
    [ProtoInclude(29, typeof(ExpirationRule))]
    [ProtoInclude(23, typeof(Datasource))]
    [ProtoInclude(21, typeof(Exchange))]
    public class Instrument : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ProtoMember(20)]
        public int ID { get; set; }

        [ProtoMember(1)]
        [MaxLength(100)]
        public string Symbol { get; set; }

        [ProtoMember(2)]
        [MaxLength(255)]
        public string UnderlyingSymbol { get; set; }

        [ProtoMember(89)]
        [MaxLength(255)]
        public string Name { get; set; }

        [ProtoMember(4)]
        public int ExchangeID { get; set; }

        [ProtoMember(5)]
        public InstrumentType Type { get; set; }

        public DateTime Expiration
        {
            get { return new DateTime(expirationYear, expirationMonth, expirationDay); }

            set
            {
                expirationYear = value.Year;
                expirationMonth = value.Month;
                expirationDay = value.Day;
            }
        }

        [ProtoMember(7)] [NotMapped] [NonSerialized] private int expirationYear;

        [ProtoMember(8)] [NotMapped] [NonSerialized] private int expirationMonth;

        [ProtoMember(9)] [NotMapped] [NonSerialized] private int expirationDay;

        [ProtoMember(12)]
        [MaxLength(25)]
        public string Currency { get; set; }

        [ProtoMember(18)]
        public string ValidExchanges { get; set; }

        [ProtoMember(19)]
        public virtual ICollection<Tag> Tags { get; set; }

        [ProtoMember(90)]
        
        public int? DatasourceID { get; set; }

        [ProtoMember(21)]
        public virtual Exchange Exchange { get; set; }

        [ProtoMember(23)]
        public virtual Datasource Datasource { get; set; }

        public SessionsSource SessionsSource { get; set; }

        [ProtoMember(26)]
        public virtual ICollection<InstrumentSession> Sessions { get; set; }
        [ProtoMember(27)]
        public int SessionTemplateID { get; set; }

        [ProtoMember(29)]
        public virtual ExpirationRule ExpirationRule { get; set; }

        public int ExpirationRuleID { get; set; }

        [NotMapped]
        public string TagsAsString
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var tag in Tags)
                {

                    sb.Append(tag.Name);
                    sb.Append(" ");

                }

                return sb.ToString();

            }
            
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("ID: " + ID);

            if (!string.IsNullOrEmpty(Symbol))
                sb.Append(" Symbol: " + Symbol);

            if (!string.IsNullOrEmpty(UnderlyingSymbol))
                sb.Append(" Underlying: " + UnderlyingSymbol);

            sb.Append(" Type: " + Type);

            sb.Append(" Exp: " + Expiration.ToString("dd-MM-yyyy"));

            if (Exchange != null)
                sb.Append(" Exch: " + Exchange.Name);

            if (Datasource != null)
                sb.Append(" DS: " + Datasource.Name);

            if (!string.IsNullOrEmpty(Currency))
                sb.Append($"({Currency})");

            return sb.ToString().Trim();
        }
    }
}