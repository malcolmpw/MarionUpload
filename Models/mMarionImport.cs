namespace MarionUpload.Models
{
    public class mMarionImport
    {
        //[Dapper.Key]

        public int ImportID { get; set; }
        public string Job { get; set; }                      //2 
        public int CurrentTaxYear { get; set; }              //9 
        public char RenderedCode { get; set; }               //14
        public char PropertyType { get; set; }               //16
        public char InterestType { get; set; }               //17
        public int YearLeaseStarted { get; set; }            //18
        public char Protest { get; set; }                    //22
        public string SPTBCode { get; set; }                 //23 3char        
        public string AgentNumber { get; set; }              //26           
        public char SortCode { get; set; }                   //29
        public string DecimalInterest { get; set; }          //34 7
        public string LeaseName { get; set; }                //41 (DESC 3) IF TYPE = 2,3 30 A
        public string OperatorName { get; set; }             //71 90 OPER NAME(DESC 4) 20 A
        public string Description1 { get; set; }             //91 120 DESCRIPTION 1 30 A
        public string Description2 { get; set; }             //121 150 DESCRIPTION 2 30 A
        public string OwnerName { get; set; }                //151 180 OWNER NAME 30 A
        public string InCareOf { get; set; }                 //181 210 IN CARE OF 30 A
        public string StreetAddress { get; set; }            //211 240 STREET ADDRESS 30 A

        public string CityStateZip { get; set; }             //241 270 CITY,ST,ZIP CDE OR OUT COUNTRY 30 A
        public string RRC { get; set; }                      //271 290 RRC# 20 A

        //public List<string> Jurisdictions { get; set; }

        public string Jurisdiction1 { get; set; }            //297 298 JURISDICTION 1 2 0 N
        public string Jurisdiction2 { get; set; }            //299 298 JURISDICTION 2 2 0 N
        public string Jurisdiction3 { get; set; }            //301 298 JURISDICTION 3 2 0 N
        public string Jurisdiction4 { get; set; }            //303 298 JURISDICTION 4 2 0 N
        public string Jurisdiction5 { get; set; }            //305 298 JURISDICTION 5 2 0 N
        public string Jurisdiction6 { get; set; }            //307 298 JURISDICTION 6 2 0 N
        public string Jurisdiction7 { get; set; }            //309 298 JURISDICTION 7 2 0 N
        public string Jurisdiction8 { get; set; }            //311 298 JURISDICTION 8 2 0 N
        public string Jurisdiction9 { get; set; }            //313 298 JURISDICTION 9 2 0 N
        public string Jurisdiction10 { get; set; }           //315 298 JURISDICTION 10 2 0 N
        public string Jurisdiction11 { get; set; }           //317 318 JURISDICTION 11 2 0 N
        public string Jurisdiction12 { get; set; }

        public float acres { get; set; }
        public int OwnerNumber { get; set; }
        public int LeaseNumber { get; set; }

        public string ExemptionMinOwnerFlagJuris1 { get; set; }
        public string ExemptionMinOwnerFlagJuris2 { get; set; }
        public string ExemptionMinOwnerFlagJuris3 { get; set; }
        public string ExemptionMinOwnerFlagJuris4 { get; set; }
        public string ExemptionMinOwnerFlagJuris5 { get; set; }
        public string ExemptionMinOwnerFlagJuris6 { get; set; }
        public string ExemptionMinOwnerFlagJuris7 { get; set; }
        public string ExemptionMinOwnerFlagJuris8 { get; set; }
        public string ExemptionMinOwnerFlagJuris9 { get; set; }
        public string ExemptionMinOwnerFlagJuris10 { get; set; }
        public string ExemptionMinOwnerFlagJuris11 { get; set; }
        public string ExemptionMinOwnerFlagJuris12 { get; set; }

        public string CustomerGeo { get; set; }

        public string PnAMinOwnerFlagJuris1 { get; set; }
        public string PnAMinOwnerFlagJuris2 { get; set; }
        public string PnAMinOwnerFlagJuris3 { get; set; }
        public string PnAMinOwnerFlagJuris4 { get; set; }
        public string PnAMinOwnerFlagJuris5 { get; set; }
        public string PnAMinOwnerFlagJuris6 { get; set; }
        public string PnAMinOwnerFlagJuris7 { get; set; }
        public string PnAMinOwnerFlagJuris8 { get; set; }
        public string PnAMinOwnerFlagJuris9 { get; set; }
        public string PnAMinOwnerFlagJuris10 { get; set; }
        public string PnAMinOwnerFlagJuris11 { get; set; }
        public string PnAMinOwnerFlagJuris12 { get; set; }

        public int MineralAccountNumber { get; set; }
        public int MineralAccountSequence { get; set; }
        public int PreviousMineralAccountSequence { get; set; }
        public int PreviousMineralAccount { get; set; }

        public string TaxableValueNewJuris1 { get; set; }
        public string TaxableValueNewJuris2 { get; set; }
        public string TaxableValueNewJuris3 { get; set; }
        public string TaxableValueNewJuris4 { get; set; }
        public string TaxableValueNewJuris5 { get; set; }
        public string TaxableValueNewJuris6 { get; set; }
        public string TaxableValueNewJuris7 { get; set; }
        public string TaxableValueNewJuris8 { get; set; }
        public string TaxableValueNewJuris9 { get; set; }
        public string TaxableValueNewJuris10 { get; set; }
        public string TaxableValueNewJuris11 { get; set; }
        public string TaxableValueNewJuris12 { get; set; }

        public decimal Abt1NewAbtValue { get; set; }
        public decimal Abt2NewAbtValue { get; set; }
        public decimal Abt3NewAbtValue { get; set; }
        public decimal Abt4NewAbtValue { get; set; }
        public decimal Abt5NewAbtValue { get; set; }
        public decimal Abt6NewAbtValue { get; set; }
        public decimal Abt7NewAbtValue { get; set; }
        public decimal Abt8NewAbtValue { get; set; }
        public decimal Abt9NewAbtValue { get; set; }
        public decimal Abt10NewAbtValue { get; set; }
        public decimal Abt11NewAbtValue { get; set; }
        public decimal Abt12NewAbtValue { get; set; }

        public string NewTCEQflag { get; set; }

        public decimal NewExemptValueJuris1 { get; set; }
        public decimal NewExemptValueJuris2 { get; set; }
        public decimal NewExemptValueJuris3 { get; set; }
        public decimal NewExemptValueJuris4 { get; set; }
        public decimal NewExemptValueJuris5 { get; set; }
        public decimal NewExemptValueJuris6 { get; set; }
        public decimal NewExemptValueJuris7 { get; set; }
        public decimal NewExemptValueJuris8 { get; set; }
        public decimal NewExemptValueJuris9 { get; set; }
        public decimal NewExemptValueJuris10 { get; set; }
        public decimal NewExemptValueJuris11 { get; set; }
        public decimal NewExemptValueJuris12 { get; set; }

        public decimal NewImprovementNumber { get; set; }

    }
}
