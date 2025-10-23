using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBoards.Entities
{
    public class Epic : WorkItem
    {
        public DateTime? StartDate { get; set; }
        //[Precision(3)]
        public DateTime? EndDate { get; set; }
    }

    public class Issue : WorkItem
    {
        //[Column(TypeName = "decimal(5,2)")]
        public decimal Efford {  get; set; }
    }

    public class Task: WorkItem
    {
        //[MaxLength(200)]
        public string Activity { get; set; }
        //[Precision(14,2)]
        public decimal RemaningWork { get; set; }
    }
    public abstract class WorkItem
    {
        public int Id { get; set; }
        public string Area { get; set; }

        public string IterationPath { get; set; }
        public int Priority { get; set; }

        public virtual List<Comment> Comments { get; set; } = new List<Comment>();

        public virtual User Author { get; set; }
        public Guid AuthorId { get; set; }

        public virtual List<Tag> Tags { get; set; }

        public virtual State State { get; set; }
        public int StateId { get; set; }
        //public List<WorkItemTag> WorkItemTags { get; set; } = new List<WorkItemTag>();
    }
}
