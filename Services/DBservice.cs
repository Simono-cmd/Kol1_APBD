using Kol1_APBD.Models.DTOs;

namespace Kol1_APBD.Services;
using Microsoft.Data.SqlClient;
using Kol1_APBD.Models;
using System.Data.Common;
public class DBservice : IDBservice
{
    private readonly IConfiguration _configuration;
    public DBservice(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    // await transaction.CommitAsync(); -do commitowania zmian do bazy
    // execute scalar - zwraca liczbę, stałą (object)
    // execute reader - do selecta
    // execute nonquery - do insert update delete


    public async Task<VisitDetailsDTO?> GetVisitById(int id)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        

        VisitDetailsDTO? visit = null;
        
            command.CommandText =
                @"
            SELECT 
                v.date,
                c.first_name, 
                c.last_name, 
                c.date_of_birth,
                m.mechanic_id,
                m.licence_number,
                s.name AS service_name,
                serv.service_fee
            FROM Visit v
            JOIN Client c ON v.client_id = c.client_id
            JOIN Mechanic m ON m.mechanic_id = v.mechanic_id
            LEFT JOIN Visit_Service serv ON v.visit_id = serv.visit_id
            LEFT JOIN Service s ON serv.service_id = s.service_id
            WHERE v.visit_id = @id";
             
            command.Parameters.AddWithValue("@id", id);
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (visit == null)
                {
                    visit = new VisitDetailsDTO
                    {
                        date = reader.GetDateTime(reader.GetOrdinal("date")),
                        client = new ClientDTO()
                        {
                            firstName = reader.GetString(reader.GetOrdinal("first_name")),
                            lastName = reader.GetString(reader.GetOrdinal("last_name")),
                            dateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                        },
                        mechanic = new MechanicDTO()
                        {
                            mechanicId = reader.GetInt32(reader.GetOrdinal("mechanic_id")),
                            licenceNumber = reader.GetString(reader.GetOrdinal("licence_number")),
                        },
                        visitServices = new List<Visit_ServiceDTO>()
                    };
                }

                if (!reader.IsDBNull(reader.GetOrdinal("service_name")) &&
                    !reader.IsDBNull(reader.GetOrdinal("service_fee")))
                {
                    visit.visitServices.Add(new Visit_ServiceDTO()
                    {
                        name = reader.GetString(reader.GetOrdinal("service_name")),
                        serviceFee = reader.GetDecimal(reader.GetOrdinal("service_fee"))
                    });
                }
            }

            return visit;

    }

    public async Task<(bool success, string? message)> AddVisit(VisitInsertDTO visit)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        await using SqlTransaction transaction = connection.BeginTransaction();

        try
        {
            var checkCmd = new SqlCommand("SELECT 1 FROM Visit WHERE visit_id = @id", connection, transaction);
            checkCmd.Parameters.AddWithValue("@id", visit.visitId);
            if (await checkCmd.ExecuteScalarAsync() != null)
                return (false, "Visit with this ID already exists.");
            
            var clientCmd = new SqlCommand("SELECT 1 FROM Client WHERE client_id = @id", connection, transaction);
            clientCmd.Parameters.AddWithValue("@id", visit.clientId);
            if (await clientCmd.ExecuteScalarAsync() == null)
                return (false, "A client with this ID doesn't exist");
            
            var mechanicCmd = new SqlCommand("SELECT mechanic_id FROM Mechanic WHERE licence_number = @num", connection, transaction);
            mechanicCmd.Parameters.AddWithValue("@num", visit.mechanicLicenceNumber);
            var mechanicExist = await mechanicCmd.ExecuteScalarAsync();
            if (mechanicExist == null)
                return (false, "Mechanic with this licence number doesnt exist.");
            int mechanicID = (int)mechanicExist;
            
            
            var serviceIds = new Dictionary<string, int>();
            foreach (var s in visit.services)
            {
                var serviceCmd = new SqlCommand("SELECT service_id FROM Service WHERE name = @name", connection, transaction);
                serviceCmd.Parameters.AddWithValue("@name", s.serviceName);
                var serviceIdObj = await serviceCmd.ExecuteScalarAsync();
                if (serviceIdObj == null)
                    return (false, $"Service '{s.serviceName}' not found.");
                serviceIds[s.serviceName] = (int)serviceIdObj;
            }
            
            // insert visit
            var insertCmd = new SqlCommand(@"
            INSERT INTO Visit (visit_id, client_id, mechanic_id, date)
            VALUES (@id,@client ,@mechanic, @date)", connection, transaction);
            insertCmd.Parameters.AddWithValue("@id", visit.visitId);
            insertCmd.Parameters.AddWithValue("@client", visit.clientId);
            insertCmd.Parameters.AddWithValue("@mechanic", mechanicID);
            insertCmd.Parameters.AddWithValue("@date", DateTime.Now.AddDays(7));
            await insertCmd.ExecuteNonQueryAsync();
            
            // insert visit service
            foreach (var s in visit.services)
            {
                var insertServiceCmd = new SqlCommand(@"
                INSERT INTO Visit_Service (visit_id, service_id, service_fee)
                VALUES (@visitId, @serviceId, @serviceFee)", connection, transaction);
                insertServiceCmd.Parameters.AddWithValue("@visitId", visit.visitId);
                insertServiceCmd.Parameters.AddWithValue("@serviceId", serviceIds[s.serviceName]);
                insertServiceCmd.Parameters.AddWithValue("@serviceFee", s.serviceFee);
                await insertServiceCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return (true, "Added successfully.");
            
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return (false, "Internal error: " + e.Message);
            throw;
        }
    } 
    
    
    
}