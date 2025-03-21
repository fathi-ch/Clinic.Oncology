﻿using System.Text;
using Clinic.Core.Contracts;
using Clinic.Core.Data;
using Clinic.Core.Dto;
using Clinic.Core.Mappers;
using Clinic.Core.Models;
using Dapper;

namespace Clinic.Core.Services;

public class PatientRepository : IPatientRepository
{
    private readonly ISqliteDbConnectionFactory _connectionFactory;

    public PatientRepository(ISqliteDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PatientResponse> CreateAsync(PatientDto patientDto)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(
                "Patients (FirstName, LastName, BirthDate, NextAppointment, Gender, Mobile, SocialSecurityNumber, Referral,Email) ");
            sb.Append(
                "VALUES (@FirstName, @LastName, @BirthDate, @NextAppointment, @Gender, @Mobile, @SocialSecurityNumber, @Referral,@Email); ");
            sb.Append("SELECT last_insert_rowid();");
            var query = sb.ToString();

            var newId = await connection.ExecuteScalarAsync<int>(query,
                new
                {
                    patientDto.FirstName,
                    patientDto.LastName,
                    patientDto.BirthDate,
                    patientDto.NextAppointment,
                    patientDto.Gender,
                    patientDto.Mobile,
                    patientDto.SocialSecurityNumber,
                    patientDto.Referral,
                    patientDto.Email
                });

            transaction.Commit();

            return patientDto.ToPatientResponse(newId);
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<PatientResponse>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var sb = new StringBuilder();
        sb.Append("SELECT * ");
        sb.Append("FROM Patients p ");

        var query = sb.ToString();
        var result = await connection.QueryAsync<Patient>(query);
        return result.ToPatientResponse();
    }

    public async Task<IEnumerable<PatientResponse>> GetAllByNameAsync(string name)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var sb = new StringBuilder()
            .Append("SELECT * ")
            .Append("FROM Patients p ");


        return (await connection.QueryAsync<Patient>(sb.ToString()))
               .Where(patient =>
                   patient.FirstName.ToLower().Contains(name.ToLower()))
                   .ToPatientResponse()
               ??
               Enumerable.Empty<PatientResponse>();
    }

    public async Task<PatientResponse?> GetByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        var sb = new StringBuilder();
        sb.Append("SELECT * ");
        sb.Append("FROM Patients p ");
        sb.Append("WHERE p.Id = @PatientId;");

        var query = sb.ToString();
        var result = await connection.QueryFirstOrDefaultAsync<Patient>(
            query,
            new { PatientId = id });

        return result.ToPatientResponse();
    }

    public async Task<PatientResponse> DeleteByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();

        var patientToDelete = await GetByIdAsync(id);

        if (patientToDelete == null)
        {
            return null;
        }

        var sb = new StringBuilder();
        sb.Append("DELETE ");
        sb.Append("FROM Patients ");
        sb.Append("WHERE Id = @PatientId;");

        var query = sb.ToString();
        var affectedPatientRows = await connection.ExecuteAsync(
            query,
            new { PatientId = id });

        return patientToDelete;
    }

    public async Task<PatientResponse> UpdateByIdAsync(int id, PatientDto patientDto)
    {
        using var connection = await _connectionFactory.CreateDbConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sb = new StringBuilder();
            sb.Append("UPDATE Patients SET ");
            sb.Append("FirstName = @FirstName, ");
            sb.Append("LastName = @LastName, ");
            sb.Append("BirthDate = @BirthDate, ");
            sb.Append("NextAppointment = @NextAppointment, ");
            sb.Append("Gender = @Gender, ");
            sb.Append("Mobile = @Mobile, ");
            sb.Append("SocialSecurityNumber = @SocialSecurityNumber, ");
            sb.Append("Referral = @Referral ,");
            sb.Append("Email = @Email ");
            sb.Append("WHERE Id = @id;");

            var query = sb.ToString();

            await connection.ExecuteAsync(query,
                new
                {
                    id = id,
                    FirstName = patientDto.FirstName,
                    LastName = patientDto.LastName,
                    BirthDate = patientDto.BirthDate,
                    NextAppointment = patientDto.NextAppointment,
                    Gender = patientDto.Gender,
                    Mobile = patientDto.Mobile,
                    SocialSecurityNumber = patientDto.SocialSecurityNumber,
                    Referral = patientDto.Referral,
                    Email= patientDto.Email
                });

            transaction.Commit();

            return patientDto.ToPatientResponse(id);
        }
        catch (Exception e)
        {
            transaction.Rollback();
            throw;
        }
    }
}