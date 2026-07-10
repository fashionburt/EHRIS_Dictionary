using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("zbufferRef")]
    public class ZbufferRef
    {
        /// <summary>
        /// id
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// kind
        /// </summary>
        [Required]
        [Column("kind")]
        [MaxLength(20)]
        public string Kind { get; set; }

        /// <summary>
        /// refKey
        /// </summary>
        [Required]
        [Column("refKey")]
        [MaxLength(50)]
        public string RefKey { get; set; }

        /// <summary>
        /// val
        /// </summary>
        [Column("val")]
        public string Val { get; set; }

        /// <summary>
        /// note
        /// </summary>
        [Column("note")]
        [MaxLength(100)]
        public string Note { get; set; }

        /// <summary>
        /// created
        /// </summary>
        [Column("created")]
        public DateTime? Created { get; set; }
    }
}