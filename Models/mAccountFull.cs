using Dapper.Contrib.Extensions;
using System;

namespace MarionUpload.Models
{
    [Table("tblAccount")]
    public class mAccountFull
    {
        [Key]
        public int AcctID { get; set; }
        public string CadParcelId { get; set; }
        public long PropID { get; internal set; }
        public string CadRealID { get; set; }
        public long NameID { get; internal set; }
        public string AcctLegal { get; set; }
        public string ChgCode { get; set; }
        public string SeqNmbr { get; set; }
        public float PctProp { get; set; }
        public char PctType { get; set; }
        public decimal ValImpAppr { get; set; }
        public decimal ValImpHsNo { get; set; }
        public decimal ValImpHsYes { get; set; }
        public decimal ValLandAppr { get; set; }
        public decimal ValLandHsNo { get; set; }
        public decimal ValLandHsYes { get; set; }
        public decimal ValLandMkt { get; set; }
        public decimal ValLandAg { get; set; }
        public decimal ValPrsnlAppr { get; set; }
        public decimal ValPrsnlHsNo { get; set; }
        public decimal ValPrsnlHsYes { get; set; }
        public decimal ValMnrlCalc { get; set; }
        public decimal ValMnrlAppr { get; set; }
        public decimal ValNpmAppr { get; set; }
        public decimal ValAcctCur { get; set; }
        public decimal ValAcctNtc { get; set; }
        public bool ValAcctLock { get; set; }
        public decimal ValueLocked { get; set; }
        public DateTime NtcDate { get; set; }
        public DateTime LastProtDate { get; set; }
        public decimal ValAcctCrt { get; set; }
        public decimal valacctPrYr { get; set; }
        public decimal AcctValPrYr { get; set; }
        public decimal valacctPrYr5 { get; set; }
        public DateTime CrtDate { get; set; }
        public bool Stat_YN { get; set; }
        public bool Protest_YN { get; set; }
        public bool ProtestResolved_YN { get; set; }
        public bool Supp_YN { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public string Memo { get; set; }
        public string ProtStat { get; set; }
        public DateTime ProtStatDate { get; set; }
        public DateTime ProtDate { get; set; }
        public string ProtCause { get; set; }
        public int CrtStat { get; set; }
        public int ResStat { get; set; }
        public DateTime ResStateDate { get; set; }
        public bool BatchProtest_YN { get; set; }
        public bool BatchWithdraw_YN { get; set; }
        public int ConveyTransactionNo { get; set; }
        public bool Prd { get; set; }
        public string crtNote { get; set; }
        public string Cad { get; set; }
        public string ValidationNote { get; set; }
        public bool delflag { get; set; }
        public string PTDcode { get; set; }
        public string GeoRef { get; set; }
        public bool corr_yn { get; set; }
        public decimal AcctValPryr5 { get; set; }
        public char division { get; set; }
    }
}
