namespace awing_fullstack_test_backend.Models
{
    public class Input
    {
        public int Id { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Treasure { get; set; }
        public ICollection<MatrixElement> MatrixElements { get; set; }
        // thuộc tính navigation
        public Output Output { get; set; }
    }
}
