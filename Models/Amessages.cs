using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace connectingToDBTESTING.Models
{
    public class Amessage : BaseEntity
    {
        public Amessage(){
            Acomments = new List<Acomment>();
        }
        [Key]
        public int AmessageId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreatedAt { get; set; }

        [Required(ErrorMessage = "Please enter anything.")]
        [StringLength(225, ErrorMessage = "Message must be between 3 and 225 characters", MinimumLength = 3)]
        [Display(Name = "Message:")]
        public string Text { get; set; }
        public int PostedToId { get; set; }
        public int UserId { get; set; }
        public User User {get;set;}
        public List<Acomment> Acomments { get; set; }

    }
}