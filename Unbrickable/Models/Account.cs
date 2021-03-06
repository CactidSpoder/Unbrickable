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
    
    public partial class Account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Account()
        {
            this.Posts = new HashSet<Post>();
            this.EditedPosts = new HashSet<Post>();
            this.Transactions = new HashSet<Transaction>();
        }
    
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int gender_id { get; set; }
        public int salutation_id { get; set; }
        public System.DateTime birthdate { get; set; }
        public string username { get; set; }
        public byte[] password { get; set; }
        public string about_me { get; set; }
        public int access_level_id { get; set; }
        public System.DateTime date_joined { get; set; }
        public decimal donation_total { get; set; }
    
        public virtual AccessLevel AccessLevel { get; set; }
        public virtual Gender Gender { get; set; }
        public virtual Salutation Salutation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Post> Posts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Post> EditedPosts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
