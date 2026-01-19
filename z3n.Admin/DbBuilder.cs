using System.Collections.Generic;
using ZennoLab.InterfacesLibrary.ProjectModel;
using System.Linq;
using System;
using NBitcoin;
using Newtonsoft.Json;


namespace z3nCore
{
    public static partial class Admin
    {
  
        #region Templates

        public static void UpdateTemplates(this IZennoPosterProjectModel project)
        {
            project.Var("DBmode","PostgreSQL");
            var db = new z3nCore.Db(project);
            var tables = db.GetTables();
            var fullStructure = new Dictionary<string, List<string>>();
            foreach (var table in tables)
            {
                if (table.StartsWith("__")) continue;
                var columns = db.GetTableColumns(table);
                fullStructure.Add(table, columns);
            }
            string jsonStructure = JsonConvert.SerializeObject(fullStructure);
            System.IO.File.WriteAllText("w:/code_hard/.net/z3nCore/.templates/db_template.json", jsonStructure);
            
            
            string tableName = "_api";
            var allColumns = project.TblColumns(tableName, true);
            var serviceColumns = new HashSet<string> { "id", "_json_structure" };
            var dataColumns = allColumns
                .Where(col => !serviceColumns.Contains(col.ToLower()))
                .ToList();

            string allIdsRaw = project.DbGet("id", tableName, where: "1=1");
            if (string.IsNullOrEmpty(allIdsRaw)) {
                project.SendErrorToLog("Таблица пуста или не найдена");
                return ;
            }

            var ids = allIdsRaw.Split('·').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var resultTemplate = new Dictionary<string, List<string>>();
            string columnsString = string.Join(",", dataColumns);

            foreach (var id in ids)
            {
                var rowData = project.SqlGetDicFromLine(columnsString, tableName, key: "id", where: $"\"id\" = '{id.Trim()}'");
                if (rowData == null || rowData.Count == 0) continue;
                var valuesTemplate = new List<string>();
                bool hasData = false;
                foreach (var col in dataColumns)
                {
                    if (rowData.ContainsKey(col) && !string.IsNullOrWhiteSpace(rowData[col]))
                    {
                        valuesTemplate.Add("REQUIRED");
                        hasData = true;
                    }
                    else
                    {
                        valuesTemplate.Add("");
                    }
                }

                if (hasData)
                {
                    resultTemplate.Add(id.Trim(), valuesTemplate);
                }
            }

            string apiStructure = JsonConvert.SerializeObject(resultTemplate, Formatting.Indented);
            System.IO.File.WriteAllText("w:/code_hard/.net/z3nCore/.templates/api_template.json", apiStructure);
        }

        #endregion
        
    }

    
}