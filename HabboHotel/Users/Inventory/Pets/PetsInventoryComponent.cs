using Plus.HabboHotel.Rooms.AI;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Users.Inventory.Pets
{
    public class PetsInventoryComponent
    {
        private ConcurrentDictionary<int, Pet> _pets;

        public IReadOnlyDictionary<int, Pet> Pets => _pets;

        public PetsInventoryComponent(List<Pet> pets)
        {
            _pets = new(pets.ToDictionary(pet => pet.PetId));
        }

        public bool AddPet(Pet pet) => _pets.TryAdd(pet.PetId, pet);

        public bool RemovePet(int petId) => _pets.TryRemove(petId, out _);
    }
}
