namespace awing_fullstack_test_backend.DTO
{
    public class FindResultRequestDto
    {
        public int M { get; set; }
        public int N { get; set; }
        public int P { get; set; }
        public List<MatrixElementDto> MatrixElements { get; set; }
    }
}
