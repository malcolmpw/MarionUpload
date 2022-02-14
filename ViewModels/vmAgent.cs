using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dapper;
using Dapper.Contrib;
using GalaSoft.MvvmLight.Command;
using MarionUpload.Models;
using MarionDistributeImport.Models;


namespace MarionUpload.ViewModels
{
    public class vmAgent
    {
        static readonly string Path = @"C:\Users\malcolm.wardlaw\Desktop\Marion Download\MARION CAD FINAL MINERAL DATA\MA215500.TXT";
        const string ConnectionString = @"Data Source=WAGSQLSRV01\DEV;Initial Catalog=wagapp2_2021_Marion;Integrated Security=True;";
        //private const string AgentErrorPath = @"c:\temp\marion_agents_not_found.txt";

        public vmAgent()
        {
            MarionAgents = new ObservableCollection<mAgent>();

        }
        public ObservableCollection<mAgent> MarionAgents { get; set; }
        private mAgent Agent { get; set; }


        public ICommand CommandImportAgents => new RelayCommand(DelegateImportAgents);
        public void DelegateImportAgents()       // note: following WagApp1, a segment is the same as an OwnerPersonalPropertySegment
        {
            ReadMarionAgentsFlatFileIntoMarionAgents();
            // GetMatchingTblNameData(); // NOT USED

        }

        public ICommand CommandUploadAgentIDs => new RelayCommand(DelegateUploadAgentIDs);
        private void DelegateUploadAgentIDs()
        {
            // By searching in WagApp2 NameSelect and PrimaryData,
            // I added NameIDs from tblName to the AbMarionAgents table, and
            // I inserted new agents into tblName when none were found.

            // Now I will collect all the Marion AgentIDs and NameIDs
            GetMarionAgentIDs();
            // And insert the matching pairs into tblCadOwner
            InsertMarionAgentNumbers(marionAgents);
        }

        public List<mMarionAgent> marionAgents;
        private List<mMarionAgent> GetMarionAgentIDs()
        {
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                // get marion agents
                var sql = "Select * from AbMarionAgents";
                marionAgents = db.Query<mMarionAgent>(sql).ToList();
            }
            return marionAgents;
        }

        public static void InsertMarionAgentNumbers(List<mMarionAgent> agents)
        {
            // insert marion agent numbers and nameIDs into tblCadOwner            
            using (IDbConnection db = new SqlConnection(ConnectionString))
            {
                string sqlQuery = "Insert Into tblCadOwners(NameID, CadID, CadOwnerID, delflag) Values(@NameID, @CadID, @CadOwnerID, @delflag)";
                foreach (mMarionAgent m in agents)
                {
                    var agentInsert = new mCadOwner()
                    {
                        NameID = m.NameId,
                        CadID = "MAR",
                        CadOwnerID = m.AgentId,
                        delflag = false
                    };                        
                    db.Execute(sqlQuery, agentInsert);
                }
            }
        }

        //static List<(string reason, string agentName, string agentZip, string zip4)> _listOfErrorAgents = new List<(string, string, string, string)>();

        //private void GetMatchingTblNameData()
        //{
        //********************* this method was abandoned in favor of manually adding the needed agents to tblName and ***************************
        //********************* using WagApp2 to find the NameID in tblName of agent matches and recording these NameIDs in dbo.AbMarionAgents ***

        //using (IDbConnection db = new SqlConnection(ConnectionString))
        //{
        //    foreach (mAgent agent in MarionAgents)
        //    {
        //        GetNameidAndAgentname(db, agent, agent.AgentName, agent.Zip, agent.Zip4);
        //    }

        //    using (StreamWriter sw = new StreamWriter(AgentErrorPath, false))
        //    {
        //        foreach (var error in _listOfErrorAgents)
        //        {
        //            sw.WriteLine($"{error.reason} - agent name:{error.agentName} Zip:{error.agentZip} Zip4:{error.zip4}");
        //        }
        //    }
        //}
        //}


        //private static mAgent GetNameidAndAgentname(IDbConnection db, mAgent _agent, string _agentName, string _agentZip, string zip4)
        //{
        //    var parameters = new { AgentName = _agentName.Trim(), AgentZip = _agentZip.Trim() };
        //    var sql = "Select * from tblName where tblName.NameC like @AgentName and tblName.MailZ like @AgentZip";
        //    try
        //    {
        //        var owner = db.Query<mOwner>(sql, parameters).SingleOrDefault();

        //        if (owner == null)
        //        {
        //            _listOfErrorAgents.Add(("no owner found", _agentName, _agentZip, zip4));
        //            return _agent;
        //        }

        //        _agent.NameID = owner.NameID;
        //        _agent.NameC = owner.NameSort;
        //        return _agent;
        //    }
        //    catch (Exception ex)
        //    {
        //        var owners = db.Query<mOwner>(sql, parameters);
        //        var filteredOwners = owners.Where(o => o.MailZ4 == zip4).ToList();

        //        if (filteredOwners.Count() > 1)
        //        {
        //            _listOfErrorAgents.Add(("multiple owners found", _agentName, _agentZip, zip4));
        //        }

        //        if (filteredOwners.Count > 0)
        //        {
        //            _agent.NameID = filteredOwners[0].NameID;
        //            _agent.NameC = filteredOwners[0].NameSort;
        //        }
        //        return _agent;
        //    }
        //}

        public void ReadMarionAgentsFlatFileIntoMarionAgents()
        {
            //var service = new DataExportService();

            using (StreamReader sr = new StreamReader(Path))
            {
                MarionAgents.Clear();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    mAgent agent = new mAgent();
                    agent = ParseLineFromMarionAgentsFlatFile(line);
                    MarionAgents.Add(agent);
                }
            }
        }

        static mAgent ParseLineFromMarionAgentsFlatFile(string line)
        {
            var data = new mAgent();
            data.AgentNumber = GetString(line, 1, 3);
            data.AgentName = GetString(line, 4, 33);
            data.InCareOf = GetString(line, 34, 63);
            data.StreetAddress = GetString(line, 64, 93);
            data.City = GetString(line, 94, 109);
            data.State = GetString(line, 110, 111);
            data.Zip = GetString(line, 112, 116);
            var last = GetString(line, 118, 121);
            data.Zip4 = last ?? "";
            return data;
        }

        private static string GetString(string line, int pos1, int pos2)
        {
            string val = line.Substring(pos1 - 1, pos2 - pos1 + 1);
            return val;
        }

        private static char GetChar(string line, int pos1)
        {
            return GetString(line, pos1, pos1)[0];
        }

        private static int GetInt(string line, int pos1, int pos2)
        {
            return int.Parse(GetString(line, pos1, pos2));
        }

        private static List<string> GetList(string line, int startPosition, int length, int numberOfValues)
        {
            List<string> results = new List<string>();
            for (int i = 0; i < numberOfValues; i++)
            {
                var nextValue = GetString(line, startPosition + i * length, startPosition + i * length + length - 1);
                results.Add(nextValue);
            }

            return results;
        }

        private static List<char> GetListOfCharacters(string line, int startPosition, int numberOfValues)
        {
            List<char> results = new List<char>();
            for (int i = 0; i < numberOfValues; i++)
            {
                var nextValue = GetChar(line, startPosition + i);
                results.Add(nextValue);
            }
            return results;
        }
    }
}
