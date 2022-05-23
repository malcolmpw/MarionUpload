using Dapper.Contrib.Extensions;

namespace MarionUpload.Models
{
    [Table("MarionJurisdictionView")]
    public class mMarionJurisdiction
    {
      public string UnitID { get; set; }
      public int PropID { get; set; }
      public float UnitPct { get; set; }
      public bool delflag { get; set; }
      public string UnitName { get; set; }
      public string CadUnitIDText { get; set; }
    }
}
