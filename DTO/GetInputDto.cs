namespace awing_fullstack_test_backend.DTO
{
    public class GetInputDto
    {
        public int Id { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Treasure { get; set; }
        public ICollection<MatrixElementDto> MatrixElements { get; set; }
        public GetOutputDto Output { get; set; }
    }
}
