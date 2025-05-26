using System.Collections.Generic;

namespace SerializationTests.Models
{
    public class OwnerProperties
    {
        public Pet Favorite { get; set; }

        public IList<Pet> Pets { get; set; }
    }
}
