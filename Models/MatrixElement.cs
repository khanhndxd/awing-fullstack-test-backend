using System.ComponentModel.DataAnnotations.Schema;

namespace awing_fullstack_test_backend.Models
{
    public class MatrixElement
    {
        public int Id { get; set; }
        public int InputId { get; set; }
        [ForeignKey(nameof(InputId))]
        public Input Input { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int Value { get; set; }
    }
}
