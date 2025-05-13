using System.ComponentModel.DataAnnotations;

namespace Kol1_APBD.Models.DTOs;

public class VisitInsertDTO
{
    
    [Required]
    public int visitId { get; set; }
    [Required]
    public int clientId { get; set; }
    [Required]
    public string mechanicLicenceNumber { get; set; }
    [Required]
    [MinLength(1, ErrorMessage = "At least one service is required.")]
    public List<ServiceInsertDTO> services { get; set; }
}

public class ServiceInsertDTO
{
    [Required(ErrorMessage = "Service name is required.")]
    public string serviceName { get; set; }
    [Range(0.01, double.MaxValue, ErrorMessage = "Service fee must be greater than zero.")]

    public decimal serviceFee { get; set; }
}