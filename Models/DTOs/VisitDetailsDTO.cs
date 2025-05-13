namespace Kol1_APBD.Models.DTOs;

public class VisitDetailsDTO
{
    public DateTime date { get; set; }
    public ClientDTO client { get; set; }
    public MechanicDTO mechanic { get; set; }
    public List<Visit_ServiceDTO> visitServices { get; set; }
    
}

public class ClientDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public DateTime dateOfBirth { get; set; }
}

public class MechanicDTO
{
    public int mechanicId { get; set; }
    public string licenceNumber { get; set; }
}

public class Visit_ServiceDTO
{
    public string name { get; set; }
    public decimal serviceFee { get; set; }
}