namespace awing_fullstack_test_backend.DTO
{
    public class UpdateInputDto
    {
        public int Id { get; set; }
        public int? Rows { get; set; }
        public int? Columns { get; set; }
        public int? Treasure { get; set; }
        public List<MatrixElementDto>? MatrixElements { get; set; }
    }
}
