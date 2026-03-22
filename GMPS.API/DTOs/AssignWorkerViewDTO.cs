namespace GMPS.API.DTOs
{
    public class DataAssignWorkerViewDTO
    {

        public WorkerInfor WorkerInfo { get; set; }
        public IEnumerable<WorkerSkillInfo> WorkerSkillInfo { get; set; }
        public IEnumerable<WorerLRInfo> WorkerLrInfo { get; set; }


    }

    public class WorkerInfor
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; }
    }

    public class WorkerSkillInfo
    {
        public string SkillName { get; set; }
    }
    
    public class WorerLRInfo     {
        public DateTime DateLR { get; set; }
    }

}
