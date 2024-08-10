using System.ComponentModel.DataAnnotations.Schema;

namespace awing_fullstack_test_backend.Models
{
    public class Output
    {
        public int Id { get; set; }
        public int InputId { get; set; }
        [ForeignKey(nameof(InputId))]
        public Input Input { get; set; }
        public double Result { get; set; }
        public string PathInfo { get; set; }
    }
}
