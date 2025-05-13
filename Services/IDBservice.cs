namespace Kol1_APBD.Services;
using Kol1_APBD.Models.DTOs;

public interface IDBservice
{
    Task<VisitDetailsDTO?> GetVisitById(int id);
    Task<(bool success, string? message)> AddVisit(VisitInsertDTO visit);
}