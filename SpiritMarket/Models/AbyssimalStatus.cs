using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace SpiritMarket.Models{
    public class AbyssimalStatus{
        [Key]
        public int AbyssimalStatusId {get; set;}

        public int AbyssimalId {get; set;}
        public Abyssimal Abyssimal {get; set;}

        public int StatusId {get; set;}
        public Status Status {get; set;}
    }
}