﻿namespace DbFileUploaderDataAccessLibrary.Data;

public interface IUploaderData
{
    Task DeleteTableData(string tableName);
    Task<int> SaveRow(dynamic parameters);
}