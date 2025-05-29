using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{

    public class CharacterResponse
    {
        public List<Character> Items { get; set; }
        
    }
    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ki { get; set; }
        public string MaxKi { get; set; }
        public string Race { get; set; }
        public string Gender { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Affiliation { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Planet OriginPlanet { get; set; }
        public List<Transformation> Transformations { get; set; }
    }

    public class Planet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDestroyed { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class Transformation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Ki { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

}
