namespace GMPS.API.DTOs
{

    //Model data respone by form restful api, include data, pagination and link for HATEOAS
    public class RestDTO<T>
    {
        public T Data { get; set; } = default!;
        public int? PageIndex { get; set; }

        public int? PageSize { get; set; }

        public int? RecordCount { get; set; }

        public List<LinkDTO> Links { get; set; } = new List<LinkDTO>();


    }

}
