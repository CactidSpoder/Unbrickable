
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Unbrickable.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Item
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Item()
    {

        this.LinkedItems = new HashSet<LinkedItem>();

    }


    public int id { get; set; }

    public string name { get; set; }

    public string description { get; set; }

    public decimal price { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<LinkedItem> LinkedItems { get; set; }

}

}
