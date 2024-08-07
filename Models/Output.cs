using System.ComponentModel.DataAnnotations.Schema;

namespace awing_fullstack_test_backend.Models
{
    public class Output
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Input))]
        public int InputId { get; set; }
        public double Result { get; set; }
    }
}
