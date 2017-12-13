using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace connectingToDBTESTING.Models
{
    public class Comment : BaseEntity
    {
        [Key]
        public long CommentId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreatedAt { get; set; }

        [Required(ErrorMessage = "Please enter anything.")]
        [StringLength(225, ErrorMessage = "Message must be between 3 and 225 characters", MinimumLength = 3)]
        [Display(Name = "Message:")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Where is you goddam id?")]
        [Display(Name = "user_id:")]
        public int UserId { get; set; }
        public User User {get;set;}
        
        public int MessageId { get; set; }

    }
}