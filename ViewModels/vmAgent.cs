using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dapper;
using GalaSoft.MvvmLight.Command;
using MarionUpload.Models;

namespace MarionUpload.ViewModels
{
    public class vmAgent
    {
        static readonly string Path = @"C:\Users\malcolm.wardlaw\Desktop\Marion Download\MARION CAD FINAL MINERAL DATA\MA215500.TXT";

        public vmAgent()
        {
            MarionAgents = new ObservableCollection<mAgent>();
            
        }
        public ObservableCollection<mAgent> MarionAgents { get; set; }
        private mAgent Agent { get; set; }
        

        public ICommand CommandImportAgents => new RelayCommand(DelegateImportAgents);

        public void DelegateImportAgents()       // note: following WagApp1, a segment is the same as an OwnerPersonalPropertySegment
        {
            Parse();
        }

        

        public void Parse()
        {
            //var service = new DataExportService();
           

            using (StreamReader sr = new StreamReader(Path))
            {
                MarionAgents.Clear();
                while (!sr.EndOfStream)
                {                
                    string line = sr.ReadLine();
                    mAgent agent = new mAgent(); 
                    agent = ParseLine(line);
                    MarionAgents.Add(agent);
                }                
            }
        }

        static mAgent ParseLine(string line)
        {
            var data = new mAgent();
            data.AgentNumber = GetString(line, 1, 3);
            data.AgentName = GetString(line, 4, 33);
            data.InCareOf = GetString(line, 34, 63);
            data.StreetAddress = GetString(line, 64, 93);
            data.City = GetString(line, 94, 109);
            data.State = GetString(line, 110, 111);
            data.Zip = GetString(line, 112, 116);
            var last = GetString(line, 118, 121) ;            
            data.Zip4 = last??"";
            return data;
        }

        private static string GetString(string line, int pos1, int pos2)
        {
            string val = line.Substring(pos1 - 1, pos2 - pos1 + 1);
            return val;
        }

        //private static List<string> GetList(string line, int startPosition, int length, int numberOfValues)
        //{
        //    List<string> results = new List<string>();
        //    for (int i = 0; i < numberOfValues; i++)
        //    {
        //        var nextValue = GetString(line, startPosition + i * length, startPosition + i * length + length - 1);
        //        results.Add(nextValue);
        //    }

        //    return results;
        //}


        //private static List<char> GetListOfCharacters(string line, int startPosition, int numberOfValues)
        //{
        //    List<char> results = new List<char>();
        //    for (int i = 0; i < numberOfValues; i++)
        //    {
        //        var nextValue = GetChar(line, startPosition + i);
        //        results.Add(nextValue);
        //    }

        //    return results;
        //}

        private static char GetChar(string line, int pos1)
        {
            return GetString(line, pos1, pos1)[0];
        }

        private static int GetInt(string line, int pos1, int pos2)
        {
            return int.Parse(GetString(line, pos1, pos2));
        }
    }
}
