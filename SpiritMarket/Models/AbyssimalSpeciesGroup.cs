using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace SpiritMarket.Models{
    public class AbyssimalSpeciesGroup{
        public int AbyssimalSpeciesId {get; set;}
        public AbyssimalSpecies AbyssimalSpecies {get; set;}

        public int AbyssimalGroupId {get; set;}
        public AbyssimalGroup AbyssimalGroup {get; set;}
    }
}