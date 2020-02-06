using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// This entity has all data types on it
    /// </summary>
    [Table("BigDbEntity")]
    public class BigEntity : IEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [PrimaryKey]
        public int ID { get; protected set; }

        [MaxLength(50)]
        [Required]
        [Column("JustAnotherName")]
        public string Name { get; set; }

        public double MyDouble { get; set; }
        
        public float MyFloat { get; set; }
        
        [Nullable(false)]
        public Single MySingle { get; set; }
        
        [Nullable(false)]
        public DateTime MyDateTime { get; set; }
        
        public decimal MyDecmial { get; set; }
        
        public char MyChar { get; set; }
        
        public bool MyBool { get; set; }
        
        public short MyShort { get; set; }
        
        public byte[] MyByteArray { get; set; }
        
        public Int16 MyInt16 { get; set; }
        
        [BlobField]
        [Nullable(false)]
        public string MyBlob1 { get; set; }
        
        [BlobField]
        [Unicode]
        public string MyBlob2 { get; set; }
        
        [XmlField]
        public string MyXml { get; set; }
    }
}