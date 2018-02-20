using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.EntityModels
{
    public class Currency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// ISO 4217 3-letter currency code
        /// </summary>
        [MaxLength(3)]
        [Index(IsUnique = true)]
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// 3 digit numeric code, useful when currency codes need to be understood in countries that do not use Latin scripts and for computerised systems
        /// </summary>
        public int NumericCode { get; set; }
    }
}