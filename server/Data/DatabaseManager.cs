﻿using System.Data.SqlClient;

public class DatabaseManager
{
    private readonly string _connectionString;

    public DatabaseManager(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}