//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MusicForNow.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Album
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Album()
        {
            this.Songs = new HashSet<Song>();
        }
    
        [Display(Name = "Album ID")]
        public string Album_ID { get; set; }
        [Display(Name = "Title")]
        public string Album_Title { get; set; }
        [Display(Name = "Release Date")]
        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public string Artist { get; set; }
        [Display(Name = "Songs")]
        public string List_Of_Songs { get; set; }
        public byte[] Image { get; set; }
    
        public virtual Artist Artist1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Song> Songs { get; set; }
    }
}
